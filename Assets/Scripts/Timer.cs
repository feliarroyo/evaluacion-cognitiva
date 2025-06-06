using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class Timer : MonoBehaviour
{
    public static int time = 10;
    public static int criticalTime = 5; // when should beep sounds be played
    private static float timeLeft;
    public static bool timerOn = false;
    public static int spentTime = 0;
    private TextMeshProUGUI timerText;

    // Start is called before the first frame update
    void Start()
    {
        timerOn = false;
        timeLeft = time;
        timerText = gameObject.GetComponent<TextMeshProUGUI>();
        UpdateTimer(timeLeft);
    }

    // Update is called once per frame
    void Update()
    {
        if (timerOn){
            if (timeLeft > 0) {
                timeLeft -= Time.deltaTime;
                UpdateTimer(timeLeft);
            }
            else {
                Logging.DebugLog("Time over");
                timeLeft = 0;
                timerText.text = MinuteNotation(0);
                timerOn = false;
                if ((GameStatus.currentPhase == GameStatus.GamePhase.Memorizing) 
                    || (GameStatus.currentPhase == GameStatus.GamePhase.Search) || (GameStatus.currentPhase == GameStatus.GamePhase.Tutorial_Memorizing))
                    GameStatus.SetNextPhase();
            }
        }
    }

    public static bool IsTimeOver()
    {
        return timeLeft == 0.0f;
    }

    private void UpdateTimer(float currentTime){
        currentTime += 1;
        float seconds = Mathf.FloorToInt(currentTime % (time + 1));
        //if (hasTickSound && (seconds.ToString() != timerText.text) && (seconds.ToString() != "0") && (seconds <= criticalTime)) 
        //    soundPlayer.PlayOneShot(tickSound);
        timerText.text = MinuteNotation(seconds);
        //if (seconds == 5) {
            //timerText.color = new Color(1f, 0.39f, 0f, 1f);
        //}
    }

    private string MinuteNotation(float number){
        int minutes = (int) number / 60;
        int seconds = (int) number % 60;
        string minString = (minutes < 10? "0" : "") + minutes.ToString();
        string secString = (seconds < 10? "0" : "") + seconds.ToString();
        string str = minString  + " : " + secString;
        return str;
    }

    public static void StartTimer(int seconds){
        time = seconds;
        timeLeft = seconds;
        timerOn = true;
    }

    public static void StopTimer(){
        spentTime = (int) (time - timeLeft);
        SetTime(0);
    }

    public static void SetTime(int newTime){
        time = newTime;
        timeLeft = newTime;
    }
}
