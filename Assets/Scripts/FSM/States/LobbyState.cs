using System;
using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UniRx;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;


public class LobbyState : IState, IConnectionCallbacks, IInRoomCallbacks, IOnEventCallback, IMatchmakingCallbacks
{
    public event EventHandler<StateBeginExitEventArgs> OnBeginExit;

    private const byte WaitForGameStartEvent = 0;

    private UnityEvent OnConnectedToServerEvent, OnDisconnectEvent, OnJoinedRoomEvent;

    private PlayerUnityEvent OnPlayerEnteredRoomEvent;

    private GameObject ui, searchingPanel;

    private Button findRoomButton, cancelButton;

    private Text player1Text, player2Text, nickNameText, logText;

    private ReactiveCommand FindRoomCommand, LeaveRoomCommand, ConnectToPhotonNetwork;

    private ReactiveProperty<bool> FindingGame, ConnectedToServer;

    private ReactiveProperty<string> Player1NickName, Player2NickName;

    private PlayerConfig playerConfig;

    private CompositeDisposable disposables, connectionDisposables;

    public LobbyState(PlayerConfig playerConfig)
    {
        this.playerConfig = playerConfig;

        OnConnectedToServerEvent = new UnityEvent();
        OnDisconnectEvent = new UnityEvent();
        OnPlayerEnteredRoomEvent = new PlayerUnityEvent();
        OnJoinedRoomEvent = new UnityEvent();

        FindRoomCommand = new ReactiveCommand();
        LeaveRoomCommand = new ReactiveCommand();
        ConnectToPhotonNetwork = new ReactiveCommand();

        ConnectedToServer = new ReactiveProperty<bool>();
        FindingGame = new ReactiveProperty<bool>(false);
        Player1NickName = new ReactiveProperty<string>(playerConfig.NickName);
        Player2NickName = new ReactiveProperty<string>();

        disposables = new CompositeDisposable();
        connectionDisposables = new CompositeDisposable();
    }

    public void BeginEnter()
    {
        InitUI();

        SubscribeOnObservables();

        PhotonNetwork.AddCallbackTarget(this);

        ConnectToPhotonNetwork.Execute();
    }

    private void InitUI()
    {
        ui = GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/UI/Menu UI"));
        findRoomButton = ui.transform.Find("Find Room(1x1)").GetComponent<Button>();
        searchingPanel = ui.transform.Find("Searching Panel").gameObject;
        cancelButton = searchingPanel.transform.Find("BG/Cancel Button").GetComponent<Button>();
        player1Text = searchingPanel.transform.Find("BG/Player1").GetComponent<Text>();
        player2Text = searchingPanel.transform.Find("BG/Player2").GetComponent<Text>();
        nickNameText = ui.transform.Find("User Info/BG/NickName").GetComponent<Text>();
        logText = ui.transform.Find("Log/Text").GetComponent<Text>();
    }

    private void SubscribeOnObservables()
    {
        ConnectedToServer.Where(val => val).Subscribe(_ => SubscribeOnConnectionDependentObservables())
            .AddTo(disposables);
        ConnectedToServer.Where(val => !val).Subscribe(_ => connectionDisposables.Clear()).AddTo(disposables);

        ConnectedToServer.Subscribe(val => findRoomButton.interactable = val).AddTo(disposables);

        FindingGame.Subscribe(val => searchingPanel.SetActive(val)).AddTo(disposables);

        Player1NickName.SubscribeToText(nickNameText).AddTo(disposables);
        Player1NickName.SubscribeToText(player1Text).AddTo(disposables);
        Player2NickName.SubscribeToText(player2Text).AddTo(disposables);

        ConnectToPhotonNetwork.Subscribe(_ =>
        {
            PhotonNetwork.GameVersion = "1";
            PhotonNetwork.AutomaticallySyncScene = true;
            PhotonNetwork.ConnectUsingSettings();
        }).AddTo(disposables);

        OnConnectedToServerEvent.AsObservable().Subscribe(_ =>
        {
            ConnectedToServer.Value = true;
            Log("Connected to the server!");
        }).AddTo(disposables);

        OnDisconnectEvent.AsObservable().Subscribe(_ =>
        {
            ConnectedToServer.Value = false;
            ConnectToPhotonNetwork.Execute();
        }).AddTo(disposables);
    }

    private void SubscribeOnConnectionDependentObservables()
    {
        OnJoinedRoomEvent.AsObservable().SubscribeWithState(PhotonNetwork.CurrentRoom, (unit, room) =>
            PhotonNetwork.SetPlayerCustomProperties(
                new ExitGames.Client.Photon.Hashtable()
                    {{"Team", room.PlayerCount == 1 ? "Bottom" : "Top"}})
        ).AddTo(connectionDisposables);

        OnPlayerEnteredRoomEvent.AsObservable().Where(_ => PhotonNetwork.LocalPlayer.IsMasterClient).Where(_ =>
            PhotonNetwork.CurrentRoom.PlayerCount == PhotonNetwork.CurrentRoom.MaxPlayers).Subscribe(player =>
        {
            Player2NickName.Value = player.NickName;

            int time = 5;

            PhotonNetwork.RaiseEvent(WaitForGameStartEvent, new object[] {time},
                new RaiseEventOptions() {Receivers = ReceiverGroup.All},
                SendOptions.SendReliable);
        }).AddTo(connectionDisposables);

        FindRoomCommand.Where(_ => PhotonNetwork.CountOfRooms > 0).Subscribe(_ =>
        {
            FindingGame.Value = true;
            Player2NickName.Value = "";

            PhotonNetwork.JoinRandomRoom();
        }).AddTo(connectionDisposables);

        FindRoomCommand.Where(_ => PhotonNetwork.CountOfRooms == 0).Subscribe(_ =>
        {
            FindingGame.Value = true;
            Player2NickName.Value = "";

            PhotonNetwork.CreateRoom(null,
                new Photon.Realtime.RoomOptions() {MaxPlayers = 2, BroadcastPropsChangeToAll = false});
        }).AddTo(connectionDisposables);

        LeaveRoomCommand.Subscribe(_ =>
        {
            PhotonNetwork.LeaveRoom();
            Delay(5, () => { FindingGame.Value = false; });
        }).AddTo(disposables);

        FindRoomCommand.BindTo(findRoomButton).AddTo(connectionDisposables);
        LeaveRoomCommand.BindTo(cancelButton).AddTo(connectionDisposables);
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
        connectionDisposables?.Clear();
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    private void Log(string text)
    {
        logText.text += "\n" + text;
        Debug.Log(text);
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

    public void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code != WaitForGameStartEvent) return;

        object[] data = (object[]) photonEvent.CustomData;

        Delay((int) data[0], () =>
        {
            StateBeginExitEventArgs args = new StateBeginExitEventArgs(new GameState(), new ScreenFadeTransition(0.5f));
            OnBeginExit.Invoke(this, args);
        });
    }

    public void OnConnected()
    {
    }

    public void OnConnectedToMaster()
    {
        OnConnectedToServerEvent.Invoke();
    }

    public void OnDisconnected(DisconnectCause cause)
    {
        OnDisconnectEvent.Invoke();
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

    public void OnPlayerEnteredRoom(Player newPlayer)
    {
        OnPlayerEnteredRoomEvent.Invoke(newPlayer);
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
}