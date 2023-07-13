using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointerArrow : MonoBehaviour
{
    public GameObject target;

    // Update is called once per frame
    void Update()
    {
        //check if the target is null, if null then ignore
        if (target != null)
        {
            Vector3 screenPos = Input.mousePosition;
            screenPos.z -= Camera.main.transform.position.z;

            target.transform.position = Camera.main.ScreenToWorldPoint(screenPos);
            
            transform.LookAt(target.transform.position);
            transform.GetChild(0).gameObject.SetActive(true);
        }
        else
        {
            transform.GetChild(0).gameObject.SetActive(false);
        }
    }
}
