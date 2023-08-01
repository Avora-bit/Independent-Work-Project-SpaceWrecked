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
    public int maxStat = 10;
    private int[,] list_Stat = new int[4, 2];                   //0 = stat, 1 = exp

    // Skill Level
    public int pointAlloc_BaseSkill = 40;                       //out of 200
    public int maxSkill = 20;
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

    [SerializeField] private BaseEntity npcPtr = null;
    [SerializeField] private ItemStat itemPtr = null;

    private List<ItemStat> inventory = new List<ItemStat>();
    public Task taskRef;           //reference to task in NPCController, attempt to finish task before taking another
    //stored reference in case need to fulfill needs or combat

    // FSM States
    public enum FSMstates
    {
        //defaults
        IDLE, HAUL,           //idle = waiting for need, task or combat, avoid combat as much as possible
        //needs
        DRINK, EAT, REST, HYGIENE, COMFORT,  RECREATION, SOCIAL,     //ordered based on importance
        //tasks
        CONSTRUCTION, COOK, PLANTS, ANIMAL, CRAFT, MEDICAL, ART, //no skill for social as it is not a task
        //combat
        RANGED, MELEE, FLEE,            //attempts to range combat before melee, then fleeing
        //else
        TOTALSTATES
    }

    public FSMstates currFSMState = FSMstates.IDLE;
    public double dtLastIdle;

    // Lethal Needs
    public float needThreshold = 0.3f;                          //percentage of max value to seek out need fulfillment
    private float maxHunger, currHunger, rateHunger;
    private float maxThirst, currThirst, rateThirst;
    private float maxEnergy, currEnergy, rateEnergy;
    private float maxOxygen, currOxygen, rateOxygen;            //constant depletion, value is based on environment

    // Mood Needs
    private float maxComfort, currComfort, rateComfort;
    private float maxHygiene, currHygiene, rateHygiene;
    private float maxFun, currFun, rateFun;
    private float maxSocial, currSocial, rateSocial;            //constant depletion, value based on self

    // Threshold Needs                                          //value is based on environment
    private float minTemperature, maxTemperature;
    private float minRadiation, maxRadiation;
    private float minPressure, maxPressure;

    // Equipment
    // should be pointers to the object
    private int equipHead, equipOuterwear, equipChest, equipInnerwear, equipPants;
    private int equipDrone, equipUtility, equipTool;

    [SerializeField] private bool canFly = false;

    // Start is called before the first frame update
    void Awake()
    {
        //base stats and general values
        {
            // Base stats
            //distributes from point allocation
            bool acceptedStats = false;
            do {
                List<int> statRange = new List<int>();
                for (int i = 0; i < list_Stat.Length / 2 - 1; i++) { statRange.Add(Random.Range(0, pointAlloc_BaseStat)); }
                //sort the 3 values
                statRange.Sort();
                list_Stat[0,0] = statRange[0];
                for (int i = 1; i < statRange.Count; i++) { list_Stat[i, 0] = statRange[i] - statRange[i - 1]; }
                list_Stat[statRange.Count, 0] = pointAlloc_BaseStat - statRange[statRange.Count - 1];
                for (int i = 0; i < list_Stat.Length / 2 - 1; i++) {
                    if (list_Stat[i, 0] > maxStat) {
                        acceptedStats = false;
                        break;
                    }
                    else acceptedStats = true;
                }
            } while (!acceptedStats);

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
            bool acceptedSkills = false;
            do {
                List<int> skillRange = new List<int>();
                for (int i = 0; i < list_Skill.Length / 2 - 1; i++) { skillRange.Add(Random.Range(0, pointAlloc_BaseSkill)); }
                //sort the 3 values
                skillRange.Sort();
                list_Skill[0, 0] = skillRange[0];
                for (int i = 1; i < skillRange.Count; i++) { list_Skill[i, 0] = skillRange[i] - skillRange[i - 1]; }
                list_Skill[skillRange.Count, 0] = pointAlloc_BaseSkill - skillRange[skillRange.Count - 1];
                for (int i = 0; i < list_Skill.Length / 2 - 1; i++)
                {
                    if (list_Skill[i, 0] > maxSkill)
                    {
                        acceptedSkills = false;
                        break;
                    }
                    else acceptedSkills = true;
                }
            } while (!acceptedSkills);
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

            //threshold needs
            //private float maxHunger, currHunger, rateHunger;
            //private float maxThirst, currThirst, rateThirst;
            //private float maxEnergy, currEnergy, rateEnergy;
            //private float maxOxygen, currOxygen, rateOxygen;

            // Mood Needs
            //private float maxComfort, currComfort, rateComfort;
            //private float maxHygiene, currHygiene, rateHygiene;
            //private float maxFun, currFun, rateFun;
            //private float maxSocial, currSocial, rateSocial;
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
            float tileMovementOffset = mapInstance.pathfindingGrid.getGridObject(transform.position).movementCost;
            if (Vector3.Distance(transform.position, targetPos) > 0.05f)
            {
                Vector3 moveDir = (targetPos - transform.position).normalized;
                transform.position = transform.position + moveDir * rateMove * Time.deltaTime * tileMovementOffset;              //add speed multiplier here, check for the grid that the npc is on, then apply the multiplier
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
        if (itemPtr || npcPtr) interact();
    }

    // Update is called once per frame
    void Update()
    {
        //check health
        
        if (currHealth <= 0)
        {
            //dead
            Destroy(gameObject);
        }
        else if (maxHealth * .8f >= currHealth)          //health less than half
        {
            //injured
            //rest and heal
        }
        //reduce needs

        //check needs for fulfill

        //if needs, set pathfind target

        //if no needs, then check for task

        //set pathfind target
        if (itemPtr == null)
        {
            if (pathVectorList != null) stopMoving();
            if (itemPtr == null) itemPtr = mapInstance.findNearest(transform.position, "Aluminium", canFly);
            if (itemPtr == null) itemPtr = mapInstance.findNearest(transform.position, "Steel", canFly);
            if (itemPtr == null) itemPtr = mapInstance.findNearest(transform.position, "Plasteel", canFly);
            if (itemPtr == null) itemPtr = mapInstance.findNearest(transform.position, "Tungsten", canFly);
        }
        else
        {
            //get item
            if (pathVectorList == null)     //start moving only when item is found
            {
                setTargetPos(itemPtr.transform.position);
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
        if (currFSMState == FSMstates.IDLE)
        {
            dtLastIdle += Time.deltaTime;
            //Debug.Log("Waiting for task: " + dtLastIdle);
        }
        else
        {
            dtLastIdle = 0;
        }
    }

    public ItemStat getItemPtr()
    {
        return itemPtr;
    }

    public void setItemPtr(ItemStat target)
    {
        itemPtr = target;
    }

    public BaseEntity getNPCPtr()
    {
        return npcPtr;
    }

    public void setNPCPtr(BaseEntity target)
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
        //Debug.Log(inventory.Count);

        //if machine then interact
    }
}
