using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    public GameObject searchingPanel;
    public Button findGameButton;
    public Text logText;

    private Text player1Text;
    private Text player2Text;

    // Start is called before the first frame update
    void Start()
    {
        Log("Connecting with nick: " + GameManager.nickName);
        PhotonNetwork.GameVersion = "1";
        PhotonNetwork.AutomaticallySyncScene = true;
        StartCoroutine(ConnectDelay());

        var texts = searchingPanel.GetComponentsInChildren<Text>();
        player1Text = texts.First(t => t.name == "Player1");
        player2Text = texts.First(t => t.name == "Player2");
    }

    public override void OnConnectedToMaster()
    {
        findGameButton.interactable = true;
        Log("Connected to server!");
    }

    public void StartFindGame()
    {
        searchingPanel.SetActive(true);
        player1Text.text = PhotonNetwork.NickName;
        player2Text.text = "";
        Log("Rooms count: " + PhotonNetwork.CountOfRooms);
        if(PhotonNetwork.CountOfRooms > 0)
        {
            JoinToRoom();
        }
        else
        {
            CreateRoom();
        }
    }

    public void CreateRoom()
    {
        PhotonNetwork.CreateRoom(null, new Photon.Realtime.RoomOptions() { MaxPlayers = 2, BroadcastPropsChangeToAll = false });
    }

    public void JoinToRoom()
    {
        PhotonNetwork.JoinRandomRoom();
    }
    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        StartCoroutine(LeaveDelay());
    }

    private IEnumerator LeaveDelay()
    {
        Button btn = searchingPanel.GetComponentInChildren<Button>();
        Text btnText = btn.GetComponentInChildren<Text>();
        string oldText = btnText.text;
        btn.interactable = false;

        for (int i = 5; i > 0; i--)
        {
            btnText.text = i.ToString();
            yield return new WaitForSeconds(1f);
        }
        searchingPanel.SetActive(false);
        btnText.text = oldText;
        btn.interactable = true;
    }

    private IEnumerator StartGameDelay()
    {
        Button btn = searchingPanel.GetComponentInChildren<Button>();
        Text btnText = btn.GetComponentInChildren<Text>();
        string oldText = btnText.text;
        btn.interactable = false;

        btnText.text = "Starting";
        
        PhotonNetwork.LoadLevel("Game");

        yield return new WaitForSeconds(5f);

        btnText.text = oldText;
        btn.interactable = true;
    }

    private IEnumerator ConnectDelay()
    {
        yield return new WaitForSeconds(2f);
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnJoinedRoom()
    {
        Log("I Joined to room: " + PhotonNetwork.CurrentRoom.Name );

        if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
        {
            PhotonNetwork.SetPlayerCustomProperties(new ExitGames.Client.Photon.Hashtable() { { "Team", "Bottom" } });
        }
        else
        {
            PhotonNetwork.SetPlayerCustomProperties(new ExitGames.Client.Photon.Hashtable() { { "Team", "Top" } });
            player2Text.text = PhotonNetwork.PlayerListOthers[0].NickName;
        }
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        findGameButton.interactable = false;
        Log("Disconnected: " + cause.ToString());
        StartCoroutine(ConnectDelay());
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Log(message);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Log(newPlayer.NickName + "Joined to room");
        player2Text.text = newPlayer.NickName;

        if (PhotonNetwork.LocalPlayer.IsMasterClient && PhotonNetwork.CurrentRoom.PlayerCount == PhotonNetwork.CurrentRoom.MaxPlayers)
        {
            photonView.RPC(nameof(StartGameRPC), RpcTarget.All);
        }
    }

    [PunRPC]
    public void StartGameRPC()
    {
        StartCoroutine(StartGameDelay());
    }
    private void Log(string text)
    {
        logText.text += "\n" + text;
        Debug.Log(text);
    }
}
