using UnityEngine;

public class Enemy_NormalZombie : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private float health = 100f;
    [SerializeField] private float moveSpeed = 3f;

    private Rigidbody2D rb;
    private CircleCollider2D hitCollider;
    private Transform playerTarget;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }

        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        hitCollider = GetComponent<CircleCollider2D>();
        if (hitCollider == null)
        {
            hitCollider = gameObject.AddComponent<CircleCollider2D>();
        }

        hitCollider.radius = 0.45f;
    }

    void Start()
    {
        FindPlayerTarget();
    }

    void FixedUpdate()
    {
        MoveTowardsPlayer();
    }

    public void TakeDamage(float damage)
    {
        health -= damage;

        if (health <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        Destroy(gameObject);
    }

    private void MoveTowardsPlayer()
    {
        if (playerTarget == null)
        {
            FindPlayerTarget();
            if (playerTarget == null)
            {
                return;
            }
        }

        Vector2 nextPosition = Vector2.MoveTowards(rb.position, playerTarget.position, moveSpeed * Time.fixedDeltaTime);
        rb.MovePosition(nextPosition);
    }

    private void FindPlayerTarget()
    {
        character_move player = FindObjectOfType<character_move>();
        playerTarget = player != null ? player.transform : null;
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        PlayerStatus playerStatus = collision.gameObject.GetComponent<PlayerStatus>();
        if (playerStatus != null)
        {
            playerStatus.TakeDamage();
        }
    }
}
