using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemStat : MonoBehaviour
{
    //assumes all public values, be careful of changing values outside
    //attempt to use a read only method 

    public string name;
    public float weight;
    public float durability;
    public int quantity;

    //other values
    public float minTemp, maxTemp;     //temp range for adverse effects
    public float beauty;       //replace with comfort value when equipment
    public float resHeat;
    public float wealth;

    //if material
    public bool isMaterial;        //if material

    public float walkSpeed;
    public float cleaniness;
    public float resBlast;
    //if not material, then fuel
    public float fuel;
    public float DmgBlast;
}