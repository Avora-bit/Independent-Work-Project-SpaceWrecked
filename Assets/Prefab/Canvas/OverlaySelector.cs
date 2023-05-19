using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OverlaySelector : MonoBehaviour
{
    Button btn_Heat, btn_Access, btn_Path;

    void Awake()
    {
        TestGrid testGrid = FindObjectOfType<TestGrid>();

        btn_Heat = gameObject.transform.Find("Btn_Temp").GetComponent<Button>();
        btn_Heat.onClick.AddListener(testGrid.toggleHeat);

        btn_Access = gameObject.transform.Find("Btn_Structure").GetComponent<Button>();
        btn_Access.onClick.AddListener(testGrid.toggleAccess);

        btn_Path = gameObject.transform.Find("Btn_Pathfinding").GetComponent<Button>();
        btn_Path.onClick.AddListener(testGrid.togglePathfinding);
    }

    void Update()
    {
        //change color of button to show state
    }
}
