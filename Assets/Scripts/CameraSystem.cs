using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public class CameraSystem : BaseSingleton<CameraSystem>
{
    [SerializeField] private float moveSpeed = 5, sprintMult = 5;
    [SerializeField] private int edgeScrollBuffer = 10;
    [SerializeField] private float minPosX = -100, minPosY = -100, maxPosX = 100, maxPosY = 100;
    [SerializeField] private float zoomSpeed = 20;
    [SerializeField] private float minZoom = 10, maxZoom = -30;

    public GameObject followCamTarget = null;


    private bool b_IsLMB = false, b_IsRMB = false;

    // Update is called once per frame
    void Update()
    {
        //mouse input
        {
            if (!b_IsLMB && Input.GetMouseButton(0))           //LMB down
            {
                //do smth
                b_IsLMB = true;
            }
            else if (b_IsLMB && !Input.GetMouseButton(0))      //LMB up
            {
                //do smth
                b_IsLMB = false;
            }
            else if (b_IsLMB && Input.GetMouseButton(0))       //LMB pressed
            {
                //do smth
            }
            if (!b_IsRMB && Input.GetMouseButton(0))           //RMB down
            {
                //do smth
                b_IsRMB = true;
            }
            else if (b_IsRMB && !Input.GetMouseButton(0))      //RMB up
            {
                //do smth
                b_IsRMB = false;
            }
            else if (b_IsRMB && Input.GetMouseButton(0))       //RMB pressed
            {
                //do smth
            }
        }

        //camera input
        {
            if (followCamTarget == null)
            {
                Vector3 moveDir = new Vector3(0, 0, 0);
                //wasd movement, mouse edge scroll
                if (Input.GetKey(KeyCode.W) || Input.mousePosition.y > Screen.height - edgeScrollBuffer) moveDir.y += moveSpeed;
                if (Input.GetKey(KeyCode.A) || Input.mousePosition.x < edgeScrollBuffer) moveDir.x -= moveSpeed;
                if (Input.GetKey(KeyCode.S) || Input.mousePosition.y < edgeScrollBuffer) moveDir.y -= moveSpeed;
                if (Input.GetKey(KeyCode.D) || Input.mousePosition.x > Screen.width - edgeScrollBuffer) moveDir.x += moveSpeed;

                transform.position += moveDir * Time.unscaledDeltaTime * (Input.GetKey(KeyCode.LeftShift) ? sprintMult : 1);
                //zoom
                if (Input.mouseScrollDelta.y > 0) transform.position = new Vector3(transform.position.x, transform.position.y,
                                                  Mathf.Lerp(transform.position.z, transform.position.z + zoomSpeed, Time.unscaledDeltaTime * zoomSpeed));
                if (Input.mouseScrollDelta.y < 0) transform.position = new Vector3(transform.position.x, transform.position.y,
                                                  Mathf.Lerp(transform.position.z, transform.position.z - zoomSpeed, Time.unscaledDeltaTime * zoomSpeed));
            }
            else
            {
                //disable follow cam
                if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D)) followCamTarget = null;

                //has a follow cam target, follow location to target
                transform.position = new Vector3(followCamTarget.transform.position.x, followCamTarget.transform.position.y, maxZoom);
            }

            //camera bounds

            if (transform.position.x <= minPosX) transform.position = new Vector3(minPosX, transform.position.y, transform.position.z);
            if (transform.position.x >= maxPosX) transform.position = new Vector3(maxPosX, transform.position.y, transform.position.z);
            if (transform.position.y <= minPosY) transform.position = new Vector3(transform.position.x, minPosY, transform.position.z);
            if (transform.position.y >= maxPosY) transform.position = new Vector3(transform.position.x, maxPosY, transform.position.z);
            if (transform.position.z >= minZoom) transform.position = new Vector3(transform.position.x, transform.position.y, minZoom);
            if (transform.position.z <= maxZoom) transform.position = new Vector3(transform.position.x, transform.position.y, maxZoom);
        }
    }
}
