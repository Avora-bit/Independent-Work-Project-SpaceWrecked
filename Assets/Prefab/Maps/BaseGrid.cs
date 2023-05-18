using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseGrid<GridType>
{
    private MapData mapData;                //reference to map size
    private GridType[,] gridArray;

    private bool bRebuild = false;          //if need rebuild

    public void generateGrid(MapData mapSize)
    {
        this.mapData = mapSize;
        gridArray = new GridType[mapSize.getWidth(), mapSize.getHeight()];
    }

    public int getWidth()
    {
        return mapData.getWidth();
    }
    public int getHeight()
    {
        return mapData.getHeight();
    }

    public float getCellSize()
    {
        return mapData.getCellSize();
    }

    public bool checkValid(int x, int y)
    {
        return (x >= 0 && x <= mapData.getWidth()) &&
            (y >= 0 && y <= mapData.getHeight());
    }

    public Vector3 getWorldPos(int x, int y)
    {
        return new Vector3(x, y, 0) * mapData.getCellSize() + mapData.getOriginPos();

    }
    public void getXY(Vector3 worldPos, out int x, out int y)
    {
        x = Mathf.FloorToInt((worldPos - mapData.getOriginPos()).x / mapData.getCellSize());
        y = Mathf.FloorToInt((worldPos - mapData.getOriginPos()).y / mapData.getCellSize());
    }

    public GridType getValue(int x, int y)
    {
        if (checkValid(x, y)) return gridArray[x, y];
        else return default;
    }

    public GridType getValue(Vector3 worldPos)
    {
        int x, y;
        getXY(worldPos, out x, out y);
        return getValue(x, y);
    }

    public void setValue(int x, int y, GridType value)
    {
        if (checkValid(x, y))
        {
            gridArray[x, y] = value;
            bRebuild = true;
        }
    }

    public void setValue(Vector3 worldPos, GridType value)
    {
        int x, y;
        getXY(worldPos, out x, out y);
        setValue(x, y, value);
    }
    public bool getRebuild() { return bRebuild; }
    public void setRebuild(bool state) { bRebuild = state; }

}