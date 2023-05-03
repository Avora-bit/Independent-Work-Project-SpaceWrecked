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
    Button btn_Pause, btn_Play, btn_Fast, btn_Faster, btn_Fastest;

    void Awake()
    {
        timeController = Object.FindObjectOfType<TimeController>();
        timeText = gameObject.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>();

        GameObject timeScale = gameObject.transform.GetChild(1).gameObject;
        btn_Pause = timeScale.transform.Find("Btn_Pause").GetComponent<Button>();
        btn_Play = timeScale.transform.Find("Btn_Play").GetComponent<Button>();
        btn_Fast = timeScale.transform.Find("Btn_Fast").GetComponent<Button>();
        btn_Faster = timeScale.transform.Find("Btn_Faster").GetComponent<Button>();
        btn_Fastest = timeScale.transform.Find("Btn_Fastest").GetComponent<Button>();

        btn_Pause.onClick.AddListener(timeController.pauseToggle);
        btn_Play.onClick.AddListener(timeController.setTimeNormal);
        btn_Fast.onClick.AddListener(timeController.setTimeFast);
        btn_Faster.onClick.AddListener(timeController.setTimeFaster);
        btn_Fastest.onClick.AddListener(timeController.setTimeFastest);


        //add listener to buttons
    }

    // Update is called once per frame
    void Update()
    {
        //read from timecontroller and display time on time text
        timeText.text = timeController.getTime();

        //listen to key press and change time scale
    }
}
