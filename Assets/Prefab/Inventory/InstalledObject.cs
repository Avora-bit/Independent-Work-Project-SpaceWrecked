using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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
    public Direction dir = Direction.Up;
    public Vector2 position;                 //origin position of the object

    public string objectType;              //sprite to render          //to be replaced with gameobject or similar storage medium
    public int width, height;
    public float movementCost;             //sum modifier

    //get reference to health

    int maxHealth, currHealth;

    static public InstalledObject CreateObject(string objectType, int width = 1, int height = 1, float movementCost = 1)
    {
        InstalledObject obj = new InstalledObject();
        obj.objectType = objectType;
        obj.width = width;
        obj.height = height;
        obj.movementCost = movementCost;

        obj.dir = Direction.Up;                 //assign with new variable 

        obj.install();
        return obj;
    }

    public void install()
    {
        switch (dir)
        {
            default:            //ignore as only 4 directions
            case Direction.Up:
                //assign the rotation to the object, requires non-static function and monobehavior
                //rotation = 0;
                break;
            case Direction.Down:

                break;
            case Direction.Left:

                break;
            case Direction.Right:

                break;
        }
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
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        list.Add(position + new Vector2(x, y));
                    }
                }
                break;
            case Direction.Down:
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        list.Add(position + new Vector2(-x, -y));
                    }
                }
                break;
            case Direction.Left:
                for (int x = 0; x < height; x++)
                {
                    for (int y = 0; y < width; y++)
                    {
                        list.Add(position + new Vector2(x, y));
                    }
                }
                break;
            case Direction.Right:           //90 degree offset from the previous case
                for (int x = 0; x < height; x++)
                {
                    for (int y = 0; y < width; y++)
                    {
                        list.Add(position + new Vector2(-x, -y));
                    }
                }
                break;
        }


        return list;
    }
}
