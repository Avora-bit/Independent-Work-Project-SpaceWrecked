using UnityEngine;
using UnityEngine.UI;

public class SettingSelector : MonoBehaviour
{
    Button btn_Audio, btn_Graphics, btn_Controls, btn_Back;
    GameObject[] childrenPage;

    int state = 0;

    void Awake()
    {
        childrenPage[0] = transform.parent.Find("Audio").gameObject;
        childrenPage[1] = transform.parent.Find("Graphics").gameObject;
        childrenPage[2] = transform.parent.Find("Controls").gameObject;

        btn_Audio = gameObject.transform.Find("Btn_Audio").GetComponent<Button>();
        btn_Audio.onClick.AddListener(setAudio);

        btn_Graphics = gameObject.transform.Find("Btn_Graphics").GetComponent<Button>();
        btn_Graphics.onClick.AddListener(setGraphics);

        btn_Controls = gameObject.transform.Find("Btn_Controls").GetComponent<Button>();
        btn_Controls.onClick.AddListener(setControls);

        btn_Back = gameObject.transform.Find("Btn_Back").GetComponent<Button>();
        btn_Back.onClick.AddListener(back);

        //load stored values 

        setAudio();         //default on audio
    }

    void Update()
    {
        
    }

    private void setAudio()
    { 
        state = 0;
        childrenPage[0].SetActive(true);
        childrenPage[1].SetActive(false);
        childrenPage[2].SetActive(false);
    }
    private void setGraphics()
    {
        state = 1;
        childrenPage[0].SetActive(false);
        childrenPage[1].SetActive(true);
        childrenPage[2].SetActive(false);
    }
    private void setControls()
    {
        state = 2;
        childrenPage[0].SetActive(false);
        childrenPage[1].SetActive(false);
        childrenPage[2].SetActive(true);
    }
    private void back()
    {
        //close canvas
    }
}
