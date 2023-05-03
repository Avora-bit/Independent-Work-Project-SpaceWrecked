using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    MapSize mapSize;                //reference to map size
    private Grid grid;

    // Start is called before the first frame update
    void Awake()
    {
        mapSize = Object.FindObjectOfType<MapSize>();
        grid = new Grid(mapSize.getWidth(), mapSize.getHeight(), mapSize.getCellSize(), new Vector3(-(mapSize.getWidth() + mapSize.getCellSize()) / 2, -(mapSize.getHeight() + mapSize.getCellSize()) / 2, 0));
        //generate a new grid with origin, offset to allow the tile to be centered on the cell
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 mouseClickPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }
}
