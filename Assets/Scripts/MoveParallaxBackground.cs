using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveParallaxBackground : MonoBehaviour
{
    private float startPosX, startPosY;
    
    [SerializeField] private float MoveSpeed = 0;
    private GameObject camera;
    
    // Start is called before the first frame update
    void Awake()
    {
        camera = GameObject.Find("Camera System");
        startPosX = transform.position.x;
        startPosY = transform.position.y;
    }

    // Update is called once per frame
    void Update()
    {
        float distanceX = camera.transform.position.x * MoveSpeed;
        float distancey = camera.transform.position.y * MoveSpeed;
        transform.position = new Vector3(distanceX, distancey, transform.position.z);
    }
}
