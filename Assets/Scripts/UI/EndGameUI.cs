using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndGameUI : MonoBehaviour
{
    public void OnResetButtonClick()
    {
        GameManager.instance.ChangeGamestate(GameManager.GameState.PreGame);
    }
}
