using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestGrid : MonoBehaviour
{
    private MapSize mapSize;                //reference to map size

    public BaseGrid<int> gridArray = new BaseGrid<int>();

    private TextMesh[,] debugTextArray;
    private bool displayDebug = true;

    [SerializeField] private HeatGradientVisual heatmap;

    void Awake()
    {
        mapSize = FindObjectOfType<MapSize>();
        gridArray.generateGrid(mapSize);

        heatmap.SetGrid(gridArray);


        //create debug text on grid
        debugTextArray = new TextMesh[mapSize.getWidth(), mapSize.getHeight()];
        for (int x = 0; x < mapSize.getWidth(); ++x)
        {
            for (int y = 0; y < mapSize.getHeight(); ++y)
            {
                debugTextArray[x, y] = createWorldText(gridArray.getValue(x, y).ToString(), gameObject.transform, gridArray.getWorldPos(x, y) + new Vector3(mapSize.getCellSize() / 2, mapSize.getCellSize() / 2, 0));
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
    public void toggleDebug(bool state)
    {
        displayDebug = state;
        for (int x = 0; x < mapSize.getWidth(); ++x)
        {
            for (int y = 0; y < mapSize.getHeight(); ++y)
            {
                debugTextArray[x, y].gameObject.SetActive(displayDebug);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        //get update value from base grid and player controller
        if (gridArray.getRebuild() == true)
        {
            //rebuild the array
            for (int x = 0; x < mapSize.getWidth(); ++x)
            {
                for (int y = 0; y < mapSize.getHeight(); ++y)
                {
                    debugTextArray[x, y].text = gridArray.getValue(x, y).ToString();
                }
            }
            heatmap.updateMeshVisual();

            gridArray.setRebuild(false);
        }
    }
}
