using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void DeadEventHandler();

public class Player : Character {

    //extra line
    public static Player instance;

    public event DeadEventHandler Dead;

    public static Player Instance
    {
        get
        {
            if (instance == null)
            {
                instance = GameObject.FindObjectOfType<Player>();
            }
            return instance;
        }
    }

    public override bool IsDead
    {
        get
        {
            if(health <= 0)
            {
                OnDead();
            }

            return health <=0;
        }
    }

    private Rigidbody2D myRigidbody;

    private bool immortal = false;

    [SerializeField]
    private float immortalTime;

    private SpriteRenderer spriteRenderer;

    private bool slide;

    [SerializeField]
    private Transform[] groundPoints;

    [SerializeField]
    private float groundRadius;

    [SerializeField]
    private LayerMask whatIsGround;

    private bool isGrounded;

    private bool jump;

    [SerializeField]
    private float jumpForce;

    [SerializeField]
    private bool airControl;

    // Use this for initialization
    public override void Start ()
    {
        base.Start();

        spriteRenderer = GetComponent<SpriteRenderer>();
        myRigidbody = GetComponent<Rigidbody2D>();
	}

    void Update()
    {
        if (!TakingDamage && !IsDead)
        {
            if (transform.position.y <= -14f)
            {
                myRigidbody.velocity = Vector2.zero;
               // transform.position - startPos;
            }
            HandleInput();

        }
    }
    // Update is called once per frame
    void FixedUpdate ()
    {
        if (!TakingDamage && !IsDead)
        {
            isGrounded = IsGrounded();

            float horizontal = Input.GetAxis("Horizontal");

            HandleMovement(horizontal);

            Flip(horizontal);

            HandleLayers();

            ResetValues();
        }

	}

    public void OnDead()
    {
        if (Dead != null)
        {
            Dead();
        }
    }

    private void HandleMovement(float horizontal)
    {
        if(myRigidbody.velocity.y < 0)
        {
            MyAnimator.SetBool("land", true);
        }

        if (!MyAnimator.GetBool("slide") && (isGrounded || airControl))
        {
            myRigidbody.velocity = new Vector2(horizontal * movementSpeed, myRigidbody.velocity.y);
        }

        if(isGrounded && jump)
        {
            isGrounded = false;
            myRigidbody.AddForce(new Vector2(0, jumpForce));
            MyAnimator.SetTrigger("jump");
        }

        MyAnimator.SetFloat("speed", Mathf.Abs(horizontal));

        if (slide && !this.MyAnimator.GetCurrentAnimatorStateInfo(0).IsName("Slide"))
        {
            MyAnimator.SetBool("slide", true);
        }
        else if (!this.MyAnimator.GetCurrentAnimatorStateInfo(0).IsName("Slide"))
        {
            MyAnimator.SetBool("slide", false);
        }
    }

    private void HandleInput()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            jump = true;
        }

        if(Input.GetKeyDown(KeyCode.LeftControl))
        {
            slide = true;
        }
    }



    private void Flip(float horizontal)
    {
        if (horizontal > 0 && !facingRight || horizontal < 0 && facingRight)
        {
            ChangeDirection();
        }
    }

    private void ResetValues()
    {
        slide = false;
        jump = false;
    }

    private bool IsGrounded()
    {
        if(myRigidbody.velocity.y <= 0)
        {
            foreach (Transform point in groundPoints)
            {
                Collider2D[] colliders = Physics2D.OverlapCircleAll(point.position, groundRadius, whatIsGround);

                for (int i = 0; i < colliders.Length; i++)
                {
                    if(colliders[i].gameObject != gameObject)
                    {
                        MyAnimator.ResetTrigger("jump");
                        MyAnimator.SetBool("land", false);
                        return true;
                    }
                }
            }
        }
        return false;
    }

    private void HandleLayers()
    {
        if(!isGrounded)
        {
            MyAnimator.SetLayerWeight(1, 1);
        }
        else
        {
            MyAnimator.SetLayerWeight(1, 0);
        }
    }

    private IEnumerator IndicateImmortal()
    {
        while (immortal)
        {
            spriteRenderer.enabled = false;
            yield return new WaitForSeconds(.1f);
            spriteRenderer.enabled = true;
            yield return new WaitForSeconds(.1f);
        }
    }
       
    public override IEnumerator TakeDamage()
    {
        if (!immortal)
        {
            health -= 10;

            if (!IsDead)
            {
                MyAnimator.SetTrigger("damage");
                immortal = true;
                StartCoroutine(IndicateImmortal());
                yield return new WaitForSeconds(immortalTime);

                immortal = false;
            }
            else
            {
                MyAnimator.SetLayerWeight(1, 0);
                MyAnimator.SetTrigger("die");
            }
        }
       
    }
}

