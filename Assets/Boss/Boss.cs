using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
public class Boss : Actor
{
    public Image CurrentHealth;
    public Image LostHealth;

    public Image CurrentStamina;
    public Image LostStamina;

    public struct StartTransform
    {
        public Transform transform;
        public Vector3 startPosition;

        public StartTransform(Transform trans)
        {
            transform = trans;
            startPosition = transform.position;
        }
    }

    public Transform[] transforms;
    private StartTransform[] WobbleUI;

    private float OldValue = 1f;
    private const float StartSpeed = 0f;
    private const float MaxSpeed = 0.1f;
    private float Speed;
    private float timer;

    private float WobbleTime;
    private float WobbleStart;

    private float WobbleStr;

    public AnimationCurve WobbleCurve;

    private Rigidbody2D rb;

    public int maxStamina = 100;
    private float stamina;
    private float oldStaminaValue;
    public float agressiveness = 5;

    public float attackInterval = .5f;
    private float attackTimer;

    public Animator LeftHand;
    private bool LeftBusy = false;
    public Animator RightHand;
    private bool RightBusy = false;

    public EnemyAttack[] attacks;

    public Transform player;

    public Transform leftConstant;
    public Transform rightConstant;
    public Transform topConstant;

    public Transform leftHandReturn;
    public Transform rightHandReturn;

    public Animator mainAnimator;

    public new void Start()
    {
        base.Start();
        WobbleUI = new StartTransform[transforms.Length];
        for(int i = 0; i < transforms.Length; i++)
        {
            WobbleUI[i] = new StartTransform(transforms[i]);
        }
    }

    bool dead;

    void Update()
    {
        if(Health <= MaxHealth/2f)
        {
            mainAnimator.SetBool("LowHealth", true);
            agressiveness = 35;
            attackInterval = 1f;
        }
        if (Health <= 0 && !dead)
        {
            dead = true;
            LeftHand.enabled = false;
            RightHand.enabled = false;
            mainAnimator.SetBool("Dead", true);
        }

        DrawHealthBar();
        if (!dead)
        {
            DoAttackLogic();
            DoStaminaRecovery();
        }
    }
    public void DrawHealthBar()
    {
        float ratio = (float)Health / (float)MaxHealth;
        CurrentHealth.fillAmount = ratio;

        if(timer == 0)
        {
            Speed = Mathf.MoveTowards(Speed, MaxSpeed, Time.deltaTime/10f);

            OldValue = Mathf.Lerp(OldValue, ratio, Speed);
        }

        LostHealth.fillAmount = (float)OldValue;

        timer = Mathf.Max(0, timer - Time.deltaTime);

        if (WobbleTime != 0)
        {
            foreach (StartTransform st in WobbleUI)
            {
                Wobble(st, WobbleCurve.Evaluate(1f - Mathf.InverseLerp(0, WobbleStart, WobbleTime)));
            }
        }
        else
        {
            foreach (StartTransform st in WobbleUI)
            {
                MoveBack(st, WobbleStr);
            }
        }
        WobbleTime = Mathf.Max(0, WobbleTime - Time.deltaTime);
    }

    private float staminaTimer;
    public void DoStaminaRecovery()
    {
        float ratio = (float)stamina / (float)maxStamina;
        CurrentStamina.fillAmount = ratio;

        if (staminaTimer == 0)
        {
            Speed = Mathf.MoveTowards(Speed, MaxSpeed, Time.deltaTime / 20f);

            oldStaminaValue = Mathf.Lerp(oldStaminaValue, ratio, Speed);
        }

        LostStamina.fillAmount = (float)oldStaminaValue;

        if (staminaTimer <= 0) stamina = Mathf.MoveTowards(stamina, maxStamina, Time.deltaTime * agressiveness);
        else staminaTimer = Mathf.MoveTowards(staminaTimer, 0, Time.deltaTime);
    }


    public void Wobble(StartTransform toWobble, float wobbleBy)
    {
        toWobble.transform.position = toWobble.startPosition + new Vector3(Random.Range(-1f, 1f) * wobbleBy, Random.Range(-1f, 1f) * wobbleBy).normalized;
    }
    public void MoveBack(StartTransform toMove, float moveBy)
    {
        toMove.transform.position = Vector3.MoveTowards(toMove.transform.position, toMove.startPosition, moveBy * Time.deltaTime);
    }

    public override void OnHit(int damage)
    {
        base.OnHit(damage);
        timer = 1f;
        Speed = StartSpeed;

        mainAnimator.SetTrigger("Damage");

        WobbleStart = Mathf.Lerp(0.1f, 1.5f, Mathf.InverseLerp(0, 125f, damage));
        WobbleTime = WobbleStart;
        WobbleStr = (float)damage / 5f;
    }

    [System.Serializable]
    public class AttackEvent : UnityEvent<Animator, bool> { }


    private void DoAttackLogic()
    {
        if (attackTimer <= 0)
        {
            int tryAttack = Random.Range(0, attacks.Length);

            bool handsUsable;
            if (attacks[tryAttack].requireBoth) handsUsable = !LeftBusy && !RightBusy;
            else handsUsable = !LeftBusy || !RightBusy;

            if (attacks[tryAttack].StaminaCost <= stamina && handsUsable)
            {

                if (attacks[tryAttack].requireBoth)
                {
                    attacks[tryAttack].function.Invoke(LeftHand, true);
                    attacks[tryAttack].function.Invoke(RightHand, false);
                }
                else
                {
                    bool handToUseLeft;
                    if (!LeftBusy && !RightBusy) handToUseLeft = Random.Range(0,2) == 0;
                    else handToUseLeft = !LeftBusy;

                    if(handToUseLeft) attacks[tryAttack].function.Invoke(LeftHand, true);
                    else attacks[tryAttack].function.Invoke(RightHand, false);
                }


                stamina -= attacks[tryAttack].StaminaCost;
                staminaTimer = 1f;

            }
            attackTimer = attackInterval;
        }
        else attackTimer -= Time.deltaTime;
    }

   

    private IEnumerator FollowPlayer(Transform hand, float forTime, bool FollowPlayerX, bool FollowPlayerY, bool leftX)
    {
        float t = 0;
        var parent = hand.transform.parent;
        while(t < forTime)
        {
            Vector3 target = new Vector3(
                FollowPlayerX ? player.position.x : leftX ? leftConstant.position.x : rightConstant.position.x,
                FollowPlayerY ? player.position.y : topConstant.position.y);

            parent.position = Vector3.Lerp(parent.position, target, 0.1f);

            t += Time.deltaTime;
            yield return null;
        }
        var backTarget = leftX ? leftHandReturn : rightHandReturn;
        yield return new WaitForSeconds(1f);
        while (Vector3.Distance(parent.position, backTarget.position) > 0.1f)
        {
            parent.position = Vector3.Lerp(parent.position, backTarget.position, 0.1f);
            yield return null;
        }
    }

    public void Attack_Sweep(Animator hand, bool left)
    {
        StartCoroutine(FollowPlayer(hand.transform, 2, false, true, left));
        Invoke($"Use{hand.gameObject.name}", 0);
        hand.SetTrigger("Sweep");
        Invoke($"Free{hand.gameObject.name}", 1f);
    }

    public void Attack_Slam(Animator hand, bool left)
    {
        Invoke($"Use{hand.gameObject.name}", 0);
        StartCoroutine(FollowPlayer(hand.transform, 1f, true, false, left));
        hand.SetTrigger("Slam");
        Invoke($"Free{hand.gameObject.name}", 5f);
    }
    public void Attack_Wave(Animator hand, bool left)
    {
        Invoke($"Use{hand.gameObject.name}",0);
        StartCoroutine(FollowPlayer(hand.transform, 2, false, true, left));

        hand.SetTrigger("Wave");
        Invoke($"Free{hand.gameObject.name}", 3f);
    }

    
    public void UseLeftHand()
    {
        LeftBusy = true;
    }
    public void FreeLeftHand()
    {
        LeftBusy = false;
    }
    public void UseRightHand()
    {
        RightBusy = true;
    }
    public void FreeRightHand()
    {
        RightBusy = false;
    }


    [System.Serializable]
    public class EnemyAttack
    {
        public bool requireBoth;
        public int StaminaCost;
        [SerializeField]
        public AttackEvent function;
    }
}
