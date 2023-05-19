using Mono.Cecil;
using System;
using System.Collections;
using System.Collections.Generic;
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
    public BaseGrid<int> arrayAccess = new BaseGrid<int>();             //get pathfinding data, 0 for empty, 1 for filled

    public BaseGrid<PathNode> pathfindingGrid;

    //null for now
    public BaseGrid<GameObject> NPCArray = new BaseGrid<GameObject>();
    public BaseGrid<GameObject> structureArray = new BaseGrid<GameObject>();
    public BaseGrid<GameObject> floorArray = new BaseGrid<GameObject>();

    void Awake()
    {
        mapData = FindObjectOfType<MapData>();

        //create debug text on grid
        debugTextArray = new TextMesh[mapData.getWidth(), mapData.getHeight()];
        for (int x = 0; x < mapData.getWidth(); ++x)
        {
            for (int y = 0; y < mapData.getHeight(); ++y)
            {
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
        arrayAccess.generateGrid(mapData, (arrayAccess, x, y) => 0);

        pathfindingGrid.generateGrid(mapData, (pathfindingGrid, x, y) => new PathNode(pathfindingGrid, 0, 0));             //prove that generics accept custom game objects
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
                if (arrayAccess.getRebuild()) {
                    for (int x = 0; x < mapData.getWidth(); ++x) {
                        for (int y = 0; y < mapData.getHeight(); ++y) {
                            debugTextArray[x, y].text = arrayAccess.getGridObject(x, y).ToString();
                        }
                    }
                    updateMeshVisual(arrayAccess);
                    arrayAccess.setRebuild(false);
                }
                meshRenderer.enabled = true;
                break;
            case 3:
                //pathfinding, no mesh
                if (pathfindingGrid.getRebuild())
                {
                    for (int x = 0; x < mapData.getWidth(); ++x)
                    {
                        for (int y = 0; y < mapData.getHeight(); ++y)
                        {
                            debugTextArray[x, y].text = pathfindingGrid.getGridObject(x, y).ToString();
                        }
                    }
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
        for (int x = 0; x < mapData.getWidth(); ++x) {
            for (int y = 0; y < mapData.getHeight(); ++y) {
                for (int z = 1; z <= 8; ++z) {
                    int xDirection = 0, yDirection = 0;
                    if (z == 1 || z == 4 || z == 7) xDirection = -1;
                    else if (z == 3 || z == 6 || z == 9) xDirection = 1;

                    if (z == 1 || z == 2 || z == 3) yDirection = 1;
                    else if (z == 7 || z == 8 || z == 9) yDirection = -1;

                    //self heat is an average of neighbour
                    int averagetemp = (arrayHeat.getGridObject(x, y) + arrayHeat.getGridObject(x + xDirection, y + yDirection)) / 2;
                    arrayHeat.setGridObject(x, y, averagetemp);
                    arrayHeat.setGridObject(x + xDirection, y + yDirection, averagetemp);
                }
            }
        }

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
                else if (arrayType == arrayAccess)
                {
                    Vector2 gridValueUV = new Vector2(gridValue, 0f);
                    AddQuad(vertices, uv, triangles, index, arrayType.getWorldPos(x, y) + 0.5f * quadSize, quadSize, gridValueUV);
                }


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
    public void toggleAccess()
    {
        if (renderLayer == 2) renderLayer = 0;
        else {
            meshRenderer.material = tempMaterials[1];
            updateMeshVisual(arrayAccess);
            renderLayer = 2;
            for (int x = 0; x < mapData.getWidth(); ++x) {
                for (int y = 0; y < mapData.getHeight(); ++y) {
                    debugTextArray[x, y].gameObject.SetActive(true);
                    debugTextArray[x, y].text = arrayAccess.getGridObject(x, y).ToString();
                }
            }
        }
    }

    public void togglePathfinding()
    {
        if (renderLayer == 3) renderLayer = 0;
        else {
            renderLayer = 3;
            for (int x = 0; x < mapData.getWidth(); ++x) {
                for (int y = 0; y < mapData.getHeight(); ++y) {
                    debugTextArray[x, y].gameObject.SetActive(true);
                    debugTextArray[x, y].text = pathfindingGrid.getGridObject(x, y).ToString();
                }
            }
        }
    }
}
