using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstalledObject : MonoBehaviour
{
    public enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }
    public Direction dir = Direction.Up;

    public string objectType;               //sprite to render          //to be replaced with gameobject or similar storage medium
    public int width, height;
    public float movementCost;              //sum modifier

    int maxHealth, currHealth;

    public void install()
    {
        switch (transform.eulerAngles.z)
        {
            default:            //ignore as only 4 directions
            case 0:
            case 360:
                //assign the rotation to the object, requires non-static function and monobehavior
                dir = Direction.Up;
                break;
            case 90:
            case -270:
                dir = Direction.Left;
                break;
            case 180:
            case -180:
                dir = Direction.Down;
                break;
            case 270:
            case -90:
                dir = Direction.Right;
                break;
        }
    }

    public List<Vector2> GetGridPositionList()
    {
        List<Vector2> list = new List<Vector2>();
        if (width == 1 && height == 1)          //dont calculate as only occupy 1 position
        {
            list.Add(transform.position);
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
                        list.Add(new Vector2(transform.position.x + x, transform.position.y + y));
                    }
                }
                break;
            case Direction.Down:
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        list.Add(new Vector2(transform.position.x - x, transform.position.y - y));
                    }
                }
                break;
            case Direction.Left:
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {

                        list.Add(new Vector2(transform.position.x - y, transform.position.y + x));
                    }
                }
                break;
            case Direction.Right:
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        list.Add(new Vector2(transform.position.x + y, transform.position.y - x));
                    }
                }
                break;
        }

        return list;
    }

    public GameObject CreateObject(string objectType, int width = 1, int height = 1, float movementCost = 1)            //use for objects created with scripts instead of prefabs
    {
        this.objectType = objectType;
        this.width = width;
        this.height = height;
        this.movementCost = movementCost;

        return gameObject;
    }
}
