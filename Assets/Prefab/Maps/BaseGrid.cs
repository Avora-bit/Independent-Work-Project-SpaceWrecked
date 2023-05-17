using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseGrid<GridType>
{
    private MapData mapSize;                //reference to map size
    private GridType[,] gridArray;

    private bool bRebuild = false;

    public void generateGrid(MapData mapSize)
    {
        this.mapSize = mapSize;
        gridArray = new GridType[mapSize.getWidth(), mapSize.getHeight()];

        //draw grid lines
        for (int x = 0; x < gridArray.GetLength(0) + 1; x++)
        {
            Debug.DrawLine(getWorldPos(x, 0), getWorldPos(x, mapSize.getHeight()), Color.white, 100f);
        }
        for (int y = 0; y < gridArray.GetLength(1) + 1; y++)
        {
            Debug.DrawLine(getWorldPos(0, y), getWorldPos(mapSize.getWidth(), y), Color.white, 100f);
        }

    }

    public int getWidth()
    {
        return mapSize.getWidth();
    }
    public int getHeight()
    {
        return mapSize.getHeight();
    }

    public float getCellSize()
    {
        return mapSize.getCellSize();
    }

    public bool checkValid(int x, int y)
    {
        return (x >= 0 && x <= mapSize.getWidth()) &&
            (y >= 0 && y <= mapSize.getHeight());
    }

    public Vector3 getWorldPos(int x, int y)
    {
        return new Vector3(x, y, 0) * mapSize.getCellSize() + mapSize.getOriginPos();

    }
    public void getXY(Vector3 worldPos, out int x, out int y)
    {
        x = Mathf.FloorToInt((worldPos - mapSize.getOriginPos()).x / mapSize.getCellSize());
        y = Mathf.FloorToInt((worldPos - mapSize.getOriginPos()).y / mapSize.getCellSize());
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
    public bool getRebuild()
    {
        return bRebuild;
    }

    public void setRebuild(bool state)
    {
        bRebuild = state;
    }

    public void addValueRadial(Vector3 worldPos, GridType value, int range)
    {
        getXY(worldPos, out int originX, out int originY);
        for (int x = 0; x < range; x++) {
            for (int y = 0; y < range; y++) {

            }
        }

        bRebuild = true;        //rebuild only at end
    }

}