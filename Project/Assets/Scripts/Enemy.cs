using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : Character {

    private static Enemy instance;

    public static Enemy Instance
    {
        get
        {
            if (instance == null)
            {
                instance = GameObject.FindObjectOfType<Enemy>();
            }
            return instance;
        }
    }

    public GameObject Target { get; set; }

    private IEnemyState currentState;
    public bool attack;

    public bool Melee { get; set; }

    [SerializeField]
    private float meleeRange;

    [SerializeField]
    private Transform leftEdge;

    [SerializeField]
    private Transform rightEdge;

    private Canvas healthCanvas;

    public bool InMeleeRange
    {
        get
        {
            if (Target != null)
            {
                return Vector2.Distance(transform.position, Target.transform.position) <= meleeRange;
            }
           
                return false;
        }
    }

    public override bool IsDead
    {
        get
        {
            return healthStat.CurrentValue <= 0;
        }
    }



    // Use this for initialization
    public override void Start ()
    {

        base.Start();
        Player.Instance.Dead += new DeadEventHandler(RemoveTarget);
        ChangeState(new IdleState());

        healthCanvas = transform.GetComponentInChildren<Canvas>();
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (!IsDead)
        {
            if (!TakingDamage)
            {
                currentState.Execute();
            }

            LookAtTarget();
        }

    }

    void FixedUpdate()
    {
        HandleAttacks();
        ResetValues();
    }

    private void LookAtTarget()
    {
        if(Target != null)
        {
            float xDir = Target.transform.position.x - transform.position.x;

            if (xDir < 0  && facingRight ||xDir > 0 && !facingRight)
            {
                ChangeDirection();
            }
        }
    }

    public void RemoveTarget()
    {
        Target = null;
        ChangeState(new PatrolState());
    }

    public void ChangeState(IEnemyState newState)
    {
        if (currentState != null)
        {
            currentState.Exit();
        }

        currentState = newState;

        currentState.Enter(this);
    }

    public void Move()
    {
        if ((GetDirection().x > 0 && transform.position.x < rightEdge.position.x) || GetDirection().x < 0 && transform.position.x > leftEdge.position.x)
        {
            MyAnimator.SetFloat("speed", 1);
            transform.Translate(GetDirection() * (movementSpeed * Time.deltaTime));
        }
        else if(currentState is PatrolState)
        {
            ChangeDirection();
        }
            
    }

    public override void OnTriggerEnter2D(Collider2D other)
    {
        base.OnTriggerEnter2D(other);
        currentState.OnTriggerEnter(other); 
    }

    public Vector2 GetDirection()
    {
        return facingRight ? Vector2.right : Vector2.left;
    }


    public void HandleAttacks()
    {
        if (attack)
        {
            MyAnimator.SetTrigger("attack");
        }
    }

   private void ResetValues()
    {
        attack = false;
    }

    public override IEnumerator TakeDamage()
    {

        if (!healthCanvas.isActiveAndEnabled)
        {
            healthCanvas.enabled = true;
        }

        healthStat.CurrentValue -= 10;

        if (!IsDead)
        {
            MyAnimator.SetTrigger("damage");
        }
        else
        {
            MyAnimator.SetTrigger("die");
            yield return null;
        }
    }

    public override void Death()
    {
        Instantiate(GameManager.Instance.CoinPrefab, new Vector3(transform.position.x, transform.position.y + 2), Quaternion.identity);
        Destroy(gameObject);
    }

    public override void ChangeDirection()
    {
        Transform tmp = transform.FindChild("EnemyHealthBarCanvas").transform;
        Vector3 pos = tmp.position;
        tmp.SetParent(null);

        base.ChangeDirection();

        tmp.SetParent(transform);
        tmp.position = pos;
    }

}
