using System.Collections.Generic;
using UnityEngine;

public class InstalledObject
{
    public enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }
    Direction dir = Direction.Up;
    Vector2 position;                 //origin position of the object

    string objectType;              //sprite to render

    float movementCost = 1;             //sum multiplier
    int width = 1, height = 1;

    public InstalledObject(string objectType, float movementCost, int width, int height)
    {
        this.objectType = objectType;
        this.movementCost = movementCost;
        this.width = width;
        this.height = height;
    }

    public List<Vector2> GetGridPositionList()
    {
        List<Vector2> list = new List<Vector2>();
        if (width == 1 && height == 1)          //dont calculate as only occupy 1 position
        {
            list.Add(position);
            return list;
        }
        switch (dir)
        {
            default:            //ignore as only 4 directions
            case Direction.Up:
            case Direction.Down:
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        list.Add(position + new Vector2(x, y));
                    }
                }
                break;
            case Direction.Left:
            case Direction.Right:           //90 degree offset from the previous case
                for (int x = 0; x < height; x++)
                {
                    for (int y = 0; y < width; y++)
                    {
                        list.Add(position + new Vector2(x, y));
                    }
                }
                break;
        }




        return list;
    }
}
