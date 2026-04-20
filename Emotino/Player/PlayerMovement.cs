using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{
    public GameOverManager over;
    public GameObject gameOver;

    public int hp = 5;
    public GameObject[] hpobj;

    public SpriteRenderer sp;
    public static PlayerMovement instance;
    public float statRate;
    public Gun gun;

    public GameObject ParticlePrefab_EnemyDie;
    private CapsuleCollider2D collider;

    public bool isDie;
    public bool isDmg;

    private Rigidbody2D rigid;
    public float moveSpeed;
    [SerializeField] private Vector3 moveDir = new Vector3();

    [Header("Jump")]
    public Transform groundCheckObj;
    public float groundCheckRadius;
    public float jumpForce = 5f;
    public float jumpDuration = 0.5f;
    public float groundCheckDistance;
    public LayerMask groundLayer;
    private bool isJumping = false;
    private float jumpStartTime;

    public bool isMove;

    public bool isCanMove = false;
    public float noise = 0;

    private void Awake()
    {
        sp = GetComponent<SpriteRenderer>();
        if (instance == null) instance = this;
        rigid = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        InputVec();
        OnJump();
    }
    private void FixedUpdate()
    {
        Movement();
    }

    private void InputVec()
    {
        float h = Input.GetAxisRaw("Horizontal");
        if (h == 1)
        {
            isMove = true;
            sp.flipX = false;
        }
        else if (h == -1)
        {
            isMove = true;
            sp.flipX = true;
        }
        else isMove = false;

        moveDir = new Vector3(h * statRate, 0, 0);
    }
    private void Movement()
    {
        if (!isCanMove) return;
        rigid.velocity = new Vector2(moveDir.x * moveSpeed + (noise * moveSpeed), rigid.velocity.y);
    }
    private void OnJump()
    {
        bool isGrounded = Physics2D.OverlapCircle(groundCheckObj.transform.position, groundCheckRadius, groundLayer);
        //Debug.DrawRay(transform.position, Vector2.down, Color.yellow, groundCheckDistance);
        if (isGrounded && Input.GetButtonDown("Jump"))
        {
            Jump();
        }

        if (isJumping && Input.GetButton("Jump") && Time.time - jumpStartTime < jumpDuration)
        {
            JumpHigher();
        }

        if (Input.GetButtonUp("Jump"))
        {
            isJumping = false;
        }
    }

    private void Jump()
    {
        rigid.AddForce(Vector3.up * jumpForce * statRate, ForceMode2D.Impulse);
        isJumping = true;
        jumpStartTime = Time.time;
    }

    private void JumpHigher()
    {
        rigid.AddForce(Vector3.up * jumpForce * statRate * Time.deltaTime, ForceMode2D.Impulse);
    }

    public void OnNoise(float power)
    {
        noise = Random.Range(-power, power) / 1000f;
    }

    public void Hert(int val)
    {
        if (isDie || isDmg) return;
        hp -= val;

        isDmg = true;
        foreach (GameObject obj in hpobj) obj.SetActive(false);

        for(int i = 0; i < hp; i++)
        {
            hpobj[i].SetActive(true);
        }

        if(hp <= 0)
        {
            Die();
        }

        StartCoroutine(Twinkling());
        Invoke("EndGod", 0.8f);
    }
    IEnumerator Twinkling()
    {
        for (int i = 0; i < 4; i++)
        {
            sp.color = new Color(1, 1, 1, 0.4f);
            yield return new WaitForSeconds(0.1f);
            sp.color = new Color(1, 1, 1, 1f);
            yield return new WaitForSeconds(0.1f);
        }
    }

    void EndGod()
    {
        sp.color = new Color(1, 1, 1, 1f);
        isDmg = false;
    }

    void Die()
    {
        isMove = false;
        gameObject.layer = 6;

        GameObject Particle = Instantiate(ParticlePrefab_EnemyDie, transform.position, transform.rotation);

        isDie = true;

        isCanMove = false;
        sp.color = Color.black;
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, 90));
        rigid.velocity = Vector3.zero;
        gameOver.SetActive(true);
        Destroy(gameObject, 5);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(Tags.Enemy)) Hert(1);
        else if (collision.gameObject.CompareTag(Tags.Spike)) Hert(3);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag(Tags.Goal))
        {
            Cam.instance.isCantMove = true;
            isCanMove = false;
            moveDir = new Vector2(1, 0);
            rigid.velocity = new Vector2(moveDir.x * moveSpeed, rigid.velocity.y);
        }

        if (collision.gameObject.CompareTag(Tags.Heart)){
            GetComponent<EmotionSystem>().IsEnableHeart();
        }

        if (collision.gameObject.CompareTag(Tags.End))
        {
            gameOver.SetActive(true);
            over.OnToTitle();
        }
    }
}
