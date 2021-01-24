using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerConfig 
{
    public string NickName
    {
        get => PlayerPrefs.GetString("Player", "Player");
        set => PlayerPrefs.SetString("Player", value);
    }
}
