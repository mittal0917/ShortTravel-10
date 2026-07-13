using UnityEngine;

public class Enemy_NormalZombie : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private float health = 5f;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private int maxHealthSlots = 5;

    private Rigidbody2D rb;
    private CircleCollider2D hitCollider;
    private Transform playerTarget;
    private Transform healthBarRoot;
    private SpriteRenderer[] healthSlots;
    private SpriteSheetCharacterAnimator spriteAnimator;
    private int configuredHealthSlots;
    private bool isDead;

    void Awake()
    {
        maxHealthSlots = Mathf.Max(1, maxHealthSlots);
        configuredHealthSlots = maxHealthSlots;

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

        spriteAnimator = GetComponent<SpriteSheetCharacterAnimator>();
        if (spriteAnimator == null)
        {
            spriteAnimator = gameObject.AddComponent<SpriteSheetCharacterAnimator>();
        }

        spriteAnimator.ConfigureForZombie();
    }

    void Start()
    {
        FindPlayerTarget();
        CreateHealthSlots();
        RefreshHealthSlots();
    }

    void FixedUpdate()
    {
        MoveTowardsPlayer();
    }

    public void TakeDamage(float damage)
    {
        if (isDead)
        {
            return;
        }

        health -= damage;
        RefreshHealthSlots();

        if (health <= 0f)
        {
            Die();
        }
    }

    public void TakeDamage(float damage, Vector2 knockbackDirection, float knockbackTiles)
    {
        if (isDead)
        {
            return;
        }

        health -= damage;
        RefreshHealthSlots();

        if (health <= 0f)
        {
            Die();
            return;
        }

        if (rb == null || knockbackTiles <= 0f)
        {
            return;
        }

        Vector2 direction = knockbackDirection.sqrMagnitude > 0.001f ? knockbackDirection.normalized : Vector2.zero;
        rb.position += direction * knockbackTiles;
    }

    public void ConfigureHealthSlots(int healthSlotsCount, bool refillHealth)
    {
        int newHealthSlots = Mathf.Max(1, healthSlotsCount);
        if (newHealthSlots == maxHealthSlots && healthSlots != null)
        {
            if (refillHealth)
            {
                health = maxHealthSlots;
                RefreshHealthSlots();
            }

            return;
        }

        maxHealthSlots = newHealthSlots;
        configuredHealthSlots = newHealthSlots;

        if (refillHealth || health > maxHealthSlots)
        {
            health = maxHealthSlots;
        }

        RebuildHealthSlots();
        RefreshHealthSlots();
    }

    public void ConfigureMoveSpeed(float newMoveSpeed)
    {
        moveSpeed = Mathf.Max(0.1f, newMoveSpeed);
    }

    private void Die()
    {
        isDead = true;

        if (spriteAnimator != null)
        {
            spriteAnimator.PlayDeath();
        }

        if (hitCollider != null)
        {
            hitCollider.enabled = false;
        }

        Destroy(gameObject, 1.2f);
    }

    private void MoveTowardsPlayer()
    {
        if (isDead)
        {
            return;
        }

        if (playerTarget == null)
        {
            FindPlayerTarget();
            if (playerTarget == null)
            {
                return;
            }
        }

        Vector2 nextPosition = Vector2.MoveTowards(rb.position, playerTarget.position, moveSpeed * Time.fixedDeltaTime);
        if (GameSessionManager.TryBlockEnemyMovementAtEscapeDoor(rb.position, nextPosition, out Vector2 blockedPosition))
        {
            rb.velocity = Vector2.zero;
            rb.MovePosition(blockedPosition);
            return;
        }

        rb.MovePosition(nextPosition);
    }

    private void FindPlayerTarget()
    {
        character_move player = FindObjectOfType<character_move>();
        playerTarget = player != null ? player.transform : null;
    }

    private void CreateHealthSlots()
    {
        if (healthBarRoot != null && healthSlots != null && configuredHealthSlots == maxHealthSlots)
        {
            return;
        }

        configuredHealthSlots = maxHealthSlots;
        GameObject root = new GameObject("HealthSlots");
        root.transform.SetParent(transform, false);
        root.transform.localPosition = new Vector3(0f, 0.72f, 0f);
        healthBarRoot = root.transform;

        healthSlots = new SpriteRenderer[maxHealthSlots];
        Sprite slotSprite = BuildSlotSprite();
        float spacing = 0.16f;
        float startX = -(maxHealthSlots - 1) * spacing * 0.5f;

        for (int i = 0; i < maxHealthSlots; i++)
        {
            GameObject slot = new GameObject("HealthSlot");
            slot.transform.SetParent(healthBarRoot, false);
            slot.transform.localPosition = new Vector3(startX + i * spacing, 0f, 0f);
            slot.transform.localScale = new Vector3(0.12f, 0.12f, 1f);

            SpriteRenderer renderer = slot.AddComponent<SpriteRenderer>();
            renderer.sprite = slotSprite;
            renderer.sortingOrder = 20;
            healthSlots[i] = renderer;
        }
    }

    private void RebuildHealthSlots()
    {
        if (healthBarRoot != null)
        {
            Destroy(healthBarRoot.gameObject);
            healthBarRoot = null;
            healthSlots = null;
        }

        CreateHealthSlots();
    }

    private void RefreshHealthSlots()
    {
        if (healthSlots == null)
        {
            return;
        }

        int activeSlots = Mathf.CeilToInt(Mathf.Clamp(health, 0f, maxHealthSlots));
        for (int i = 0; i < healthSlots.Length; i++)
        {
            healthSlots[i].color = i < activeSlots
                ? new Color(0.9f, 0.05f, 0.05f, 1f)
                : new Color(0.12f, 0.12f, 0.12f, 0.45f);
        }
    }

    private static Sprite BuildSlotSprite()
    {
        const int size = 16;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                texture.SetPixel(x, y, Color.white);
            }
        }

        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        PlayerStatus playerStatus = collision.gameObject.GetComponent<PlayerStatus>();
        if (playerStatus != null)
        {
            // 닫힌 문이 플레이어와 좀비 사이를 막고 있으므로 직접 접촉 피해를 막습니다.
            if (GameSessionManager.IsEscapeDoorBlocking)
            {
                return;
            }

            playerStatus.TakeDamage();
        }
    }
}
