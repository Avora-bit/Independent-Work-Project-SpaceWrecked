using System.Collections.Generic;
using UnityEngine;

public class BaseEntity : MonoBehaviour
{
    // This is the base Entity class for a moving NPC. 

    MasterGrid mapInstance;

    //pathfinding stuff
    private List<Vector3> pathVectorList = null;
    private int currentPathIndex = 0;

    // base stats 0-10;
    public int pointAlloc_BaseStat = 15;                        //out of 40
    private int[,] list_Stat = new int[4, 2];                   //0 = stat, 1 = exp

    // Skill Level
    public int pointAlloc_BaseSkill = 40;                       //out of 200
    //first value is the level, followed by current exp
    private int[,] list_Skill = new int[10, 2];                  //0 = skill, 1 = exp

    // Priority Queue                           // from a scale of -3 to 3, the higher the priority, the more likely the NPC is assigned the task
    // The task is calculated after needs, only NPC with non-urgent needs are assigned a task
    //haul, construction, cook, plants, animals, craft, medical, artistic
    //no ranged, melee and social
    public int[] taskPriority = new int[8];

    // General Value calculated from stats
    private float maxHealth, currHealth;
    private float rateMove, rateWork, rateLearn, rateResearch;
    private float maxCapacity, currCapacity;

    private GameObject npcPtr = null;            //pointer to object
    private ItemStat itemPtr = null;

    private List<ItemStat> inventory = new List<ItemStat>();
    private Task taskRef;           //reference to task in NPCController, attempt to finish task before taking another
    //stored reference in case need to fulfill needs

    // FSM States
    enum FSMstates
    {
        IDLE, MOVE, HAUL,
        //needs
        EAT, DRINK, REST, COMFORT, HYGIENE, RECREATION, SOCIAL,
        //task
        CONSTRUCTION, COOK, PLANTS, ANIMAL, CRAFT, MEDICAL, ART, //no skill for social as it is not a task
        //combat
        RANGED, MELEE, FLEE,
        //else
        TOTALSTATES
    }

    // Lethal Needs
    public float needThreshold = 0.3f;                          //percentage of max value to seek out need fulfillment
    private float maxHunger, currHunger, rateHunger;
    private float maxThirst, currThirst, rateThirst;
    private float maxEnergy, currEnergy, rateEnergy;
    private float maxOxygen, currOxygen, rateOxygen;            //constant depletion, value is based on environment

    // Threshold Needs                          //value is based on environment
    private float minTemperature, maxTemperature;
    private float minRadiation, maxRadiation;
    private float minPressure, maxPressure;

    // Mood Needs
    private float maxComfort, currComfort, rateComfort;
    private float maxHygiene, currHygiene, rateHygiene;
    private float maxFun, currFun, rateFun;
    private float maxSocial, currSocial, rateSocial;


    // Equipment
    // should be pointers to the object
    private int equipHead, equipOuterwear, equipChest, equipInnerwear, equipPants;
    private int equipDrone, equipUtility, equipTool;

    private bool canFly = false;            //ie equipped jetpack

    // Inventory
    private int currInventorySize, maxInventorySize;
    private int[] InventoryItems;                           //pointer list to all items in inventory

    // Start is called before the first frame update
    void Awake()
    {
        //base stats and general values
        {
            // Base stats
            //distributes from point allocation
                                                            //list_Stat[0, 0]
                                                            //generate 3 values for the ranges
                                                            //sort the 3 values
                                                            //0 = 1st range
                                                            //1 = 2nd range - 1st range
                                                            //2 = 3rd range - 2nd range
                                                            //3 = max - 3rd range

            // GV
            maxHealth = 100 + list_Stat[0, 0] * 10;         //theoritical 100-200 health range
            currHealth = maxHealth;
            maxCapacity = 50 + list_Stat[0, 0] * 5;         //theoritical 50-100 carry capacity
            rateMove = (2 + list_Stat[0, 0] / 5)*2;         //theoritical 4-8 move speed
            rateWork = 1 + list_Stat[1, 0] / 10;            //theoritical 1-2 work speed
            rateLearn = 1 + list_Stat[2, 0] / 10;           //theoritical 1-2 learn speed
            rateResearch = 1 + list_Stat[2, 0] / 10;        //theoritical 1-2 research speed
        }

        //skill levels
        {
            //skill level

            //follow method above for generating base stats

            //LevelValue temp;
            //temp.level = 0;
            //temp.exp = 0;
            //list_Skill[0] = temp;

            //task priority
        }

        //task priority
        {
            //default assign all as 0
            for (int i = 0; i < 8; i++) { taskPriority[i] = 0; }

            //make functions to read and write the values
        }

        //needs
        {
            //lethal needs
            needThreshold = 0.3f;

            //threshold needs

            //mood needs
        }

    }


    public void setMapInstance(MasterGrid instance)
    {
        mapInstance = instance;
    }

    public void setTargetPos(Vector3 targetPos)
    {
        currentPathIndex = 0;
        pathVectorList = mapInstance.findVectorPath(transform.position, targetPos, canFly);

        if (pathVectorList != null && pathVectorList.Count > 1) pathVectorList.RemoveAt(0);     //remove self position
    }

    private void handleMovement()
    {
        if (pathVectorList != null)     //only move when there is a path
        {
            Vector3 targetPos = pathVectorList[currentPathIndex];
            if (Vector3.Distance(transform.position, targetPos) > 0.05f)
            {
                Vector3 moveDir = (targetPos - transform.position).normalized;
                transform.position = transform.position + moveDir * rateMove * Time.deltaTime;
            }
            else
            {
                transform.position = targetPos;
                currentPathIndex++;
                if (currentPathIndex >= pathVectorList.Count) stopMoving();
            }
        }
    }

    private void stopMoving()
    {
        pathVectorList = null;
        //pick up item
        interact();
    }

    // Update is called once per frame
    void Update()
    {
        //check health

        //reduce needs

        //check needs for fulfill
         
        //if needs, set pathfind target

        //if no needs, then check for task

        //if no task, then assign task

        //set pathfind target
        if (itemPtr == null)
        {
            Debug.Log("waiting for item");
            itemPtr = mapInstance.findNearest((int)transform.position.x, (int)transform.position.y, "Aluminium", canFly);
        }
        else
        {
            //get item
            if (pathVectorList == null)     //start moving only when item is found
            {
                setTargetPos(new Vector3(itemPtr.xCoord, itemPtr.yCoord, 0));
            }
        }

        //move to object
        if (npcPtr != null)
        {
            //chase after NPC
            if (pathVectorList == null)
                setTargetPos(npcPtr.transform.position);
        }
        else
        {
            //null
        }

        

        //movement
        handleMovement();
        //do task
        //interact function called by handlemmovement


        //debug

    }

    

    public ItemStat getItemPtr()
    {
        return itemPtr;
    }

    public void setItemPtr(ItemStat target)
    {
        itemPtr = target;
    }

    public GameObject getNPCPtr()
    {
        return npcPtr;
    }

    public void setNPCPtr(GameObject target)
    {
        npcPtr = target;
    }

    private void interact()
    {
        //check what is on the position
        //get position, then ask all layers to do stuff

        //if NPC, then interact

        //if item, then pickup
        inventory.Add(itemPtr);
        Destroy(itemPtr.gameObject);
        itemPtr = null;
        Debug.Log(inventory.Count);

        //if machine then interact
    }
}
