using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TimeController : BaseSingleton<TimeController>
{
    private float dElapsedTime;            //tracking game time elapsed
    private float dayTime;                 //tracking current time in the day
    //assumes an effective 1:60 time ratio, 1 day cycle is 24 minutes
    //there is no seasons, months, years and other concepts of time blocks. 
    private int days, hours, minutes;

    private float[] timeScale = {1f, 2f, 4f, 8f};
    private int index = 0;          //default rate of time
    private bool togglePause = false;

    // Update is called once per frame
    void Update()
    {
        dElapsedTime += Time.deltaTime;
        dayTime += Time.deltaTime;
        if (dayTime >= 60 * 24)                     //14400 seconds
        {
            days += 1;
            dayTime = 0;
        }
    }

    public string getTime()
    {
        hours = (int)Mathf.Floor((dElapsedTime - days * 14400) / 60);
        minutes = (int)Mathf.Floor(dElapsedTime - (days * 14400) - (hours * 60));
        return string.Format("{0} days\n{1:00}{2:00}", days, hours, minutes);
    }

    public int getDays() { return days; }
    public int getHours() { return hours; }
    public int getMinutes() { return minutes; }

    public void pauseToggle() { 
        togglePause = !togglePause;
        if (togglePause) Time.timeScale = 0f;
        else Time.timeScale = timeScale[index];
    } //does not change index so the next toggle will set the timescale back to before. 

    public void setTimeNormal() { index = 0; togglePause = false; Time.timeScale = timeScale[index]; }
    public void setTimeFast() { index = 1; togglePause = false; Time.timeScale = timeScale[index]; }
    public void setTimeFaster() { index = 2; togglePause = false; Time.timeScale = timeScale[index]; }
    public void setTimeFastest() { index = 3; togglePause = false; Time.timeScale = timeScale[index]; }

}