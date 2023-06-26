using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class MasterGrid : MonoBehaviour
{
    //overarching manager for the grid system in the map
    //test grid stores all the interacting grids and updates them
    //this allows for the grid to reuse code and reduce update

    private MapData mapData;                //reference to map data
    TimeController timeController;          //reference to time

    public InventoryManager inventoryManager;      //sibling
    public NPCController npcController;            //sibling

    //ground renderer
    private Mesh mesh;
    private MeshRenderer meshRenderer;

    //child object, overlay renderer
    private Mesh overlayMesh;
    private MeshRenderer overlayMeshRenderer;
    private int renderLayer = 0;            //0 for dont render, 1++ according to following arrays

    public BaseGrid<double> arrayHeat = new BaseGrid<double>();
    public BaseGrid<double> arrayRadiation = new BaseGrid<double>();
    public BaseGrid<double> arrayOxygen = new BaseGrid<double>();

    public BaseGrid<PathNode> pathfindingGrid = new BaseGrid<PathNode>();
    //stores data for access levels, walkability and weighted paths
    private List<PathNode> openList;                //list to allow for data manipulation
    private HashSet<PathNode> closedList;           //generic hashset to check if it contains neighbour
    private const int MOVE_STRAIGHT_COST = 10;
    private const int MOVE_DIAGONAL_COST = 14;              //sqrt of 10+10

    [Serializable]
    public struct TileMapSpriteUV
    {
        public TileMapObject.TileType tileType;
        public Vector2Int uv00Pixel;
        public Vector2Int uv11Pixel;
    }

    private struct UVCoords
    {
        public Vector2 uv00;
        public Vector2 uv11;
    }
    public BaseGrid<TileMapObject> tilemapGrid = new BaseGrid<TileMapObject>();
    [SerializeField] TileMapSpriteUV[] tileMapSpriteUVArray;
    private Dictionary<TileMapObject.TileType, UVCoords> dictUVCoords;

    //able to store prefabs as well as construction blueprints
    public BaseGrid<GameObject> structureArray = new BaseGrid<GameObject>();
    public BaseGrid<GameObject> floorArray = new BaseGrid<GameObject>();
    public BaseGrid<GameObject> ceilingArray = new BaseGrid<GameObject>();

    void Awake()
    {
        mapData = FindObjectOfType<MapData>();
        timeController = FindObjectOfType<TimeController>();

        inventoryManager = transform.parent.Find("Inventory Manager").gameObject.GetComponent<InventoryManager>();
        npcController = transform.parent.Find("Inventory Manager").gameObject.GetComponent<NPCController>();

        //tile mesh
        meshRenderer = GetComponent<MeshRenderer>();
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        Texture texture = GetComponent<MeshRenderer>().material.mainTexture;
        float tWidth = texture.width; 
        float tHeight = texture.height; 
        dictUVCoords = new Dictionary<TileMapObject.TileType, UVCoords>();
        foreach (TileMapSpriteUV spriteUV in tileMapSpriteUVArray)
        {
            dictUVCoords[spriteUV.tileType] = new UVCoords {
                uv00 = new Vector2(spriteUV.uv00Pixel.x / tWidth, spriteUV.uv00Pixel.y / tHeight),
                uv11 = new Vector2(spriteUV.uv11Pixel.x / tWidth, spriteUV.uv11Pixel.y / tHeight),
            };
        }

        //overlay renderer
        tilemapGrid.generateGrid(mapData, (tileMap, x, y) => new TileMapObject(tileMap, x, y));
        overlayMeshRenderer = transform.GetChild(0).GetComponent<MeshRenderer>();
        overlayMesh = new Mesh();
        transform.GetChild(0).GetComponent<MeshFilter>().mesh = overlayMesh;

        //create grids with data type
        arrayHeat.generateGrid(mapData, (arrayHeat, x, y) => -300);
        arrayRadiation.generateGrid(mapData, (arrayHeat, x, y) => 0);
        arrayOxygen.generateGrid(mapData, (arrayOxygen, x, y) => 0);
        pathfindingGrid.generateGrid(mapData, (pathfindingGrid, x, y) => new PathNode(pathfindingGrid, x, y));
    }

    public List<Vector3> findVectorPath(Vector3 startPos, Vector3 endPos, bool canFly)
    {
        List<PathNode> path = findPath(pathfindingGrid.getGridObject(startPos), pathfindingGrid.getGridObject(endPos), canFly);
        if (path != null)
        {
            List<Vector3> vectorPath = new List<Vector3>();
            foreach (PathNode node in path)     //convert path into world coords, center of grid
            {
                vectorPath.Add(pathfindingGrid.getWorldPosCenter(node.x, node.y));
                for (int i = 0; i < path.Count - 1; i++)
                {
                    Debug.DrawLine(pathfindingGrid.getWorldPosCenter(path[i].x, path[i].y),
                                   pathfindingGrid.getWorldPosCenter(path[i + 1].x, path[i + 1].y),
                                   Color.green);
                }
            }
            return vectorPath;
        }
        return null;        //path is null
    }
    public List<PathNode> findPath(PathNode startnode, PathNode endnode, bool canFly)
    {
        if (startnode == null || endnode == null || startnode.isSolid || endnode.isSolid) return null;
        if ((startnode.isSpace || endnode.isSpace) && !canFly) return null;                                //if in space and cannot fly, then break
        //prevents if location nodes are outside the map, or if the end node is solid
        //add another check for access array

        openList = new List<PathNode> { startnode };
        closedList = new HashSet<PathNode>();

        //cycle through all nodes and set values
        for (int x = 0; x < mapData.getWidth(); x++)
        {
            for (int y = 0; y < mapData.getHeight(); y++)
            {
                PathNode node = pathfindingGrid.getGridObject(x, y);
                node.costG = int.MaxValue;          //set to infinite value
                node.calculateCostF();
                node.setPrevNode(null);
            }
        }

        startnode.costG = 0;        //starting node, current cost = 0
        startnode.costH = calculateDistance(startnode, endnode);
        startnode.calculateCostF();

        while (openList.Count > 0)
        {
            PathNode currNode = getLowestFCostNode(openList);
            if (currNode == endnode) return calculatePath(endnode);     //found path
            openList.Remove(currNode); closedList.Add(currNode);

            foreach (PathNode neighbourNode in getNeighbours(currNode))
            {
                if (closedList.Contains(neighbourNode)) continue;
                //if node is solid, or is in space and cannot fly, then ignore the node and add it to closed list
                if (neighbourNode.isSolid || (neighbourNode.isSpace && !canFly)) { closedList.Add(neighbourNode); continue; }
                //diagonal movement check;
                if (calculateDistance(currNode, neighbourNode) == MOVE_DIAGONAL_COST)           //check if this is diagonal
                {
                    int dirX = neighbourNode.x - currNode.x;
                    int dirY = neighbourNode.y - currNode.y;
                    if (pathfindingGrid.getGridObject(currNode.x + dirX, currNode.y).isSolid &&
                        pathfindingGrid.getGridObject(currNode.x, currNode.y + dirY).isSolid) continue;     //ignore
                }

                int tempCostG = currNode.costG + calculateDistance(currNode, neighbourNode) + neighbourNode.costPenalty;
                if (tempCostG < neighbourNode.costG)
                {
                    neighbourNode.setPrevNode(currNode);
                    neighbourNode.costG = tempCostG;
                    neighbourNode.costH = calculateDistance(neighbourNode, endnode);
                    neighbourNode.calculateCostF();

                    if (!openList.Contains(neighbourNode)) openList.Add(neighbourNode);
                }
            }
        }
        //out of nodes on open list, but path hasnt been found
        return null;
    }
    private int calculateDistance(PathNode a, PathNode b)
    {               //distance ignoring obstructions
        int distX = Mathf.Abs(a.x - b.x);
        int distY = Mathf.Abs(a.y - b.y);
        int remaining = Mathf.Abs(distX - distY);
        return MOVE_DIAGONAL_COST * Mathf.Min(distX, distY) +
               MOVE_STRAIGHT_COST * remaining;
    }
    private PathNode getLowestFCostNode(List<PathNode> pathNodeList)
    {
        PathNode lowestFCostNode = pathNodeList[0];
        for (int i = 1; i < pathNodeList.Count; i++)
        {
            if (pathNodeList[i].costF < lowestFCostNode.costF) 
                lowestFCostNode = pathNodeList[i];
        }
        return lowestFCostNode;
    }
    private List<PathNode> calculatePath(PathNode endNode)
    {
        //retracing steps
        List<PathNode> path = new List<PathNode>();
        path.Add(endNode);
        PathNode currNode = endNode;
        while (currNode.getPrevNode() != null)
        {
            //cycle through all the nodes and find its "parent"
            //until it reaches a node with no parent, aka the start node
            path.Add(currNode.getPrevNode());
            currNode = currNode.getPrevNode();
        }
        path.Reverse();         //since the first value is the end, need to reverse it. 
        return path;
    }
    private List<PathNode> getNeighbours(PathNode currNode)
    {
        List<PathNode> Neighbours = new List<PathNode>();
        for (int x = -1; x <= 1; ++x)
        {
            for (int y = -1; y <= 1; ++y)
            {
                if (pathfindingGrid.checkValid(currNode.x + x, currNode.y + y))
                {           //ensure that the node is a neighbour and not on edge
                    if (x == 0 && y == 0) continue;                                         //ensure node is not itself
                    Neighbours.Add(pathfindingGrid.getGridObject(currNode.x + x, currNode.y + y));
                }
            }
        }
        return Neighbours;
    }

    private int getTotalCost(PathNode startnode, PathNode endnode, bool canFly)
    {
        if (findPath(startnode, endnode, canFly) != null) return endnode.costG;
        //path is null
        return int.MaxValue;
        //the last node's costG, should be the total cost of the journey
    }

    void Update()
    {
        //diffuse heat
        if (Time.timeScale > 0 && arrayHeat.getRebuild())
        {
            arrayHeat.setRebuild(diffuse(arrayHeat));
        }
        if (Time.timeScale > 0 && arrayRadiation.getRebuild())
        {
            arrayRadiation.setRebuild(diffuse(arrayRadiation));
        }
        if (Time.timeScale > 0 && arrayOxygen.getRebuild())
        {
            arrayOxygen.setRebuild(diffuse(arrayOxygen));
        }

        //tilemapGrid
        //always render the base tile mesh, only rebuild when updated
        if (tilemapGrid.getRebuild())
        {
            updateMeshVisual(tilemapGrid);
        }
        //only rebuild mesh if both render and rebuild is true
        switch (renderLayer)
        {
            case 1:
                //heat visual
                if (arrayHeat.getRebuild())
                {
                    updateMeshVisual(arrayHeat);
                }
                break;
            case 2:
                //access visual
                if (pathfindingGrid.getRebuild())
                {
                    updateMeshVisual(pathfindingGrid);
                    pathfindingGrid.setRebuild(false);
                }
                break;
            case 3:
                //radiation visual
                if (arrayRadiation.getRebuild()) 
                {
                    updateMeshVisual(arrayRadiation);
                }
                break;
            case 4:
                //oxygen visual
                if (arrayOxygen.getRebuild())
                {
                    updateMeshVisual(arrayOxygen);
                }
                break;
            default:
                //case 0, nothing
                break;
        }

    }

    //tile map, base render
    public void updateMeshVisual(BaseGrid<TileMapObject> arrayType)
    {
        CreateEmptyMeshData(mapData.getWidth() * mapData.getHeight(),
            out Vector3[] vertices, out Vector2[] uv, out int[] triangles);

        for (int x = 0; x < mapData.getWidth(); x++)
        {
            for (int y = 0; y < mapData.getHeight(); y++)
            {
                int index = x * mapData.getHeight() + y;
                Vector3 quadSize = new Vector3(1, 1) * mapData.getCellSize();

                TileMapObject gridObject = tilemapGrid.getGridObject(x, y);
                TileMapObject.TileType tileType = gridObject.getTileType();
                Vector2 UV00 = Vector2.zero, UV11 = Vector2.zero;
                if (tileType == TileMapObject.TileType.None)
                {
                    quadSize = Vector3.zero;
                }
                else
                {
                    UVCoords uvCoords = dictUVCoords[tileType];
                    UV00 = uvCoords.uv00;
                    UV11 = uvCoords.uv11;
                }
                AddQuad(vertices, uv, triangles, index, tilemapGrid.getWorldPos(x, y) + 0.5f * quadSize, quadSize, UV00, UV11);

            }
        }
        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
    }

    //debug
    public static TextMesh createWorldText(string text = "null", Transform parent = null, Vector3 localPosition = default(Vector3), int sortingOrder = 0)
    {
        GameObject worldTextObject = new GameObject("Debug_Text", typeof(TextMesh));
        Transform transform = worldTextObject.transform;
        transform.SetParent(parent);
        transform.localPosition = localPosition;
        TextMesh textMesh = worldTextObject.GetComponent<TextMesh>();
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.text = text;
        textMesh.fontSize = 10;
        textMesh.color = Color.white;
        textMesh.GetComponent<MeshRenderer>().sortingOrder = sortingOrder;
        return textMesh;
    }

    //overlay
    public void updateMeshVisual(BaseGrid<int> arrayType)
    {
        CreateEmptyMeshData(mapData.getWidth() * mapData.getHeight(),
            out Vector3[] vertices, out Vector2[] uv, out int[] triangles);

        for (int x = 0; x < mapData.getWidth(); x++)
        {
            for (int y = 0; y < mapData.getHeight(); y++)
            {
                int index = x * mapData.getHeight() + y;
                Vector3 quadSize = new Vector3(1, 1) * mapData.getCellSize();

                //if (arrayType == arrayHeat)
                //{
                //    float gridValue = arrayType.getGridObject(x, y);
                //    float gridValueNormalized = (gridValue - mapData.getMinTemp()) / (mapData.getMaxTemp() - mapData.getMinTemp());
                //    Vector2 gridValueUV = new Vector2(gridValueNormalized, 0f);
                //    AddQuad(vertices, uv, triangles, index, arrayType.getWorldPos(x, y) + 0.5f * quadSize, quadSize, gridValueUV);
                //}
            }
        }
        overlayMesh.vertices = vertices;
        overlayMesh.uv = uv;
        overlayMesh.triangles = triangles;
    }
    public void updateMeshVisual(BaseGrid<float> arrayType)
    {
        CreateEmptyMeshData(mapData.getWidth() * mapData.getHeight(),
            out Vector3[] vertices, out Vector2[] uv, out int[] triangles);

        for (int x = 0; x < mapData.getWidth(); x++)
        {
            for (int y = 0; y < mapData.getHeight(); y++)
            {
                int index = x * mapData.getHeight() + y;
                Vector3 quadSize = new Vector3(1, 1) * mapData.getCellSize();

                //if (arrayType == arrayHeat)
                //{
                //    float gridValue = arrayType.getGridObject(x, y);
                //    float gridValueNormalized = (gridValue - mapData.getMinTemp()) / (mapData.getMaxTemp() - mapData.getMinTemp());
                //    Vector2 gridValueUV = new Vector2(gridValueNormalized, 0f);
                //    AddQuad(vertices, uv, triangles, index, arrayType.getWorldPos(x, y) + 0.5f * quadSize, quadSize, gridValueUV);
                //}
            }
        }
        overlayMesh.vertices = vertices;
        overlayMesh.uv = uv;
        overlayMesh.triangles = triangles;
    }
    public void updateMeshVisual(BaseGrid<double> arrayType)
    {
        CreateEmptyMeshData(mapData.getWidth() * mapData.getHeight(),
            out Vector3[] vertices, out Vector2[] uv, out int[] triangles);

        for (int x = 0; x < mapData.getWidth(); x++)
        {
            for (int y = 0; y < mapData.getHeight(); y++)
            {
                int index = x * mapData.getHeight() + y;
                Vector3 quadSize = new Vector3(1, 1) * mapData.getCellSize();

                if (arrayType == arrayHeat)
                {
                    double gridValue = arrayType.getGridObject(x, y);
                    double gridValueNormalized = (gridValue - mapData.getMinTemp()) / (mapData.getMaxTemp() - mapData.getMinTemp());
                    Vector2 gridValueUV = new Vector2((float)gridValueNormalized, 0);
                    AddQuad(vertices, uv, triangles, index, arrayType.getWorldPos(x, y) + 0.5f * quadSize, quadSize, gridValueUV);
                }
                else if (arrayType == arrayRadiation || arrayType == arrayOxygen)
                {
                    Vector2 gridValueUV = new Vector2((float)arrayType.getGridObject(x, y) / 100, 0);       //percentage
                    AddQuad(vertices, uv, triangles, index, arrayType.getWorldPos(x, y) + 0.5f * quadSize, quadSize, gridValueUV);
                }
            }
        }
        overlayMesh.vertices = vertices;
        overlayMesh.uv = uv;
        overlayMesh.triangles = triangles;
    }
    public void updateMeshVisual(BaseGrid<PathNode> arrayType)
    {
        CreateEmptyMeshData(mapData.getWidth() * mapData.getHeight(),
            out Vector3[] vertices, out Vector2[] uv, out int[] triangles);

        for (int x = 0; x < mapData.getWidth(); x++)
        {
            for (int y = 0; y < mapData.getHeight(); y++)
            {
                int index = x * mapData.getHeight() + y;
                Vector3 quadSize = new Vector3(1, 1) * mapData.getCellSize();

                if (arrayType == pathfindingGrid)
                {
                    bool isSolid = pathfindingGrid.getGridObject(x, y).isSolid;
                    if (!isSolid)
                    {
                        //walkable
                        int access = pathfindingGrid.getGridObject(x, y).accessLayer;
                        Vector2 gridValueUV = new Vector2(access / 7f, 0f);
                        AddQuad(vertices, uv, triangles, index, pathfindingGrid.getWorldPos(x, y) + 0.5f * quadSize, quadSize, gridValueUV);
                    }
                    else
                    {
                        Vector2 gridValueUV = new Vector2(0, 0f);           //if not walkable then just black
                        AddQuad(vertices, uv, triangles, index, pathfindingGrid.getWorldPos(x, y) + 0.5f * quadSize, quadSize, gridValueUV);
                    }

                }

            }
        }
        overlayMesh.vertices = vertices;
        overlayMesh.uv = uv;
        overlayMesh.triangles = triangles;
    }

    private void AddQuad(Vector3[] vertices, Vector2[] uvs, int[] triangles, int index, Vector3 GridPos, Vector3 QuadSize, Vector2 uv)
    {
        vertices[index * 4] = new Vector3((-0.5f + GridPos.x) * QuadSize.x, (-0.5f + GridPos.y) * QuadSize.y);
        vertices[(index * 4) + 1] = new Vector3((-0.5f + GridPos.x) * QuadSize.x, (+0.5f + GridPos.y) * QuadSize.y);
        vertices[(index * 4) + 2] = new Vector3((+0.5f + GridPos.x) * QuadSize.x, (+0.5f + GridPos.y) * QuadSize.y);
        vertices[(index * 4) + 3] = new Vector3((+0.5f + GridPos.x) * QuadSize.x, (-0.5f + GridPos.y) * QuadSize.y);

        uvs[(index * 4)] = uv;
        uvs[(index * 4) + 1] = uv;
        uvs[(index * 4) + 2] = uv;
        uvs[(index * 4) + 3] = uv;

        triangles[(index * 6) + 0] = (index * 4) + 0;
        triangles[(index * 6) + 1] = (index * 4) + 1;
        triangles[(index * 6) + 2] = (index * 4) + 2;
        triangles[(index * 6) + 3] = (index * 4) + 2;
        triangles[(index * 6) + 4] = (index * 4) + 3;
        triangles[(index * 6) + 5] = (index * 4) + 0;
    }
    private void AddQuad(Vector3[] vertices, Vector2[] uvs, int[] triangles, int index, Vector3 GridPos, Vector3 QuadSize, Vector2 uv00, Vector2 uv11)
    {
        //overloaded function to show texture like a sprite atlas
        vertices[index * 4] = new Vector3((-0.5f + GridPos.x) * QuadSize.x, (-0.5f + GridPos.y) * QuadSize.y);
        vertices[(index * 4) + 1] = new Vector3((-0.5f + GridPos.x) * QuadSize.x, (+0.5f + GridPos.y) * QuadSize.y);
        vertices[(index * 4) + 2] = new Vector3((+0.5f + GridPos.x) * QuadSize.x, (+0.5f + GridPos.y) * QuadSize.y);
        vertices[(index * 4) + 3] = new Vector3((+0.5f + GridPos.x) * QuadSize.x, (-0.5f + GridPos.y) * QuadSize.y);

        uvs[(index * 4)] = new Vector2(uv00.x, uv11.y);
        uvs[(index * 4) + 1] = new Vector2(uv00.x, uv00.y);
        uvs[(index * 4) + 2] = new Vector2(uv11.x, uv00.y);
        uvs[(index * 4) + 3] = new Vector2(uv11.x, uv11.y);

        triangles[(index * 6) + 0] = (index * 4) + 0;
        triangles[(index * 6) + 1] = (index * 4) + 1;
        triangles[(index * 6) + 2] = (index * 4) + 2;
        triangles[(index * 6) + 3] = (index * 4) + 2;
        triangles[(index * 6) + 4] = (index * 4) + 3;
        triangles[(index * 6) + 5] = (index * 4) + 0;
    }
    private void CreateEmptyMeshData(int quadCount, out Vector3[] vertices, out Vector2[] uvs, out int[] triangles)
    {
        vertices = new Vector3[quadCount * 4];
        uvs = new Vector2[quadCount * 4];
        triangles = new int[quadCount * 6];
    }

    //toggle
    public int getRenderLayer() { return renderLayer; }
    public void toggleHeat()
    {
        if (renderLayer == 1)
        {
            overlayMeshRenderer.enabled = false;
            renderLayer = 0;
            //for (int x = 0; x < mapData.getWidth(); ++x)
            //{
            //    for (int y = 0; y < mapData.getHeight(); ++y)
            //    {
            //        debugTextArray[x, y].gameObject.SetActive(false);
            //    }
            //}
        }
        else
        {
            overlayMeshRenderer.enabled = true;
            overlayMeshRenderer.material = Resources.Load<Material>("Overlay/HeatGradient");
            updateMeshVisual(arrayHeat);
            renderLayer = 1;
            //for (int x = 0; x < mapData.getWidth(); ++x)
            //{
            //    for (int y = 0; y < mapData.getHeight(); ++y)
            //    {
            //        debugTextArray[x, y].gameObject.SetActive(true);
            //        debugTextArray[x, y].text = ((int)arrayHeat.getGridObject(x, y)).ToString();
            //    }
            //}
        }
    }
    public void toggleAccess()
    {
        if (renderLayer == 2)
        {
            overlayMeshRenderer.enabled = false;
            renderLayer = 0;
            //for (int x = 0; x < mapData.getWidth(); ++x)
            //{
            //    for (int y = 0; y < mapData.getHeight(); ++y)
            //    {
            //        debugTextArray[x, y].gameObject.SetActive(false);
            //    }
            //}
        }
        else
        {
            overlayMeshRenderer.enabled = true;
            overlayMeshRenderer.material = Resources.Load<Material>("Overlay/Access");
            updateMeshVisual(pathfindingGrid);                                  //requires update
            renderLayer = 2;
            //for (int x = 0; x < mapData.getWidth(); ++x)
            //{
            //    for (int y = 0; y < mapData.getHeight(); ++y)
            //    {
            //        debugTextArray[x, y].gameObject.SetActive(false);
            //        debugTextArray[x, y].text = pathfindingGrid.getGridObject(x, y).ToString();
            //    }
            //}
        }
    }
    public void toggleRadiation()
    {
        if (renderLayer == 3)
        {
            overlayMeshRenderer.enabled = false;
            renderLayer = 0;
            //for (int x = 0; x < mapData.getWidth(); ++x)
            //{
            //    for (int y = 0; y < mapData.getHeight(); ++y)
            //    {
            //        debugTextArray[x, y].gameObject.SetActive(false);
            //    }
            //}
        }
        else
        {
            overlayMeshRenderer.enabled = true;
            overlayMeshRenderer.material = Resources.Load<Material>("Overlay/RadiationGradient");
            updateMeshVisual(arrayRadiation);                                  //requires update
            renderLayer = 3;
            //for (int x = 0; x < mapData.getWidth(); ++x)
            //{
            //    for (int y = 0; y < mapData.getHeight(); ++y)
            //    {
            //        debugTextArray[x, y].gameObject.SetActive(false);
            //        debugTextArray[x, y].text = pathfindingGrid.getGridObject(x, y).ToString();
            //    }
            //}
        }
    }
    public void toggleOxygen()
    {
        if (renderLayer == 4)
        {
            overlayMeshRenderer.enabled = false;
            renderLayer = 0;
            //for (int x = 0; x < mapData.getWidth(); ++x)
            //{
            //    for (int y = 0; y < mapData.getHeight(); ++y)
            //    {
            //        debugTextArray[x, y].gameObject.SetActive(false);
            //    }
            //}
        }
        else
        {
            overlayMeshRenderer.enabled = true;
            overlayMeshRenderer.material = Resources.Load<Material>("Overlay/OxygenGradient");
            updateMeshVisual(arrayOxygen);                                  //requires update
            renderLayer = 4;
            //for (int x = 0; x < mapData.getWidth(); ++x)
            //{
            //    for (int y = 0; y < mapData.getHeight(); ++y)
            //    {
            //        debugTextArray[x, y].gameObject.SetActive(false);
            //        debugTextArray[x, y].text = pathfindingGrid.getGridObject(x, y).ToString();
            //    }
            //}
        }
    }

    //calculation
    public void floodrandom()
    {
        //theoritical usage of breadth first search, A* for searching
        //https://www.geeksforgeeks.org/flood-fill-algorithm/ 
        //create a list and a hash set for the values

        int tileCount = 0;
        int totalTileCount = mapData.getWidth() * mapData.getHeight();

        int setRandAccess = 0;
        //set all walls to 0, add the rest into the open list

        while (true)
        {
            setRandAccess++;
            //choose a random point to start
            while (true)            //condition
            {
                //loop and add

                //flood fill from point 1

                tileCount++;
            }

            //if (tileCount >= totalTileCount) break;             //only exit when the number of accesses, or assignment is more than the total number of cells
        }
        //do stuff here
    }

    private bool diffuse(BaseGrid<double> arrayType)
    {
        bool updated = false;

        for (int x = 0; x < mapData.getWidth(); ++x)
        {
            for (int y = 0; y < mapData.getHeight(); ++y)
            {
                double self = arrayType.getGridObject(x, y);
                for (int nX = -1; nX <= 1; ++nX)
                {
                    for (int nY = -1; nY <= 1; ++nY)
                    {
                        if (arrayType.checkValid(x + nX, y + nY))
                        {           //ensure that the node is a neighbour and not on edge
                            if (x == 0 && y == 0) continue;                                         //ensure node is not itself
                            double neighbour = arrayType.getGridObject(x + nX, y + nY);
                            double diff = self - neighbour;
                            if (diff > 0.5)
                            {
                                updated = true;
                                //assumes distribution of 2%                                        //get insulation values afterwards
                                diff /= 50;
                                self -= diff * Time.timeScale;
                                neighbour += diff * Time.timeScale;
                                arrayType.setGridObject(x + nX, y + nY, neighbour);
                                arrayType.setGridObject(x, y, self);
                            }
                        }
                    }
                }
            }
        }
        return updated;
    }

    //inventory
    public ItemStat findNearest(int xCoord, int yCoord, string name, bool canFly)           //brute force
    {
        ItemStat nearestItem = null;                //comparison pointer
        int cost = int.MaxValue;                    //max cost to compare smaller values

        PathNode startNode = pathfindingGrid.getGridObject(xCoord, yCoord);

        foreach (ItemStat item in inventoryManager.transform)           //ignores non item gameobjects
        {
            if (item.name == name)
            {
                PathNode endNode = pathfindingGrid.getGridObject(new Vector3(item.xCoord, item.yCoord, 0));     //convert world position to vector to call overloaded function
                int itemCost = getTotalCost(startNode, endNode, canFly);            //pathfind to location, get cost
                //cost based comparison
                if (itemCost < cost)
                {
                    cost = itemCost;
                    nearestItem = item;
                }
            }
        }

        return nearestItem;              //aka null, or closest item
        //if either process cannot find the item, then return null
    }

}