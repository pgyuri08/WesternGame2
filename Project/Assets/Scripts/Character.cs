using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Character : MonoBehaviour {

    public Animator MyAnimator { get; private set; }

    [SerializeField]
    protected float movementSpeed;

    protected bool facingRight;

    [SerializeField]
    protected int health;

    [SerializeField]
    public EdgeCollider2D SlideCollider;

    [SerializeField]
    public EdgeCollider2D knifeCollider;

    public EdgeCollider2D KnifeCollider
    {
        get
        {
            return knifeCollider;
        }
    }

    [SerializeField]
    private List<string> damageSources;

    public abstract bool IsDead { get; }

    public bool Attack { get; set; }

    public bool TakingDamage { get; set; }


    // Use this for initialization
    public virtual void Start ()
    {
        facingRight = true;
        MyAnimator = GetComponent<Animator>();
    }
	
	// Update is called once per frame
	void Update ()
    {
  
        
	}

    public abstract IEnumerator TakeDamage();

    public void SlideAttack()
    {
        SlideCollider.enabled = !SlideCollider.enabled;
    }

    public virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (damageSources.Contains(other.tag))
        {
            StartCoroutine(TakeDamage());
        }
    }

    public void MeleeAttack()
    {
        KnifeCollider.enabled = true;
    }

    public void ChangeDirection()
    {
        facingRight = !facingRight;
        transform.localScale = new Vector3(transform.localScale.x * -1, 1, 1);
    }



}
