using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Grid
{
    private int width, height;
    private float cellSize;
    private Vector3 originPos;
    private int[,] gridArray;
    public Grid(int width, int height, float cellSize, Vector3 originPos)
    {
        this.width = width;
        this.height = height;
        this.cellSize = cellSize;
        this.originPos = originPos;

        gridArray = new int[width, height];

        //debug
        for (int x = 0; x < gridArray.GetLength(0) + 1; x++)
        {
            Debug.DrawLine(getWorldPos(x, 0), getWorldPos(x, height), Color.white, 100f);
        }
        for (int y = 0; y < gridArray.GetLength(1) + 1; y++)
        {
            
            Debug.DrawLine(getWorldPos(0, y), getWorldPos(width, y), Color.white, 100f);
        }
    }

    public bool checkValid(int x, int y)
    {
        return (x >= 0 && x <= width) && 
            (y >= 0 && y <= height);
    }

    private Vector3 getWorldPos(int x, int y)
    {
        return new Vector3(x, y) * cellSize + originPos; 

    }
    public void getXY(Vector3 worldPos, out int x, out int y)
    {
        x = Mathf.FloorToInt((worldPos - originPos).x / cellSize - cellSize / 2);
        y = Mathf.FloorToInt((worldPos - originPos).y / cellSize - cellSize / 2);
    }

    public void setValue(int x, int y, int value)
    {
        if (checkValid(x, y))gridArray[x, y] = value;
    }
    public void setValue(Vector3 worldPos, int value)
    {
        int x, y;
        getXY(worldPos, out x, out y);
        setValue(x, y, value);
    }

    public int getValue(int x, int y)
    {
        if (checkValid(x, y)) return gridArray[x, y];
        return 0;
    }
    public int getValue(Vector3 worldPos)
    {
        int x, y;
        getXY(worldPos, out x, out y);
        return getValue(x, y);
    }

    public void Update()
    {

    }
}
