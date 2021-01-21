using Photon.Pun;
using UnityEngine;

public class VictoryDialog : MonoBehaviour
{
    public void Back()
    {
        PhotonNetwork.LeaveRoom();
        SceneLoader.Instance.LoadSceneAsync("MainMenu");
        Time.timeScale = 1f;
    }
}
