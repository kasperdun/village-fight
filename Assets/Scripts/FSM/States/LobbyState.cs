using System;
using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class LobbyState : IState, IConnectionCallbacks, IInRoomCallbacks, IOnEventCallback, IMatchmakingCallbacks
{
    public event EventHandler<StateBeginExitEventArgs> OnBeginExit;

    private const byte WaitForGameStartEvent = 0;

    private GameObject ui, searchingPanel;

    private Button findRoomButton, cancelButton;

    private Text player1Text, player2Text, nickNameText, logText;

    private ReactiveCommand FindRoomCommand, LeaveRoomCommand, StartGameCommand;

    private ReactiveProperty<bool> FindingGame;

    private ReactiveProperty<string> Player1NickName, Player2NickName;

    private PlayerConfig playerConfig;

    private CompositeDisposable disposables;
    
    public LobbyState(PlayerConfig playerConfig)
    {
        this.playerConfig = playerConfig;
        
        FindRoomCommand = new ReactiveCommand();
        LeaveRoomCommand = new ReactiveCommand();
        
        FindingGame = new ReactiveProperty<bool>(false);
        Player1NickName = new ReactiveProperty<string>(playerConfig.NickName);
        Player2NickName = new ReactiveProperty<string>();

        disposables = new CompositeDisposable();
    }

    public void BeginEnter()
    {
        PhotonNetwork.AddCallbackTarget(this);

        ConnectToPhotonNetwork();
        
        InitUI();
        
        DoSubscriptions();
    }
    
    private void ConnectToPhotonNetwork()
    {
        PhotonNetwork.GameVersion = "1";
        PhotonNetwork.AutomaticallySyncScene = true;

        Observable.Interval(TimeSpan.FromSeconds(2)).Take(1).Subscribe(_ => PhotonNetwork.ConnectUsingSettings());
    }

    private void InitUI()
    {
        ui = GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/UI/Main Menu"));
        findRoomButton = ui.transform.Find("Find Room(1x1)").GetComponent<Button>();
        searchingPanel = ui.transform.Find("Searching Panel").gameObject;
        cancelButton = searchingPanel.transform.Find("BG/Cancel Button").GetComponent<Button>();
        player1Text = searchingPanel.transform.Find("BG/Player1").GetComponent<Text>();
        player2Text = searchingPanel.transform.Find("BG/Player2").GetComponent<Text>();
        nickNameText = ui.transform.Find("User Info/BG/NickName").GetComponent<Text>();
        logText = ui.transform.Find("Log/Text").GetComponent<Text>();
    }

    private void DoSubscriptions()
    {
        FindRoomCommand.BindTo(findRoomButton).AddTo(disposables);
        LeaveRoomCommand.BindTo(cancelButton).AddTo(disposables);
        
        FindingGame.Subscribe(val => searchingPanel.SetActive(val)).AddTo(disposables);

        Player1NickName.SubscribeToText(nickNameText).AddTo(disposables);
        Player1NickName.SubscribeToText(player1Text).AddTo(disposables);
        Player2NickName.SubscribeToText(player2Text).AddTo(disposables);

        FindRoomCommand.Where(_ => PhotonNetwork.CountOfRooms > 0).Subscribe(_ =>
        {
            FindingGame.Value = true;
            Player2NickName.Value = "";
            
            PhotonNetwork.JoinRandomRoom();
        }).AddTo(disposables);
        
        FindRoomCommand.Where(_ => PhotonNetwork.CountOfRooms == 0).Subscribe(_ =>
        {
            FindingGame.Value = true;
            Player2NickName.Value = "";
            
            PhotonNetwork.CreateRoom(null,
                new Photon.Realtime.RoomOptions() {MaxPlayers = 2, BroadcastPropsChangeToAll = false});
        }).AddTo(disposables);

        LeaveRoomCommand.Subscribe(_ =>
        {
            PhotonNetwork.LeaveRoom();
            Delay(5, () => { FindingGame.Value = false;});
        }).AddTo(disposables);
    }

    public void EndEnter()
    {
        
    }

    public IEnumerable Execute()
    {
        yield return null;
    }

    public void EndExit()
    {
        GameObject.Destroy(ui);
        disposables.Clear();
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    public void OnConnected()
    {
        
    }

    public void OnConnectedToMaster()
    {
        findRoomButton.interactable = true;
        Log("Connected to server!");

    }

    public void OnDisconnected(DisconnectCause cause)
    {
        ConnectToPhotonNetwork();
    }

    public void OnRegionListReceived(RegionHandler regionHandler)
    {
        
    }

    public void OnCustomAuthenticationResponse(Dictionary<string, object> data)
    {
        
    }

    public void OnCustomAuthenticationFailed(string debugMessage)
    {
        
    }

    private void Delay(int time, Action callback)
    {
        cancelButton.interactable = false;
        
        Text buttonText = cancelButton.GetComponentInChildren<Text>();
        ReactiveProperty<int> delay = new ReactiveProperty<int>(time);
        IDisposable delayDesposable = delay.SubscribeToText(buttonText);

        IObservable<long> interval = Observable.Interval(TimeSpan.FromSeconds(1));
        interval.Take(time).Subscribe(_ => delay.Value--);

        interval.Where(x => x == time).Subscribe(_ =>
        {
            cancelButton.interactable = true;
            callback?.Invoke();

            delayDesposable.Dispose();
            buttonText.text = "Cancel";
        });
    }

    public void OnPlayerEnteredRoom(Player newPlayer)
    {
        player2Text.text = newPlayer.NickName;
        
        if (PhotonNetwork.LocalPlayer.IsMasterClient && PhotonNetwork.CurrentRoom.PlayerCount == PhotonNetwork.CurrentRoom.MaxPlayers)
        {
            PhotonNetwork.RaiseEvent(WaitForGameStartEvent, new {Time = 5}, new RaiseEventOptions() {Receivers = ReceiverGroup.All},
                SendOptions.SendReliable);
        }
    }

    public void OnPlayerLeftRoom(Player otherPlayer)
    {
        
    }

    public void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        
    }

    public void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        
    }

    public void OnMasterClientSwitched(Player newMasterClient)
    {
        
    }

    public void OnEvent(EventData photonEvent)
    {
        if(photonEvent.Code != WaitForGameStartEvent) return;
        Delay((int) photonEvent.CustomData.GetType().GetProperty("Time").GetValue(photonEvent.CustomData), () =>  PhotonNetwork.LoadLevel("Game"));
    }

    public void OnFriendListUpdate(List<FriendInfo> friendList)
    {
        
    }

    public void OnCreatedRoom()
    {
        
    }

    public void OnCreateRoomFailed(short returnCode, string message)
    {
        
    }

    public void OnJoinedRoom()
    {
        PhotonNetwork.SetPlayerCustomProperties(new ExitGames.Client.Photon.Hashtable() { { "Team", PhotonNetwork.CurrentRoom.PlayerCount == 1 ? "Bottom" : "Top"} });
    }

    public void OnJoinRoomFailed(short returnCode, string message)
    {
        
    }

    public void OnJoinRandomFailed(short returnCode, string message)
    {
        
    }

    public void OnLeftRoom()
    {
        
    }
    
    private void Log(string text)
    {
        logText.text += "\n" + text;
        Debug.Log(text);
    }
}
