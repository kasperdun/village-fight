using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UniRx;
using UnityEngine;

public class GameController : MonoBehaviour
{
    private int GameControllerSpawned
    {
        get => PlayerPrefs.GetInt("GameControllerSpawned", 0);
        set => PlayerPrefs.SetInt("GameControllerSpawned",value);
    }

    private void Awake()
    {
        DontDestroyOnLoad(this);
        
        if(GameControllerSpawned == 1) 
            Destroy(gameObject);

        GameControllerSpawned = 1;

    }
    
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

    private void OnApplicationQuit()
    {
        GameControllerSpawned = 0;
    }
}