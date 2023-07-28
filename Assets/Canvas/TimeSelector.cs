using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TimeSelector : MonoBehaviour
{
    //this script should read the time and display it in a child gameobject called timetext
    //this script should also get a reference to all btn child objects to assign the timescale index

    TimeController timeController;

    TextMeshProUGUI timeText;
    Button btn_Slow, btn_Pause, btn_Play, btn_Fast, btn_Faster;

    void Awake()
    {
        timeController = FindObjectOfType<TimeController>();
        timeText = gameObject.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>();
        GameObject timeScale = gameObject.transform.GetChild(1).gameObject;

        btn_Slow = timeScale.transform.Find("Btn_Slow").GetComponent<Button>();
        btn_Slow.onClick.AddListener(timeController.setTimeSlow);

        btn_Pause = timeScale.transform.Find("Btn_Pause").GetComponent<Button>();
        btn_Pause.onClick.AddListener(timeController.pauseToggle);

        btn_Play = timeScale.transform.Find("Btn_Play").GetComponent<Button>();
        btn_Play.onClick.AddListener(timeController.setTimeNormal);

        btn_Fast = timeScale.transform.Find("Btn_Fast").GetComponent<Button>();
        btn_Fast.onClick.AddListener(timeController.setTimeFast);

        btn_Faster = timeScale.transform.Find("Btn_Faster").GetComponent<Button>();
        btn_Faster.onClick.AddListener(timeController.setTimeFaster);

    }
    void Update()
    {
        timeText.text = timeController.getTime();

        //change button color to show selected state
    }
}
