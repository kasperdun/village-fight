using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainBuilding : BuildingBase
{
    protected override void SetLayer()
    {
        gameObject.layer = PhotonNetwork.LocalPlayer.CustomProperties["Team"].ToString() == tag ? GameManager.myTeamLayer : GameManager.enemyTeamLayer;
    }
    protected override void Die()
    {
        base.Die();
        GameManager.GameOver();
    }

    public override void Update()
    {
    }
}
