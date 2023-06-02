using System.Collections.Generic;
using UnityEngine;

public class BaseEntity : MonoBehaviour
{
    // This is the base Entity class for a moving NPC. 

    MasterGrid mapinstance;
        //get reference to map
        //get walkspeed data
        //change walkspeed

    //pathfinding stuff
    List<Vector3> pathVectorList;
    int currentPathIndex = 0;

    // base stats 0-10;
    private int pointAlloc_BaseStat;
    private int[,] list_Stat = new int[4, 2];                //0 = stat, 1 = exp

    // General Value calculated from stats
    private float maxHealth, currHealth;
    private float rateMove, rateWork, rateLearn, rateResearch;
    private float maxCapacity, currCapacity;

    private GameObject targetPtr;            //pointer to object

    // FSM States
    enum FSMstates
    {
        IDLE, MOVE, HAUL,
        //needs

        //task

        //combat
        ATTACK, FLEE,
    }

    // Lethal Needs
    private float needThreshold;                  //percentage of max value to seek out need fulfillment
    private float maxHunger, currHunger, rateHunger;
    private float maxThirst, currThirst, rateThirst;
    private float maxEnergy, currEnergy, rateEnergy;
    private float maxOxygen, currOxygen, rateOxygen;        //value is based on environment

    // Threshold Needs                          //value is based on environment
    private float minTemperature, maxTemperature;
    private float minRadiation, maxRadiation;
    private float minPressure, maxPressure;

    // Mood Needs
    private float maxComfort, currComfort, rateComfort;
    private float maxHygiene, currHygiene, rateHygiene;
    private float maxFun, currFun, rateFun;
    private float maxSocial, currSocial, rateSocial;

    // Skill Level
    private int pointAlloc_BaseSkill;
    //first value is the level, followed by current exp
    private int[,] list_Skill = new int[10, 2];                  //0 = skill, 1 = exp

    // Priority Queue                           // from a scale of -10 to 10, the higher the priority, the more likely the NPC is assigned the task,
    // the lower the value, the more likely someone else is assigned the task
    // The task is calculated after needs, only NPC with non-urgent needs are assigned a task
    private int priorityHaul = 0;               //hauling involves moving items to stockpiles, taking items to stockpiles, refilling machines
    private int priorityConstruction = 0;
    private int priorityCook = 0;
    private int priorityFarm = 0;
    private int priorityTend = 0;               //to break down into seperate tasks and convert into a list
    private int priorityCraft = 0;
    private int priorityMedical = 0;
    private int priorityArtistic = 0;
    //no ranged, melee and social

    // Equipment
    // should be pointers to the object
    private int equipHead, equipOuterwear, equipChest, equipInnerwear, equipPants;
    private int equipDrone, equipUtility, equipTool;

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
            pointAlloc_BaseStat = 15;                       //0.4 allocation out of 40
                                                            //list_Stat[0, 0]
                                                            //generate 3 values for the ranges
                                                            //sort the 3 values
                                                            //0 = 1st range
                                                            //1 = 2nd range - 1st range
                                                            //2 = 3rd range - 2nd range
                                                            //3 = max - 3rd range

            // GV
            maxHealth = 100 + list_Stat[0, 0] * 10;          //theoritical 100-200 health range
            currHealth = maxHealth;
            maxCapacity = 50 + list_Stat[0, 0] * 5;         //theoritical 50-100 carry capacity
            rateMove = (2 + list_Stat[0, 0] / 5)*2;            //theoritical 4-8 move speed
            rateWork = 1 + list_Stat[1, 0] / 10;          //theoritical 1-2 work speed
            rateLearn = 1 + list_Stat[2, 0] / 10;      //theoritical 1-2 learn speed
            rateResearch = 1 + list_Stat[2, 0] / 10;      //theoritical 1-2 research speed
        }

        //needs
        {
            //lethal needs
            needThreshold = 0.3f;

            //threshold needs

            //mood needs
        }

        //skill levels
        {
            //skill level
            pointAlloc_BaseSkill = 40;          //0.2 allocation, out of 200

            //follow method above for generating base stats

            //LevelValue temp;
            //temp.level = 0;
            //temp.exp = 0;
            //list_Skill[0] = temp;
        }
    }


    public void setMapInstance(MasterGrid instance)
    {
        mapinstance = instance;
    }

    public void setTargetPos(Vector3 targetPos)
    {
        currentPathIndex = 0;
        pathVectorList = mapinstance.findVectorPath(transform.position, targetPos);

        if (pathVectorList != null && pathVectorList.Count > 1) pathVectorList.RemoveAt(0);

    }

    private void handleMovement()
    {
        if (pathVectorList != null)
        {
            Vector3 targetPos = pathVectorList[currentPathIndex];
            if (Vector3.Distance(transform.position, targetPos) > 0.05f)
            {
                Vector3 moveDir = (targetPos - transform.position).normalized;
                float distanceBefore = Vector3.Distance(transform.position, targetPos);
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
        handleMovement();
        //do task

    }

    private void interact()
    {
        //check what is on the position
        //get position, then ask all layers to do stuff

        //if item, then pickup

        //if machine then interact
    }
}
