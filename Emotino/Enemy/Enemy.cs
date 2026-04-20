using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;


public class Enemy : MonoBehaviour
{
    private Animator anim;
    private Rigidbody2D rb;
    private SpriteRenderer spr;
    private PlayerMovement player;
    private CapsuleCollider2D collider;

    private bool isNotTrigger;
    private bool isMove = true;

    public float checkDistance;
    public float maxEnemyHp = 5;
    public float enemyHP;

    public GameObject ParticlePrefab_EnemyDie;
    

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.7f;
    public LayerMask groundLayer;

    [Header("Move")]
    public float moveSpeed = 5f;
    public float maxMoveSpeed = 5;
    public float jumpForce = 5f;
    private bool isGrounded = false;
    public float jumpTimer = 3;


    public bool isDie = false;

    [Header("knockback")]
    public float knockbackForce = 5f; // �˹� ��
    public float knockbackDuration = 0.5f; // �˹� ���� �ð�
    private bool isKnockedBack = false; // �˹� ���� ���θ� ��Ÿ���� ����
    private float knockbackTimer = 0f; // �˹� ���� �ð��� ����ϴ� Ÿ�̸�


    void Start()
    {
        player = PlayerMovement.instance;
        anim = GetComponent<Animator>();
        collider = GetComponent<CapsuleCollider2D>();
        rb = GetComponent<Rigidbody2D>();
        spr = GetComponent<SpriteRenderer>();
        enemyHP = maxEnemyHp;

        gameObject.layer = 0;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, checkDistance);
    }

    // Update is called once per frame
    void Update()
    {
        if (isDie==false)
        {
            Move();
            Knockback();
            
        }
        
    }
    #region �̵�, �̵�����
    void Move()
    {
         MoveAndJump();
    }
    void MoveAndJump()
    {
        if(Vector2.Distance(transform.position, player.transform.position) > checkDistance)
        {
            anim.SetBool("isMove", false);
            rb.velocity = Vector2.zero;
            return;
        }
        if (player.transform.position.x <= transform.position.x) spr.flipX = false;
        else if (player.transform.position.x > transform.position.x) spr.flipX = true;
        if (player == null) return;
        
        Vector2 direction = (player.transform.position - transform.position).normalized;
        if (rb.velocity == Vector2.zero) anim.SetBool("isMove", false);
        else anim.SetBool("isMove", true);
        rb.AddForce(direction * moveSpeed, ForceMode2D.Force);
        
        if (Mathf.Abs(rb.velocity.x) > maxMoveSpeed) rb.velocity = new Vector2(Mathf.Sign(rb.velocity.x) * maxMoveSpeed, rb.velocity.y); //���ӵ� ����

        jumpTimer -= Time.deltaTime;
        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, groundCheckRadius, groundLayer);
        bool isJump = Physics2D.Raycast(transform.position, Vector2.left, groundCheckRadius, groundLayer) != null || Physics2D.Raycast(transform.position, Vector2.right, groundCheckRadius, groundLayer) != null;

        // ���� ����
        if (isJump && isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            jumpTimer = Random.Range(2f, 4f);
        }
    }
    #endregion

    #region ������ �Ծ�����
    public void TakeDamage()
    {
        enemyHP--;
    
        if (!isKnockedBack)
        {
            isMove = false;
            isKnockedBack = true;
            rb.velocity = Vector2.zero;

            Vector2 knockbackDir = (transform.position - player.transform.position).normalized;
            rb.AddForce(knockbackDir * knockbackForce, ForceMode2D.Impulse);

            knockbackTimer = knockbackDuration;
        }
        if (enemyHP <= 0) Die();
    }
    void Knockback()
    {
        if (isKnockedBack)
        {
            spr.color = Color.red;
            Invoke("ColorWhite", 0.1f);
            knockbackTimer -= Time.deltaTime;

            if (knockbackTimer <= 0)
            {
                isMove = true;
                isKnockedBack = false;
            }
        }
        else
        {
            spr.color = Color.white;

        }
    }
    void Die()
    {
        anim.SetBool("isMove", false);
        gameObject.layer = 6;

        GameObject Particle = Instantiate(ParticlePrefab_EnemyDie, transform.position, transform.rotation);
        collider.size = collider.size / 2;
        

        isDie = true;

        spr.color = Color.gray;
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, 90));
        rb.velocity = Vector3.zero;

        Vector2 knockbackDir = (transform.position - player.transform.position).normalized;
        rb.AddForce(knockbackDir * knockbackForce / 20, ForceMode2D.Impulse);

        Destroy(gameObject, 3);
    }
    #endregion



    private void OnTriggerEnter2D(Collider2D collision)
    {
        isNotTrigger = true;
        if (collision.gameObject.CompareTag("Bullet") && isDie == false)
        {
            TakeDamage();
        }
    }
}
