using UnityEngine;

public class Task : MonoBehaviour
{
    public string taskName;                     //display name
    public GameObject taskObject;               //object reference
    public BaseEntity entity;                   //entity that took the task, if null then open to take
    public int priority;                        //0-9 priority scale
    public int type;                            //what type of action is it
    //if 0 then general, then scaling up based on skill list, has a list of 8
}
