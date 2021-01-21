using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public Text nickNameText;
    public GameObject changeNickPanel;

    private void Start()
    {
        Debug.Log("Menu nickname: " + GameManager.nickName);
        nickNameText.text = GameManager.nickName;
    }

    public void StartGame()
    {
        SceneLoader.Instance.LoadSceneAsync("Game");
    }

    public void OnNickClicked()
    {
        changeNickPanel.SetActive(true);
        var input = changeNickPanel.GetComponentInChildren<InputField>();
        input.text = GameManager.nickName;
    }

    public void OnNickChanged()
    {
        GameManager.UpdateNickName(changeNickPanel.GetComponentInChildren<InputField>().text);
        nickNameText.text = GameManager.nickName;
        changeNickPanel.SetActive(false);
    }
}
