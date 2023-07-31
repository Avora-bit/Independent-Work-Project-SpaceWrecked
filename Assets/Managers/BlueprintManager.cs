using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlueprintManager : MonoBehaviour
{
    private void Awake()
    {
        
    }

    void Update()
    {
        //choose random child to be built
        //give illusion of slow building
        if (transform.childCount > 0)
        {
            int childIndex = Random.Range(0, transform.childCount);                     //get random child index
            transform.GetChild(childIndex).GetComponent<Blueprint>().FinishBuild();     //finish the build
        }
    }
}
