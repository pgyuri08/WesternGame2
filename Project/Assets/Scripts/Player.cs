using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void DeadEventHandler();

public class Player : Character {

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
            if(healthStat.CurrentValue <= 0)
            {
                OnDead();
            }

            return healthStat.CurrentValue <=0;
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

    private float direction;

    private float btnHorizaontal;

    private bool move;

    [SerializeField]
    private float jumpForce;

    [SerializeField]
    private bool airControl;

    private Vector2 startPos;

    // Use this for initialization
    public override void Start ()
    {
        base.Start();

        startPos = this.transform.position;
        spriteRenderer = GetComponent<SpriteRenderer>();
        myRigidbody = GetComponent<Rigidbody2D>();

	}

    void Update()
    {
        if (!TakingDamage && !IsDead)
        {
            if (transform.position.y <= -14f)
            {

                Death();

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

            if (move)
            {
                this.btnHorizaontal = Mathf.Lerp(btnHorizaontal, direction, Time.deltaTime * 2);
                HandleMovement(btnHorizaontal);
                Flip(direction); 
            }
            else
            {
                HandleMovement(horizontal);

                Flip(horizontal);
            }

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

            FindObjectOfType<AudioManager>().Play("PlayerJump");
        }

        MyAnimator.SetFloat("speed", Mathf.Abs(horizontal));

        if (slide && !this.MyAnimator.GetCurrentAnimatorStateInfo(0).IsName("Slide"))
        {
            MyAnimator.SetBool("slide", true);
            FindObjectOfType<AudioManager>().Play("Slide");
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
            healthStat.CurrentValue -= 10;

            if (!IsDead)
            {
                MyAnimator.SetTrigger("damage");
                FindObjectOfType<AudioManager>().Play("Die");
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

    public override void Death()
    {
        myRigidbody.velocity = Vector2.zero;
        MyAnimator.SetTrigger("idle");
        healthStat.CurrentValue = healthStat.MaxVal;
        transform.position = startPos;
    }

    public void BtnJmp()
    {
        MyAnimator.SetTrigger("jump");
        jump = true;
    }

    public void BtnSlide()
    {
        MyAnimator.SetTrigger("slide");
    }

    public void BtnMove(float direction)
    {
        this.direction = direction;
        this.move = true;
    }

    public void BtnStopMove()
    {
        this.direction = 0;
        this.btnHorizaontal = 0;
        this.move = false;
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.tag == "Coin")
        {
            GameManager.Instance.CollectedCoins++;
            StartCoroutine("ChangeLevel");
            Destroy(other.gameObject);
            FindObjectOfType<AudioManager>().Play("Coin");
        }

        if (other.gameObject.tag == "Sign")
        {
            StartCoroutine("ChangeLevel");
        }
    }
    IEnumerator ChangeLevel()
    {
        float fadeTime = GameObject.Find("World").GetComponent<Fading>().Beginfade(1);
        yield return new WaitForSeconds(fadeTime);
        Application.LoadLevel(Application.loadedLevel + 1);
    }
}

