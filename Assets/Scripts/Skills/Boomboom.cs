using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Boomboom : SkillBase
{
    public GameObject particles;

    private Slime _slime;
    // Start is called before the first frame update
    protected override void Start()
    {
        _slime = GetComponent<Slime>();
        _animator = _slime.GetAnimator();
    }

    // Update is called once per frame
    protected override void Update()
    {
        
    }

    public override void UseSkill()
    {
        base.UseSkill();
        
        _animator.SetTrigger("Skill");
        _slime.ChangeMP(-manaCost);
    }

    public void DamageEnemies(AnimationEvent e)
    {
        if (particles)
        {
            GameObject explosion = Instantiate(particles, transform.position, particles.transform.rotation);
            Destroy(explosion, explosion.GetComponent<ParticleSystem>().main.startLifetimeMultiplier);
        }

        MobBase[] enemies = _slime.FindEnemiesInRadius(range);

        foreach (MobBase enemy in enemies)
        {
                enemy.TakeDamage(damage);
        }
    }

    protected AnimationEvent AddAnimationEvent(string animationName, int frame, string functionName, string parameter = null)
    {
        AnimationClip clip = _animator.runtimeAnimatorController.animationClips.ToList()
            .Where(anim => anim.name == animationName)
            .First();

        AnimationEvent animationEvent = new AnimationEvent();
        animationEvent.functionName = functionName;
        animationEvent.stringParameter = parameter;
        animationEvent.time = frame / clip.frameRate;

        clip.AddEvent(animationEvent);
        return animationEvent;
    }
}
