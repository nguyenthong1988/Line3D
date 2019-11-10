using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HUDManager : Singleton<HUDManager>
{
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI highScoreText;

    [Header("Panel")]
    public BeginGameUI beginGameUI;
    public EndGameUI endGameUI;

    public virtual void Initialize()
    {

    }

    public void UpdatePlayedTime()
    {
        if(timeText)
        {
            int playedTime = (int)GameManager.instance.playedTime;
            int minutes = playedTime / 60;
            int seconds = playedTime % 60;
            timeText.text = "TIME: " + minutes.ToString("D2") + ":" + seconds.ToString("D2");
        }
    }

    public void UpdateScore()
    {
        scoreText?.SetText(string.Format("SCORE: {0}", GameManager.instance.score));
    }

    public void UpdateHighScore()
    {
        highScoreText?.SetText(string.Format("SCORE: {0}", GameManager.instance.score));
    }

    public void ShowBeginGame()
    {
        beginGameUI?.gameObject.SetActive(true);
    }

    public void ShowEndGame()
    {
        endGameUI?.gameObject.SetActive(true);
    }
}
