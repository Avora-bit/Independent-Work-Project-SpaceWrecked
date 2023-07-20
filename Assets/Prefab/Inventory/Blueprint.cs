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


    GameObject selfPrefab;                          //spawn item when completed
    ItemStat selfRef;                               //reference to the script object
    List<KeyValuePair<ItemStat, int>> materialContain;              //stores items and name of materials needed

    //destroyed when finished, telling the grid to change certain values
    
    
    // Start is called before the first frame update
    void Awake()
    {
        //selfRef = selfPrefab.GetComponent<ItemStat>();      //reference to script
        //get prefab values
        
        //material
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
