using Mono.Cecil;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Collections;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class TestGrid : MonoBehaviour
{
    //overarching manager for the grid system in the map
    //test grid stores all the interacting grids and updates them
    //this allows for the grid to reuse code and reduce update

    private MapData mapData;                //reference to map data

    private TextMesh[,] debugTextArray;
    private Mesh mesh;
    private int renderLayer = 0;            //0 for dont render, 1++ according to following arrays
    MeshRenderer meshRenderer;

    [SerializeField] private Material[] tempMaterials;


    public BaseGrid<int> arrayHeat = new BaseGrid<int>();               //get heat data

    public BaseGrid<PathNode> pathfindingGrid = new BaseGrid<PathNode>();
    private List<PathNode> openList;                //list to allow for data manipulation
    private HashSet<PathNode> closedList;           //generic hashset to check if it contains neighbour
    private const int MOVE_STRAIGHT_COST = 10;
    private const int MOVE_DIAGONAL_COST = 14;              //sqrt of 10+10

    //null for now
    public BaseGrid<GameObject> structureArray = new BaseGrid<GameObject>();
    public BaseGrid<GameObject> floorArray = new BaseGrid<GameObject>();
    public BaseGrid<GameObject> ceilingArray = new BaseGrid<GameObject>();

    void Awake()
    {
        mapData = FindObjectOfType<MapData>();

        //create debug text on grid
        debugTextArray = new TextMesh[mapData.getWidth(), mapData.getHeight()];
        for (int x = 0; x < mapData.getWidth(); ++x) {
            for (int y = 0; y < mapData.getHeight(); ++y) {
                debugTextArray[x, y] = createWorldText("0", gameObject.transform, mapData.getOriginPos() +
                    new Vector3(mapData.getCellSize() * x, mapData.getCellSize() * y, 0) +
                    new Vector3(mapData.getCellSize() / 2, mapData.getCellSize() / 2, 0));
                debugTextArray[x, y].gameObject.SetActive(false);


            }
        }

        //create visual overlay mesh
        meshRenderer = GetComponent<MeshRenderer>();
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        //create grids with data type
        arrayHeat.generateGrid(mapData, (arrayHeat, x, y) => 0);

        pathfindingGrid.generateGrid(mapData, (pathfindingGrid, x, y) => new PathNode(pathfindingGrid, x, y));             //prove that generics accept custom game objects

    }

    public List<Vector3> findVectorPath(Vector3 startPos, Vector3 endPos)
    {
        List<PathNode> path = findPath(pathfindingGrid.getGridObject(startPos), pathfindingGrid.getGridObject(endPos));
        if (path != null)
        {
            List<Vector3> vectorPath = new List<Vector3>();
            foreach (PathNode node in path)
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
        return null;
    }

    public List<PathNode> findPath(PathNode startnode, PathNode endnode) {
        if (startnode == null || endnode == null) return null;

        openList = new List<PathNode> { startnode };
        closedList = new HashSet<PathNode>();

        //cycle through all nodes and set values
        for (int x = 0; x < mapData.getWidth(); x++) {
            for (int y = 0; y < mapData.getHeight(); y++) {
                PathNode node = pathfindingGrid.getGridObject(x, y);
                node.costG = int.MaxValue;          //set to infinite value
                node.calculateCostF();
                node.setPrevNode(null);
            }
        }

        startnode.costG = 0;        //starting node, cost = 0
        startnode.costH = calculateDistanceCost(startnode, endnode);

        while (openList.Count > 0) {
            PathNode currNode = getLowestFCostNode(openList);
            if (currNode == endnode) return calculatePath(endnode);
            openList.Remove(currNode); closedList.Add(currNode);

            foreach (PathNode neighbourNode in getNeighbours(currNode)) {
                if (closedList.Contains(neighbourNode)) continue;
                if (!neighbourNode.isWalkable) { closedList.Add(neighbourNode); continue; }
                //diagonal movement check;
                if (calculateDistanceCost(currNode, neighbourNode) == MOVE_DIAGONAL_COST)           //check if this is diagonal
                {
                    int dirX = neighbourNode.x - currNode.x;
                    int dirY = neighbourNode.y - currNode.y;
                    if (!pathfindingGrid.getGridObject(currNode.x + dirX, currNode.y).isWalkable &&
                        !pathfindingGrid.getGridObject(currNode.x, currNode.y + dirY).isWalkable) continue;     //ignore
                }

                int tempCostG = currNode.costG + calculateDistanceCost(currNode, neighbourNode);
                if (tempCostG < neighbourNode.costG) {
                    neighbourNode.setPrevNode(currNode);
                    neighbourNode.costG = tempCostG;
                    neighbourNode.costH = calculateDistanceCost(neighbourNode, endnode);
                    neighbourNode.calculateCostF();

                    if (!openList.Contains(neighbourNode)) openList.Add(neighbourNode);
                }
            }
        }
        //out of nodes on open list, but path hasnt been found
        return null;
    }

    private int calculateDistanceCost(PathNode a, PathNode b) {               //distance ignoring obstructions
        int distX = Mathf.Abs(a.x - b.x);
        int distY = Mathf.Abs(a.y - b.y);
        int remaining = Mathf.Abs(distX - distY);
        return MOVE_DIAGONAL_COST * Mathf.Min(distX, distY) + 
               MOVE_STRAIGHT_COST * remaining;
    }

    private PathNode getLowestFCostNode(List<PathNode> pathNodeList) {
        PathNode lowestFCostNode = pathNodeList[0];
        for (int i = 0; i < pathNodeList.Count; i++) {
            if (pathNodeList[i].costF < lowestFCostNode.costF) lowestFCostNode = pathNodeList[i];      //simple min comparison
        }
        return lowestFCostNode;
    }

    private List<PathNode> calculatePath(PathNode endNode) {
        List<PathNode> path = new List<PathNode>();
        path.Add(endNode);
        PathNode currNode = endNode;
        while (currNode.getPrevNode() != null) {
            //cycle through all the nodes and find its "parent"
            //until it reaches a node with no parent, aka the start node
            path.Add(currNode.getPrevNode());
            currNode = currNode.getPrevNode();
        }
        path.Reverse();         //since the first value is the end, need to reverse it. 
        return path;
    }

    private List<PathNode> getNeighbours(PathNode currNode) {
        List<PathNode> Neighbours = new List<PathNode>();
        for (int x = -1; x <= 1; ++x) {
            for (int y = -1; y <= 1; ++y) {
                if (pathfindingGrid.checkValid(currNode.x + x, currNode.y + y)) {           //ensure that the node is a neighbour and not on edge
                    if (x == 0 && y == 0) continue;                                         //ensure node is not itself
                    Neighbours.Add(pathfindingGrid.getGridObject(currNode.x + x, currNode.y + y));
                }
            }
        }
        return Neighbours;
    }

    void Update()
    {   
        //only rebuild if both render and rebuild is true, reduces update complexity
        switch (renderLayer) {
            case 1:
                //heat visual
                if (arrayHeat.getRebuild()) {
                    for (int x = 0; x < mapData.getWidth(); ++x) {
                        for (int y = 0; y < mapData.getHeight(); ++y) {
                            debugTextArray[x, y].text = arrayHeat.getGridObject(x, y).ToString();
                        }
                    }
                    updateMeshVisual(arrayHeat);
                    arrayHeat.setRebuild(false);
                }
                meshRenderer.enabled = true;
                break;
            case 2:
                //access visual
                if (pathfindingGrid.getRebuild()) {
                    for (int x = 0; x < mapData.getWidth(); ++x) {
                        for (int y = 0; y < mapData.getHeight(); ++y) {
                            debugTextArray[x, y].text = pathfindingGrid.getGridObject(x, y).ToString();
                        }
                    }
                    updateMeshVisual(pathfindingGrid);
                    pathfindingGrid.setRebuild(false);
                }
                meshRenderer.enabled = true;
                break;
            default:
                //case 0, dont render
                meshRenderer.enabled = false;
                for (int x = 0; x < mapData.getWidth(); ++x)
                {
                    for (int y = 0; y < mapData.getHeight(); ++y)
                    {
                        debugTextArray[x, y].gameObject.SetActive(false);
                    }
                }
                break;
        }

        //diffuse heat
        //replace with neighbour list type diffusion
        //for (int x = -1; x <= 1; ++x) {
        //    for (int y = -1; y <= 1; ++y) {
        //        if (arrayHeat.checkValid(currNode.x + x, currNode.y + y)) {

        //        }
        //    }
        //}

        //for (int x = 0; x < mapData.getWidth(); ++x) {
        //    for (int y = 0; y < mapData.getHeight(); ++y) {
        //        for (int z = 1; z <= 8; ++z) {
        //            int xDirection = 0, yDirection = 0;
        //            if (z == 1 || z == 4 || z == 7) xDirection = -1;
        //            else if (z == 3 || z == 6 || z == 9) xDirection = 1;

        //            if (z == 1 || z == 2 || z == 3) yDirection = 1;
        //            else if (z == 7 || z == 8 || z == 9) yDirection = -1;

        //            //self heat is an average of neighbour
        //            int averagetemp = (arrayHeat.getGridObject(x, y) + arrayHeat.getGridObject(x + xDirection, y + yDirection)) / 2;
        //            arrayHeat.setGridObject(x, y, averagetemp);
        //            arrayHeat.setGridObject(x + xDirection, y + yDirection, averagetemp);
        //        }
        //    }
        //}

    }

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

    public void updateMeshVisual(BaseGrid<int> arrayType)
    {
        CreateEmptyMeshData(mapData.getWidth() * mapData.getHeight(),
            out Vector3[] vertices, out Vector2[] uv, out int[] triangles);

        for (int x = 0; x < mapData.getWidth(); x++) {
            for (int y = 0; y < mapData.getHeight(); y++) {
                int index = x * mapData.getHeight() + y;
                Vector3 quadSize = new Vector3(1, 1) * mapData.getCellSize();

                float gridValue = arrayType.getGridObject(x, y);
                if (arrayType == arrayHeat)
                {
                    float gridValueNormalized = (gridValue - mapData.getMinTemp()) / (mapData.getMaxTemp() - mapData.getMinTemp());
                    Vector2 gridValueUV = new Vector2(gridValueNormalized, 0f);
                    AddQuad(vertices, uv, triangles, index, arrayType.getWorldPos(x, y) + 0.5f * quadSize, quadSize, gridValueUV);
                }
            }
        }
        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
    }

    public void updateMeshVisual(BaseGrid<PathNode> pathArray)
    {
        CreateEmptyMeshData(mapData.getWidth() * mapData.getHeight(),
            out Vector3[] vertices, out Vector2[] uv, out int[] triangles);

        for (int x = 0; x < mapData.getWidth(); x++)
        {
            for (int y = 0; y < mapData.getHeight(); y++)
            {
                int index = x * mapData.getHeight() + y;
                Vector3 quadSize = new Vector3(1, 1) * mapData.getCellSize();

                bool walkable = pathArray.getGridObject(x, y).isWalkable;
                Vector2 gridValueUV = new Vector2(walkable == false ? 0 : 1, 0f);
                AddQuad(vertices, uv, triangles, index, pathArray.getWorldPos(x, y) + 0.5f * quadSize, quadSize, gridValueUV);

            }
        }

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
    }

    private void AddQuad(Vector3[] vertices, Vector2[] uvs, int[] triangles, int index, Vector3 GridPos, Vector3 QuadSize, Vector2 Uv)
    {
        vertices[index * 4] = new Vector3((-0.5f + GridPos.x) * QuadSize.x, (-0.5f + GridPos.y) * QuadSize.y);
        vertices[(index * 4) + 1] = new Vector3((-0.5f + GridPos.x) * QuadSize.x, (+0.5f + GridPos.y) * QuadSize.y);
        vertices[(index * 4) + 2] = new Vector3((+0.5f + GridPos.x) * QuadSize.x, (+0.5f + GridPos.y) * QuadSize.y);
        vertices[(index * 4) + 3] = new Vector3((+0.5f + GridPos.x) * QuadSize.x, (-0.5f + GridPos.y) * QuadSize.y);

        uvs[(index * 4)] = Uv;
        uvs[(index * 4) + 1] = Uv;
        uvs[(index * 4) + 2] = Uv;
        uvs[(index * 4) + 3] = Uv;

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

    public void toggleHeat()
    {
        if (renderLayer == 1) renderLayer = 0;
        else {
            meshRenderer.material = tempMaterials[0];
            meshRenderer.material = meshRenderer.materials[0];
            updateMeshVisual(arrayHeat);
            renderLayer = 1;
            for (int x = 0; x < mapData.getWidth(); ++x) {
                for (int y = 0; y < mapData.getHeight(); ++y) {
                    debugTextArray[x, y].gameObject.SetActive(true);
                    debugTextArray[x, y].text = arrayHeat.getGridObject(x, y).ToString();
                }
            }
        }
    }
    public void togglePathfinding()
    {
        if (renderLayer == 2) renderLayer = 0;
        else {
            meshRenderer.material = tempMaterials[1];
            updateMeshVisual(pathfindingGrid);
            renderLayer = 2;
            for (int x = 0; x < mapData.getWidth(); ++x) {
                for (int y = 0; y < mapData.getHeight(); ++y) {
                    debugTextArray[x, y].gameObject.SetActive(false);
                    debugTextArray[x, y].text = pathfindingGrid.getGridObject(x, y).ToString();
                }
            }
        }
    }
}
