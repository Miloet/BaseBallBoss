using System;
using UnityEngine;

public class Actor : MonoBehaviour
{
    public int MaxHealth;
    [NonSerialized]
    public int Health;
    public int Defence;

    public void Start()
    {
        Health = MaxHealth;
    }
    public void TakeDamage(int damage)
    {
        Health -= Mathf.Clamp(damage - Defence, 0, Health);
        OnHit(damage);
        if (Health <= 0) OnDeath();
    }
    public virtual void OnHit(int damage)
    {

    }
    public virtual void OnDeath()
    {

    }
}
