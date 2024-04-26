using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    #region Variables
    //-----------------------------------------------------

    public float BaseSpeed;
    public float JumpHeight;
    public float Gravity = 1;
    public float SlidingFallSpeed;

    public float footDistance = 0.1f;

    //Keys
    float Move;
    float MoveRaw;
    bool JumpKey;

    float jumpBuffer;

    int MaxWallJumps = 5;
    int WallJumpTimes;
    bool WallJumping;

    bool EndOfFixedUpdate;

    public LayerMask GroundLayerMask;

    bool g;

    Rigidbody2D RB;
    public SpriteRenderer SR;
    public Animator ANI;

    private bool canAct = true;
    [Space(10)]
    public GameObject ball;

    public float ball_timeToMax;
    public float minForce = 1;
    public float maxForce = 10;

    bool canThrow = true;
    [Space(10)]
    public GameObject bat;
    public Transform HitboxPosition;

    public float bat_timeToMax;
    public float minStrength = 5;
    public float maxStrength = 18;


    //-----------------------------------------------------
    #endregion


    #region Functions
    //-----------------------------------------------------

    // Getting the componants
    void Start()
    {
        SR = GetComponent<SpriteRenderer>();
        RB = GetComponent<Rigidbody2D>();
        //Get componants
        RB.gravityScale = Gravity;

        Attack.prefab = Resources.Load<GameObject>("Attack");
    }

    //Updating the keys
    void Update()
    {
        if (Input.GetButtonDown("Jump")) jumpBuffer = 0.1f;
        Move = Input.GetAxis("Horizontal");
        MoveRaw = Input.GetAxisRaw("Horizontal");
        RB.gravityScale = Gravity;


        if (canAct)
        {
            if (Input.GetButtonDown("Fire1"))
            {
                StartCoroutine(SwingBat());
            }
            if (Input.GetButtonDown("Fire2"))//&& canThrow)
            {
                StartCoroutine(ThrowBall());
            }
        }
    }


    public IEnumerator ThrowBall()
    {
        //Play throw animation
        ChargeMeter.SetCharge(0);
        ChargeMeter.ChangeVisibility(true);

        canAct = false;
        canThrow = false;

        yield return new WaitForSeconds(0.1f);
        
        float t = 0;

        Vector2 noCharge = new Vector2(0f, 1f);

        Vector2 angle;
        while(t < ball_timeToMax && Input.GetButton("Fire2"))
        {
            t += Time.deltaTime;
            ChargeMeter.SetCharge(Mathf.InverseLerp(0, ball_timeToMax, t));
            yield return null;
        }
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = ((Vector2)mousePos - (Vector2)transform.position).normalized;

        float normalizedTime = Mathf.InverseLerp(0, ball_timeToMax, t);
        angle = Vector2.Lerp(noCharge, direction, normalizedTime);

        var b = Instantiate(ball,transform.position,Quaternion.identity,null);
        b.GetComponent<Rigidbody2D>().velocity = angle * Mathf.Lerp(minForce, maxForce,normalizedTime);


        ChargeMeter.ChangeVisibility(false);
        yield return new WaitForSeconds(0.1f);
        //canThrow = true;
        canAct = true;
    }


    public IEnumerator SwingBat()
    {
        //Play throw animation
        ChargeMeter.SetCharge(0);
        ChargeMeter.ChangeVisibility(true);

        canAct = false;

        ANI.SetBool("Ready", true);

        yield return new WaitForSeconds(0.1f);
        float t = 0;

        Vector2 mousePos = Vector3.zero;
        Vector2 direction;

        Camera cam = Camera.main;

        while (Input.GetButton("Fire1"))
        {
            t += Time.deltaTime;
            ChargeMeter.SetCharge(Mathf.InverseLerp(0, bat_timeToMax, t));

            mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
            bat.transform.rotation = Quaternion.LookRotation(Vector3.forward, (Vector2)transform.position - (Vector2)mousePos);

            yield return null;
        }

        ANI.SetBool("Ready", false);

        float normalizedTime = Mathf.InverseLerp(0, ball_timeToMax, t);

        //create attack in direction, can parry with lerp normalized time between min and max str as damage
        Attack.CreateAttack(HitboxPosition.position, (mousePos - (Vector2)transform.position).normalized, 1f, (int)Mathf.Lerp(minStrength, maxStrength, normalizedTime),true, false, false, .1f, "Enemy", "Projectile", "Attack");


        ANI.SetTrigger("Swing");

        ChargeMeter.ChangeVisibility(false);
        yield return new WaitForSeconds(0.1f);
        canAct = true;
    }



    #region Movement

    private void FixedUpdate()
    {
        if (jumpBuffer > 0)
        {
            JumpKey = true;
            jumpBuffer -= Time.deltaTime;
        }
        else JumpKey = false;

        g = Grounded();

        //Horizontal Movement
        if (g && !WallJumping) RB.velocity = new Vector2(Move * BaseSpeed, RB.velocity.y);
        else if (!WallJumping && Move != 0) RB.velocity = new Vector2(Move * BaseSpeed, RB.velocity.y);


        //Wall jump
        if (!g && !WallJumping)
        {
            var RayLeft = WallRaycast(1);
            var RayRight = WallRaycast(-1);

            if (RayLeft) if (MoveRaw == 1 || MoveRaw == 0) CanWallJump(1);
            if (RayRight) if (MoveRaw == -1 || MoveRaw == 0) CanWallJump(-1);
        }

        //Jump
        if (g && JumpKey)
        {
            JumpKey = false;
            RB.velocity = new Vector2(MoveRaw * BaseSpeed, JumpHeight);
        }

       

        //Crouch
        //Yet to be implemented
        //Will most likely be done with sprites and the shrinking of the hitboxes




        EndOfFixedUpdate = true;
    }

    public bool Grounded()
    {
        bool ray = Physics2D.Raycast(transform.position - new Vector3(0, SR.bounds.size.y / 2), Vector2.down, footDistance, GroundLayerMask) ||
            Physics2D.Raycast(transform.position - new Vector3(SR.bounds.size.x / 2, SR.bounds.size.y / 2), Vector2.down, footDistance, GroundLayerMask) ||
            Physics2D.Raycast(transform.position - new Vector3(-SR.bounds.size.x / 2, SR.bounds.size.y / 2), Vector2.down, footDistance, GroundLayerMask);
        if (ray) WallJumpTimes = MaxWallJumps;
        return ray;
    }

    public void OnDrawGizmos()
    {
        DrawRaycast(transform.position - new Vector3(0, SR.bounds.size.y / 2), Vector2.down);
        DrawRaycast(transform.position - new Vector3(SR.bounds.size.x / 2, SR.bounds.size.y / 2), Vector2.down);
        DrawRaycast(transform.position - new Vector3(-SR.bounds.size.x / 2, SR.bounds.size.y / 2), Vector2.down);
    }


    void DrawRaycast(Vector3 origin, Vector2 direction)
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(origin, direction * footDistance);
        RaycastHit2D hit = Physics2D.Raycast(origin, direction, footDistance, GroundLayerMask);
        if (hit.collider != null)
        {
            Gizmos.DrawSphere(hit.point, 0.05f);
        }
    }
    #endregion

    #region Wall jump
    //-----------------------------------------------------

    public bool WallRaycast(int dir)
    {
        bool ray = Physics2D.Raycast(transform.position,
            new Vector2(dir, 0),
            0.3f + SR.bounds.size.x / 2,
            GroundLayerMask);
        return ray;
    }
    public Vector2 WallRayPos(int dir)
    {
        var ray = Physics2D.Raycast(transform.position,
           new Vector2(dir, 0),
           0.3f + SR.bounds.size.x / 2,
           GroundLayerMask);

        return new Vector2(ray.point.x - SR.bounds.size.x / 2 * dir, ray.point.y);
    }
    public void CanWallJump(int dir)
    {
        transform.position = WallRayPos(dir); 
        RB.velocity = new Vector2(RB.velocity.x, Mathf.Max(RB.velocity.y, -SlidingFallSpeed));
        if (JumpKey && WallJumpTimes >= 1)
        {
            WallJumpTimes--;
            RB.velocity = new Vector2(-dir * BaseSpeed, JumpHeight);
            StartCoroutine(Jump(-dir));
        }
    }
    IEnumerator Jump(float Axis)
    {
        WallJumping = true;
        jumpBuffer = 0;
        var time = 0.1f;
        RB.velocity = new Vector2(Axis * BaseSpeed, RB.velocity.y);
        while (true)
        {
            yield return new WaitUntil(() => EndOfFixedUpdate == true);

            WallJumping = true;
            
            if (time < 0)
            {
                WallJumping = false;
                break;
            }
            time -= Time.deltaTime;

        }
        WallJumping = false;
    }

    //-----------------------------------------------------
    #endregion


    

    //-----------------------------------------------------
    #endregion
}
