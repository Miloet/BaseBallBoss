using System.Linq;
using UnityEngine;


public class Attack : MonoBehaviour
{
    public static GameObject prefab;

    public int Damage;
    public string[] ToHit;
    public bool Parry;
    public bool CanBeParried;
    public bool Pierce;
    public Vector2 Direction;


    private void OnTriggerEnter2D(Collider2D collision)
    {
        
        if(ToHit.Contains(collision.tag))
        {
            print("Enter trigger for " + name + " tried to collide with " + collision.tag);
            if (!Pierce) Destroy(gameObject, 0.1f);

            Actor hit;
            if ((hit = collision.GetComponent<Actor>()) != null)
            {   
                hit.TakeDamage(Damage);
                return;
            }

            if(!Parry) return;

            Attack attack;
            if((attack = collision.GetComponent<Attack>()) != null)
            {
                attack.GetParried(this);
                return;
            }
        }
    }

    public void GetParried(Attack parriedBy)
    {
        if (!CanBeParried) return;
        ToHit = parriedBy.ToHit;
        Damage += parriedBy.Damage;

        parriedBy.Parry = false;
        //CanBeParried;

        if (this is Ball) 
        {
            Rigidbody2D rb;
            rb = transform.parent.GetComponent<Rigidbody2D>();
            rb.velocity = parriedBy.Direction * (rb.velocity.magnitude + parriedBy.Damage/2f);
        }
    }


    public void SetVaribles(int d, string[] tag, bool p, bool c_p, bool pi, Vector2 dir)
    {
        Damage = d;
        ToHit = tag;
        Parry = p;
        CanBeParried = c_p;
        Pierce = pi;
        Direction = dir;
    }

    public static GameObject CreateAttack(Vector2 position, Vector2 direction, float radius, int damage, bool parry, bool canBeParried, bool piercing, float timeToLast, params string[] tagsToHit)
    {
        var g = Instantiate(prefab, position, Quaternion.FromToRotation(position, direction), null);
        var col = g.AddComponent<CircleCollider2D>();
        col.radius = radius;
        col.isTrigger = true;

        g.GetComponent<Attack>().SetVaribles(damage, tagsToHit, parry, canBeParried, piercing, direction);
        Destroy(g, timeToLast);

        return g;
    }
    public static GameObject CreateAttack(Vector2 position, Vector2 direction, Vector2 size, int damage, bool parry, bool canBeParried, bool piercing, float timeToLast, params string[] tagsToHit)
    {
        var g = Instantiate(prefab, position, Quaternion.FromToRotation(position, direction), null);
        g.AddComponent<BoxCollider2D>().size = size;

        g.GetComponent<Attack>().SetVaribles(damage, tagsToHit, parry, canBeParried, piercing, direction);
        Destroy(g, timeToLast);

        return g;
    }
}
