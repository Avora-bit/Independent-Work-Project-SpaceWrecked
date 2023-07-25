using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ArchitectSelector : MonoBehaviour
{
    public GameObject buildTab;
    TextMeshProUGUI itemData;
    Button btn_Structure, btn_Production, btn_Needs, btn_Ship, btn_Others;

    string structureString = "Structure, Keep out the Vaccumm";
    string productionString = "Production & Machinery... Automation!";
    string needsString = "Care for your Colonists' Needs";
    string shipString = "Vital Ship Structures";
    string othersString = "Others";

    void Awake()
    {
        itemData = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        Transform tabSelector = transform.GetChild(1);

        btn_Structure = tabSelector.Find("Btn_Structure").GetComponent<Button>();
        btn_Structure.onClick.AddListener(toggleStructure);

        btn_Production = tabSelector.Find("Btn_Production").GetComponent<Button>();
        btn_Production.onClick.AddListener(toggleProduction);

        btn_Needs = tabSelector.Find("Btn_Needs").GetComponent<Button>();
        btn_Needs.onClick.AddListener(toggleNeeds);

        btn_Ship = tabSelector.Find("Btn_Ship").GetComponent<Button>();
        btn_Ship.onClick.AddListener(toggleShip);

        btn_Others = tabSelector.Find("Btn_Others").GetComponent<Button>();
        btn_Others.onClick.AddListener(toggleOthers);
    }

    public void toggleStructure()
    {
        if (buildTab.transform.GetChild(0).gameObject.activeSelf == false)
        {
            resetTabs();
            buildTab.transform.GetChild(0).gameObject.SetActive(true);
            itemData.text = structureString;
        }
        else
        {
            resetTabs();
        }
    }
    public void toggleProduction()
    {
        if (buildTab.transform.GetChild(1).gameObject.activeSelf == false)
        {
            resetTabs();
            buildTab.transform.GetChild(1).gameObject.SetActive(true);
            itemData.text = productionString;
        }
        else
        {
            resetTabs();
        }
    }
    public void toggleNeeds()
    {
        if (buildTab.transform.GetChild(2).gameObject.activeSelf == false)
        {
            resetTabs();
            buildTab.transform.GetChild(2).gameObject.SetActive(true);
            itemData.text = needsString;
        }
        else
        {
            resetTabs();
        }
    }
        public void toggleShip()
    {
        if (buildTab.transform.GetChild(3).gameObject.activeSelf == false)
        {
            resetTabs();
            buildTab.transform.GetChild(3).gameObject.SetActive(true);
            itemData.text = shipString;
        }
        else
        {
            resetTabs();
        }
    }
    public void toggleOthers()
    {
        if (buildTab.transform.GetChild(4).gameObject.activeSelf == false)
        {
            resetTabs();
            buildTab.transform.GetChild(4).gameObject.SetActive(true);
            itemData.text = othersString;
        }
        else
        {
            resetTabs();
        }
    }
    public void resetTabs()
    {
        itemData.text = "";
        foreach (Transform child in buildTab.transform)
        {
            child.gameObject.SetActive(false);
        }
        FindAnyObjectByType<PlayerController>().ResetControl();
    }
}
