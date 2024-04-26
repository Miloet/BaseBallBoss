using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class ChargeMeter : MonoBehaviour
{
    public bool isVisible;
    float alpha;
    public float speed = 5f;
    public float graceTime = 1f;
    float charge;

    public Image image;
    public Gradient chargeColor;
    public CanvasGroup mask;


    Rigidbody2D playerRB;
    public static ChargeMeter instance;


    Vector2 target;
    public float moveSpeed;

    Camera main;
    void Start()
    {
        instance = this;
        playerRB = FindObjectOfType<PlayerController>().GetComponent<Rigidbody2D>();
        main = Camera.main;
    }

    private void Update()
    {
        alpha = Mathf.MoveTowards(alpha, isVisible ? 1 + graceTime:0f, Time.deltaTime*speed);
        image.color = chargeColor.Evaluate(charge);
        image.fillAmount = charge;


        mask.alpha = alpha;
        transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, alpha);


        if (playerRB.velocity.magnitude != 0)
        {
            target = new Vector3(-playerRB.velocity.x, playerRB.velocity.y * 2f, 0).normalized * 3f + (Vector3)playerRB.position + Vector3.down;
        }
        transform.position = Vector3.MoveTowards(transform.position, main.WorldToScreenPoint(target), moveSpeed * Time.deltaTime);
    }
    public static void ChangeVisibility(bool vis)
    {
        instance.isVisible = vis;
    }

    public static void SetCharge(float cha)
    {
        instance.charge = cha;
    }
}
