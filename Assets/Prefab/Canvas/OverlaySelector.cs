using UnityEngine;
using UnityEngine.UI;

public class OverlaySelector : MonoBehaviour
{
    Button btn_Heat, btn_Access, btn_Radiaton, btn_Oxygen;

    void Awake()
    {
        MasterGrid testGrid = FindObjectOfType<MasterGrid>();

        btn_Heat = gameObject.transform.Find("Btn_Temp").GetComponent<Button>();
        btn_Heat.onClick.AddListener(testGrid.toggleHeat);

        btn_Access = gameObject.transform.Find("Btn_Access").GetComponent<Button>();
        btn_Access.onClick.AddListener(testGrid.toggleAccess);

        btn_Radiaton = gameObject.transform.Find("Btn_Radiation").GetComponent<Button>();
        btn_Radiaton.onClick.AddListener(testGrid.toggleRadiation);

        btn_Oxygen = gameObject.transform.Find("Btn_Oxygen").GetComponent<Button>();
        btn_Oxygen.onClick.AddListener(testGrid.toggleOxygen);
    }

    void Update()
    {
        //change color of button to show state
    }
}
