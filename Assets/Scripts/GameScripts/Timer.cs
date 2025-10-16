using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using System.Text;

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
                timeLeft = 0;
                timerText.text = MinuteNotation(0);
                timerOn = false;
                if ((GameStatus.currentPhase == GameStatus.GamePhase.Memorizing) 
                    || (GameStatus.currentPhase == GameStatus.GamePhase.Search) || (GameStatus.currentPhase == GameStatus.GamePhase.Tutorial_Memorizing) || (GameStatus.currentPhase == GameStatus.GamePhase.Tutorial_Search))
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
        timerText.text = MinuteNotation(seconds);
    }

    private string MinuteNotation(float number){
        int minutes = (int) number / 60;
        int seconds = (int) number % 60;
        StringBuilder sb = new();
        sb.Append(minutes < 10? "0" : "").Append(minutes).Append(" : ").Append(seconds < 10? "0" : "").Append(seconds);
        return sb.ToString();
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
