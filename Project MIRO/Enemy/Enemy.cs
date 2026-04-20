using UnityEngine;
using UnityEngine.SceneManagement;

public class Enemy : MonoBehaviour
{
    public float moveSpeed = 2f;
    public GameObject bombPrefab;

    private Rigidbody2D rb;
    private Transform player;
    private SpriteRenderer spriteRenderer;
    private string currentScene;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // 현재 씬 이름 가져오기
        currentScene = SceneManager.GetActiveScene().name;

        // 움직여도 되는 씬만 동작
        if (currentScene == Scenes.editor)
        {
            enabled = false; // 특정 씬이면 비활성화
            return;
        }

        // 플레이어 찾기
        var playerObj = FindAnyObjectByType<PlayerMovement>();
        if (playerObj != null)
            player = playerObj.transform;

        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void FixedUpdate()
    {
        if (player == null) return;

        // 플레이어 방향으로 이동
        Vector2 dir = (player.position - transform.position).normalized;
        rb.MovePosition(rb.position + dir * moveSpeed * Time.fixedDeltaTime);

        // 이동 방향에 따라 flipX 조절
        if (dir.x < 0) spriteRenderer.flipX = true;
        else if (dir.x > 0) spriteRenderer.flipX = false;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Bullet"))
        {
            // 폭발 프리팹 생성
            if (bombPrefab != null)
                Instantiate(bombPrefab, transform.position, Quaternion.identity);

            Destroy(gameObject); // 적 제거dks
        }
    }
}
