using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildHint : MonoBehaviour
{
    //behavior class to change color based on state of the tile
    public bool state;
    public void Check(bool state)
    {
        this.state = state;
        if (state) gameObject.GetComponent<SpriteRenderer>().color = new Color(0f, 1f, 0f, 150 / 255f);         //green
        else gameObject.GetComponent<SpriteRenderer>().color = new Color(1f, 0f, 0f, 150 / 255f);               //red
    }
}
