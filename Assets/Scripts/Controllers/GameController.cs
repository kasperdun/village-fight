using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UniRx;
using UnityEngine;

public class GameController : MonoBehaviour
{
    private void Start()
    {
        InitStateMachine();
    }
    private void InitStateMachine()
    {
        PlayerModel playerModel = new PlayerModel(PlayerPrefs.GetString("NickName", "Player"), 0);
        IStateMachine stateMachine = new StateMachine(new LobbyState(new PlayerConfig()));
        StartCoroutine(stateMachine.Execute().GetEnumerator());
    }
}