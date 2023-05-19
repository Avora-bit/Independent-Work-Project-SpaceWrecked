using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : BaseSingleton<PlayerController>
{
    [SerializeField] private float moveSpeed = 5, sprintMult = 5;
    [SerializeField] private int edgeScrollBuffer = 10;
    [SerializeField] private float zoomSpeed = 20;
    [SerializeField] private float minZoom = 10, maxZoom = -30;

    private MapData mapData;
    private float minPosX, minPosY, maxPosX, maxPosY;
    private TestGrid testGrid;

    public GameObject followCamTarget = null;

    private Vector3 mousePos, screenPos;
    private Vector3 dragLMBstart, dragLMBend;


    private bool b_IsLMB = false, b_IsRMB = false, b_IsMMB = false;

    private void Awake()
    {
        //assign reference to map size
        mapData = FindAnyObjectByType<MapData>();

        testGrid = gameObject.transform.parent.parent.GetChild(1).GetComponent<TestGrid>();
        minPosX = mapData.getOriginPos().x;
        minPosY = mapData.getOriginPos().y;
        maxPosX = -mapData.getOriginPos().x;
        maxPosY = -mapData.getOriginPos().y;
    }

    // Update is called once per frame
    void Update()
    {
        if (minPosX == maxPosX && minPosY == maxPosY)           //primitive bounds lock since min and max pos should not be equal,
                                                                //should be replaced with screen loading to ensure managers are created before controllers
        {
            minPosX = mapData.getOriginPos().x;
            minPosY = mapData.getOriginPos().y;
            maxPosX = -mapData.getOriginPos().x;
            maxPosY = -mapData.getOriginPos().y;
        }
        
        //mouse input
        {
            screenPos = Input.mousePosition;
            screenPos.z -= Camera.main.transform.position.z;
            mousePos = Camera.main.ScreenToWorldPoint(screenPos);

            if (!b_IsLMB && Input.GetMouseButton(0))           //LMB down
            {
                b_IsLMB = true;
                dragLMBstart = screenPos;
                //increments of 10
                //int temp = Mathf.Clamp(testGrid.arrayHeat.getValue(mousePos) + 10, mapData.getMinTemp(), mapData.getMaxTemp());
                //testGrid.arrayHeat.setGridObject(mousePos, temp);

                //testGrid.arrayAccess.setGridObject(mousePos, 1);

                //testGrid.pathfindingGrid.setGridObject(mousePos, 1);
            }
            else if (b_IsLMB && Input.GetMouseButton(0))       //LMB pressed, exclude first and last frame
            {
                dragLMBend = screenPos;
            }
            else if (b_IsLMB && !Input.GetMouseButton(0))      //LMB up
            {
                b_IsLMB = false;
                //based on start and end, do smth

                dragLMBstart = dragLMBend = new Vector3(0, 0, 0);   //reset the position
            }
            else
            {
                //null
            }

            if (!b_IsRMB && Input.GetMouseButton(1))           //RMB down
            {
                b_IsRMB = true;

                //increments of 10
                //int temp = Mathf.Clamp(testGrid.arrayHeat.getValue(mousePos) - 10, mapData.getMinTemp(), mapData.getMaxTemp());
                //testGrid.arrayHeat.setGridObject(mousePos, temp);

                //testGrid.arrayAccess.setValue(mousePos, 0);

                //testGrid.pathfindingGrid.setGridObject(mousePos, 1);
            }
            else if (b_IsRMB && Input.GetMouseButton(1))       //RMB pressed, exclude first and last frame
            {

            }
            else if (b_IsRMB && !Input.GetMouseButton(1))      //RMB up
            {
                b_IsRMB = false;
            }
            else
            {
                //null
            }

            if (!b_IsMMB && Input.GetMouseButton(2))           //MMB down
            {
                b_IsMMB = true;
            }
            else if (b_IsMMB && Input.GetMouseButton(2))       //MMB pressed, exclude first and last frame
            {

            }
            else if (b_IsMMB && !Input.GetMouseButton(2))      //MMB up
            {
                b_IsMMB = false;
            }
            else
            {
                //null
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
