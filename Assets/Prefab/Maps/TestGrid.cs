using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestGrid : MonoBehaviour
{
    private MapData mapData;                //reference to map data

    public BaseGrid<int> heatArray = new BaseGrid<int>();
    private TextMesh[,] debugTextArray;
    private bool displayDebug = false;
    [SerializeField] private HeatGradientVisual heatmap;

    //null for now
    public BaseGrid<int> damageArray = new BaseGrid<int>();
    public BaseGrid<GameObject> NPCArray = new BaseGrid<GameObject>();
    public BaseGrid<GameObject> structureArray = new BaseGrid<GameObject>();
    public BaseGrid<GameObject> floorArray = new BaseGrid<GameObject>();
    public BaseGrid<int> accessArray = new BaseGrid<int>();


    void Awake()
    {
        mapData = FindObjectOfType<MapData>();
        heatArray.generateGrid(mapData);

        heatmap.SetGrid(heatArray);


        //create debug text on grid
        debugTextArray = new TextMesh[mapData.getWidth(), mapData.getHeight()];
        for (int x = 0; x < mapData.getWidth(); ++x)
        {
            for (int y = 0; y < mapData.getHeight(); ++y)
            {
                //heat 
                debugTextArray[x, y] = createWorldText(heatArray.getValue(x, y).ToString(), gameObject.transform, heatArray.getWorldPos(x, y) + new Vector3(mapData.getCellSize() / 2, mapData.getCellSize() / 2, 0));
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

    //call this function when you press the overlay button
    public void toggleDebug()
    {
        displayDebug = !displayDebug;
        for (int x = 0; x < mapData.getWidth(); ++x)
        {
            for (int y = 0; y < mapData.getHeight(); ++y)
            {
                debugTextArray[x, y].gameObject.SetActive(displayDebug);
            }
        }
    }

    public void addRadial(int intensity, int range)
    {

    }

    // Update is called once per frame
    void Update()
    {
        //heat map
        if (heatArray.getRebuild() == true)
        {
            //rebuild the array
            for (int x = 0; x < mapData.getWidth(); ++x)
            {
                for (int y = 0; y < mapData.getHeight(); ++y)
                {
                    debugTextArray[x, y].text = heatArray.getValue(x, y).ToString();
                }
            }
            heatmap.updateMeshVisual();

            heatArray.setRebuild(false);
        }
    }
}
