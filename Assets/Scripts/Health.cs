using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    public static event Action<Health> OnHealthAdded = delegate { };
    public static event Action<Health> OnHealthRemoved = delegate { };

    public event Action<float> OnHPChanged = delegate { };
    public event Action<float> OnMPChanged = delegate { };


    public float maxHealth = 10f;
    public float maxMana = 10f;
    public float currentHealth { get; private set; }
    public float currentMana { get; private set; }

    public bool enableMPRegen = false;
    public float regenMPPerSecond = 0f;

    private void OnEnable()
    {
        currentHealth = maxHealth;
        currentMana = 0f;

    }

    private void Start()
    {
        OnHealthAdded(this);

        if (enableMPRegen)
        {
            InvokeRepeating(nameof(RegenerateMP), 0, 1f);
        }
    }

    // Update is called once per frame
    private void OnDisable()
    {
        OnHealthRemoved(this);
    }

    public void ChangeHP(float amount)
    {
        currentHealth += amount;

        if(currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }

        OnHPChanged(currentHealth / maxHealth);
    }
    public void ChangeMP(float amount)
    {
        currentMana += amount;

        if(currentMana > maxMana)
        {
            currentMana = maxMana;
        }

        OnMPChanged(currentMana / maxMana);
    }

    public void RegenerateMP()
    {
        ChangeMP(regenMPPerSecond);
    }
}
