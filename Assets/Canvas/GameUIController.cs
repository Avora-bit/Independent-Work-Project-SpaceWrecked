using System.ComponentModel;
using UnityEngine;

public class GameUIController : MonoBehaviour
{
    //selector for which layer on tool bar is created and rendered


    void Awake()
    {
        //on awake, set every layer to false
    }

    // Update is called once per frame
    void Update()
    {
        //only when the toolbar is selected, then open the layer
        //else set the rest to false
    }
}
