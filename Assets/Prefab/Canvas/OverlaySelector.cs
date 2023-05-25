using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OverlaySelector : MonoBehaviour
{
    Button btn_Heat, btn_Access;

    void Awake()
    {
        TestGrid testGrid = FindObjectOfType<TestGrid>();

        btn_Heat = gameObject.transform.Find("Btn_Temp").GetComponent<Button>();
        btn_Heat.onClick.AddListener(testGrid.toggleHeat);

        btn_Access = gameObject.transform.Find("Btn_Access").GetComponent<Button>();
        btn_Access.onClick.AddListener(testGrid.toggleAccess);
    }

    void Update()
    {
        //change color of button to show state
    }
}
