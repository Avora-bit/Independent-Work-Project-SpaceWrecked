using UnityEngine;
using UnityEngine.UI;

public class OverlaySelector : MonoBehaviour
{
    Button btn_Heat, btn_Radiaton, btn_Oxygen;

    void Awake()
    {
        MasterGrid testGrid = FindObjectOfType<MasterGrid>();

        btn_Heat = gameObject.transform.Find("Btn_Temp").GetComponent<Button>();
        btn_Heat.onClick.AddListener(testGrid.toggleHeat);

        btn_Radiaton = gameObject.transform.Find("Btn_Radiation").GetComponent<Button>();
        btn_Radiaton.onClick.AddListener(testGrid.toggleRadiation);

        btn_Oxygen = gameObject.transform.Find("Btn_Oxygen").GetComponent<Button>();
        btn_Oxygen.onClick.AddListener(testGrid.toggleOxygen);
    }
}
