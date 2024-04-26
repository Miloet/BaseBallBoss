using System.Collections;
using UnityEngine;

public class Ball : Attack
{

    public CircleCollider2D col;
    public Rigidbody2D rb;

    private float timer = 1f;

    private bool Destroying = false;
    private void Update()
    {
        if (Destroying) return;
        col.radius = Mathf.Sqrt(rb.velocity.magnitude)/3f;



        if (rb.velocity.magnitude < 0.01f) timer -= Time.deltaTime;


        if (timer <= 0) StartCoroutine(DestroySelf());
        
    }

    public IEnumerator DestroySelf()
    {
        Destroying = true;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime;

            transform.parent.localScale = Vector3.one * (1f-t) * 0.3f;
            yield return null;
        }

        Destroy(transform.parent.gameObject);
    }


}
