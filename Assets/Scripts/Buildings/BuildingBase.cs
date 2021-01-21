using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class BuildingBase : MonoBehaviourPun
{
    public MobBase mobPrefab;
    public bool IsGhost = false;

    protected Health _health;

    private void Start()
    {
        if (IsGhost) return;

        SetLayer();
        _health = gameObject.GetComponent<Health>();
        _health.enabled = true;
        var collider = gameObject.GetComponent<BoxCollider>();
        if (collider != null) {
            collider.enabled = true;
        }
    }

    protected virtual void SetLayer()
    {
        gameObject.layer = photonView.IsMine ? GameManager.myTeamLayer : GameManager.enemyTeamLayer;
    }

    // Update is called once per frame
    public virtual void Update()
    {
        if (IsGhost || !IsAlive() || !photonView.IsMine) return;

        if (_health.currentMana >= _health.maxMana)
        {
            CreateMob(mobPrefab, new Vector3(transform.position.x + 2, transform.position.y, transform.position.z), transform.rotation);
            _health.ChangeMP(-_health.maxMana);
        }
    }

    [PunRPC]
    public virtual void Damage(float damage)
    {
        if (!IsAlive()) return;

        _health.ChangeHP(-damage);

        if (_health.currentHealth <= 0)
        {
            Die();
        }
    }

    protected virtual void CreateMob(MobBase mob, Vector3 position, Quaternion rotation)
    {
        if (photonView.IsMine)
        {
            PhotonNetwork.Instantiate(Path.Combine("Prefabs", mob.name), position, rotation);
        }
    }

    public virtual bool IsAlive()
    {
        return _health.currentHealth > 0;
    }

    protected virtual void Die()
    {
        if (photonView.IsMine)
        {
            PhotonNetwork.Destroy(photonView);
        }
    }
}
