using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using DG.Tweening;
using System.Linq;
using Photon.Pun;
using System.IO;

[RequireComponent(typeof(Health))]
public class MobBase : MonoBehaviourPun, IPunObservable, IDamagaeble
{
    public float speed = 0.5f;
    [Min(2.4f)]
    public float attackDistance = 2.5f;
    public float attackDelay = 2f;
    public float attackDamage = 1f;
    [Range(0, 1)]
    public float critChance = 0.1f;
    public int critMultiplier = 3;
    public float viewRange = 5f;

    protected NavMeshAgent _navMeshAgent;
    protected bool isAttacking = false;
    protected Animator _animator;
    protected MobBase _enemyMob;
    protected Health _health;
    protected SkillBase _skill;
    protected MainBuilding _enemyMainBuilding;
    protected Transform _currentTarget;

    private void Awake()
    {
        gameObject.layer = photonView.IsMine ? GameManager.myTeamLayer : GameManager.enemyTeamLayer;

        _health = gameObject.GetComponent<Health>();
        _navMeshAgent = gameObject.GetComponent<NavMeshAgent>();
        _animator = gameObject.GetComponent<Animator>();
        _skill = gameObject.GetComponent<SkillBase>();
    }

    public virtual void Start()
    {
        _health.enabled = true;

        if (!photonView.IsMine) return;

        _enemyMainBuilding = FindEnemyMainBuilding();
        

        if (_navMeshAgent != null)
        {
            _navMeshAgent.enabled = true;
            _navMeshAgent.isStopped = true;
        }
    }

    public virtual void Update()
    {
        if (!IsAlive() || (_skill != null && _skill.InUse()) || isAttacking)
            return;

        if (photonView.IsMine)
        {
            LocalUpdate();
        }
        else
        {
            AnotherUpdate();
        }
        
    }

    protected virtual void LocalUpdate()
    {
        if (_enemyMob != null && !_enemyMob.IsAlive())
        {
            _enemyMob = null;
            _currentTarget = null;
        }

        if (_enemyMob == null)
        {
            _enemyMob = FindEnemyInRadius(viewRange);

            _currentTarget = _enemyMob != null ? _enemyMob.transform : _enemyMainBuilding.transform;

            _navMeshAgent.SetDestination(_currentTarget.position);
            _navMeshAgent.isStopped = false;
        }

        if (IsTargetOnAttackDistance())
        {

            transform.DOLookAt(_currentTarget.transform.position, 0.5f);
            _navMeshAgent.isStopped = true;


            if (_enemyMob != null && _skill != null && _skill.IsReady() && _health.currentMana >= _skill.manaCost)
            {
                _skill.UseSkill();
                return;
            }

            StartCoroutine(Attacking());
        }
    }

    protected virtual void AnotherUpdate()
    {

    }

    public virtual Animator GetAnimator()
    {
        return _animator;
    }
    
    protected virtual MainBuilding FindEnemyMainBuilding()
    {
        return FindObjectsOfType<MainBuilding>()
            .Where(building => building.gameObject.layer != gameObject.layer)
            .FirstOrDefault();
    }

    public virtual bool IsAlive()
    {
        return _health.currentHealth > 0;
    }

    [PunRPC]
    public virtual void TakeDamage(float damage)
    {
        if (!IsAlive()) return;

        ChangeHP(-damage);

        if (_health.currentHealth <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        if(_animator != null)
            _animator.SetTrigger("Die");

        _health.enabled = false;
        GameManager.AddScore(photonView.IsMine);

        transform.DOLocalMoveY(transform.position.y - 1, 2)
            .OnComplete(() => {
                transform.DOKill();
                if (photonView.IsMine)
                {
                    PhotonNetwork.Destroy(photonView);
                }
            });
    }

    public virtual void ChangeHP(float addHP)
    {
        _health.ChangeHP(addHP);
    }


    public virtual void ChangeMP(float addMP)
    {
        _health.ChangeMP(addMP);

    }

    private bool IsTargetOnAttackDistance()
    {
        return Vector3.Distance(transform.position, _currentTarget.position) < attackDistance;
    }

    public virtual void Attack(AnimationEvent animationEvent)
    {
        if (_currentTarget == null) return;

        float power = attackDamage;

        if (animationEvent != null && animationEvent.stringParameter == "crit")
        {
            power = (attackDamage * critMultiplier);
        }

        _currentTarget.gameObject.GetComponent<PhotonView>().RPC(nameof(TakeDamage), RpcTarget.All, power);

        isAttacking = false;
    }

    protected IEnumerator Attacking() {
        isAttacking = true;
        if (_animator != null) {
            if (IsChanceAppear(critChance))
            {
                _animator.SetTrigger("CritAttack");
            }
            else
            {
                _animator.SetTrigger("Attack");
            }
            yield return new WaitForSeconds(attackDelay);

        }
        else
        {
            Attack(null);
            yield return new WaitForSeconds(attackDelay);
        }
    }

    protected bool IsChanceAppear(float chance)
    {
        return Random.value <= chance;
    }

    
    protected virtual void CreateMob(MobBase mob, Vector3 position, Quaternion rotation)
    {
        if (photonView.IsMine)
        {
            PhotonNetwork.Instantiate(Path.Combine("Prefabs", mob.name), position, rotation);
        }
    }

    public virtual MobBase FindEnemyInRadius(float radius)
    {
        MobBase nearestEnemy = null;

        MobBase[] mobs = FindEnemiesInRadius(radius);

        float minimumDistance = Mathf.Infinity;

        foreach (MobBase mob in mobs)
        {
            // TODO: order mobs by health and select first

            float distance = Vector3.Distance(transform.position, mob.transform.position);
            if (distance < minimumDistance)
            {
                nearestEnemy = mob;
                minimumDistance = distance;
            }
        }

        return nearestEnemy;
    }

    public virtual MobBase[] FindEnemiesInRadius(float radius)
    {
        MobBase[] mobs = Physics.OverlapSphere(transform.position, radius, 1 << GameManager.enemyTeamLayer)
            .Where(col => {

                // TODO: find IDamageable instead MobBase
                // if enemy is building check !mob.IsGhost

                var mob = col.gameObject.GetComponent<MobBase>();
                return mob != null && mob.IsAlive();
            })
            .Select(col => col.gameObject.GetComponent<MobBase>())
            .ToArray();

        return mobs;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(_health.currentHealth);
        }
        else
        {
            _health.ChangeHP((float)stream.ReceiveNext() - _health.currentHealth);
        }
    }
}
