using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillBase : MonoBehaviour
{
    public string skillName = "new skill";
    public float cooldown = 5f;
    public float damage = 3f;
    public float range = 3f;
    public float manaCost = 10f;
    public float duration = 5f;

    protected Animator _animator;
    protected bool _isReady = true;
    protected bool _inUse = false;

    // Start is called before the first frame update
    protected virtual void Start()
    {
        
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        
    }
    public virtual void UseSkill()
    {
        StartCoroutine(Cooldown());
        StartCoroutine(SetInUse());


    }

    protected IEnumerator Cooldown()
    {
        _isReady = false;
        yield return new WaitForSeconds(cooldown);
        _isReady = true;

    }    
    
    protected IEnumerator SetInUse()
    {
        _inUse = true;
        yield return new WaitForSeconds(duration);
        _inUse = false;

    }

    public virtual bool InUse()
    {
        return _inUse;
    }
    public virtual bool IsReady()
    {
        return _isReady;
    }
}
