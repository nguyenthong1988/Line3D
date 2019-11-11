﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EndGameUI : MonoBehaviour
{
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI highScoreText;

    public void OnShowEndGame()
    {
        if (timeText)
        {
            int playedTime = (int)GameManager.instance.playedTime;
            int minutes = playedTime / 60;
            int seconds = playedTime % 60;
            timeText.text = "TIME: " + minutes.ToString("D2") + ":" + seconds.ToString("D2");
        }

        scoreText?.SetText(string.Format("SCORE: {0}", GameManager.instance.score));
        highScoreText?.SetText(string.Format("HIGH SCORE: {0}", GameManager.instance.highScore));
    }

    public void OnResetButtonClick()
    {
        GameManager.instance.ChangeGamestate(GameManager.GameState.PreGame);
    }
}
