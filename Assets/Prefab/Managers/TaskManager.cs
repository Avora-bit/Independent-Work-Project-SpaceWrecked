using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskManager : MonoBehaviour
{
    List<Task> taskList = new List<Task>();
    TimeController timeController;

    bool sortedToday = false;

    private void Awake()
    {
        timeController = FindObjectOfType<TimeController>();
    }

    void Update()
    {
        if (!sortedToday && timeController.getHours() == 0)
        {
            sortedToday = true;
            taskList.Sort((p1, p2) => p1.priority.CompareTo(p2.priority));          //sorting based on strength of priority, ignore the skill/work type as they are unordered
            Debug.Log("sorted today");
        }
        else if (sortedToday && timeController.getHours() == 1)
        {
            sortedToday = false;
        }
    }

    //update the task every 0000, so the new tasks are created unordered
}
