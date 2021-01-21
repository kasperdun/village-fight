using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Team
{
    Top,
    Bottom
}

public interface IDamagaeble
{
    public void TakeDamage(float damage);
}