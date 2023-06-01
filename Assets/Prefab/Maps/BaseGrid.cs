using System;
using UnityEngine;

public class BaseGrid<GridType>
{
    public MapData mapData;                //reference to map size
    private GridType[,] gridArray;

    private bool bRebuild = false;          //if need rebuild

    public void generateGrid(MapData mapData, Func<BaseGrid<GridType>, int, int, GridType> createGridObject)
    {
        //map data handles the width, height, size and starting vector
        this.mapData = mapData;
        gridArray = new GridType[mapData.getWidth(), mapData.getHeight()];

        for (int x = 0; x < mapData.getWidth(); x++) {
            for (int y = 0; y < mapData.getHeight(); y++) {
                gridArray[x, y] = createGridObject(this, x, y);
            }
        }
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
        return x >= 0 && x < mapData.getWidth() &&
               y >= 0 && y < mapData.getHeight();
    }

    public Vector3 getWorldPos(int x, int y)
    {
        return new Vector3(x, y, 0) * mapData.getCellSize() + mapData.getOriginPos();
    }
    public Vector3 getWorldPosCenter(int x, int y)
    {
        return new Vector3(x, y, 0) * mapData.getCellSize() + mapData.getOriginPos() +
               new Vector3(mapData.getCellSize() / 2, mapData.getCellSize() / 2, 0);
    }

    public void getXY(Vector3 worldPos, out int x, out int y)
    {
        x = Mathf.FloorToInt((worldPos - mapData.getOriginPos()).x / mapData.getCellSize());
        y = Mathf.FloorToInt((worldPos - mapData.getOriginPos()).y / mapData.getCellSize());
    }

    public GridType getGridObject(int x, int y)
    {
        if (checkValid(x, y)) return gridArray[x, y];
        else return default;
    }

    public GridType getGridObject(Vector3 worldPos)
    {
        int x, y;
        getXY(worldPos, out x, out y);
        return getGridObject(x, y);
    }

    public void setGridObject(int x, int y, GridType value)
    {
        if (checkValid(x, y))
        {
            gridArray[x, y] = value;
            bRebuild = true;
        }
    }

    public void setGridObject(Vector3 worldPos, GridType value)
    {
        int x, y;
        getXY(worldPos, out x, out y);
        setGridObject(x, y, value);
    }
    public bool getRebuild() { return bRebuild; }
    public void setRebuild(bool state) { bRebuild = state; }

}