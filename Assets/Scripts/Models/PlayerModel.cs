using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

public class PlayerModel
{
    public ReactiveProperty<string> NickName { get; set; }
    public ReactiveProperty<int> TeamID { get; set; }

    public PlayerModel(string nickname, int teamID)
    {
        NickName = new ReactiveProperty<string>(nickname);
        TeamID = new ReactiveProperty<int>(teamID);
    }
}
