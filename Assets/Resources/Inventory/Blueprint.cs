using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blueprint : MonoBehaviour
{
    //created when placing items using build mode
    //has location data
    //has data of item to spawn
    //has a list of materials needed
    //has a list of materials stored
    //has a build value.

    public Vector2 location;

    public GameObject selfPrefab;                          //spawn item when completed
    public ItemStat selfItemStat;                               //reference to the script object
    List<KeyValuePair<ItemStat, int>> materialContain;              //stores items and name of materials needed, ie inventory

    public enum gridLayer
    {
        Floor,              //tilemap object
        Roof,               //null for now
        Object,             //structure, object
        numLayers, 
    }
    public gridLayer layer;
    int material;           //1-4               //additional tag for material if stated

    //destroyed when finished, telling the grid to change certain values
    
    
    // Start is called before the first frame update
    void Awake()
    {
        //selfRef = selfPrefab.GetComponent<ItemStat>();      //reference to script
        //get prefab values

        //layer

        //material

    }

    public void FinishBuild()
    {
        switch (layer)
        {
            default:
            case gridLayer.Floor:
                //assign to mastergrid
                break;
            case gridLayer.Roof:
                //assign to mastergrid
                break;
            case gridLayer.Object:
                GameObject selfObject = Instantiate(selfPrefab);
                //assign parent here, to object manager
                break;
        }
        Destroy(gameObject);
    }
}
