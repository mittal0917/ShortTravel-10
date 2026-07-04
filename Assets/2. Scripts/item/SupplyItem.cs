using UnityEngine;

public class SupplyItem : MonoBehaviour
{
    public float flySpeed = 8f;
    private Transform targetPlayer;
    private bool isFlying = false;
    private bool isCollected;

    void Awake()
    {
        EnsureVisual();
        EnsureCollider();
    }

    public void StartMagnet(Transform playerTransform)
    {
        if (isFlying) return; // 이미 끌려가는 중이면 무시
        targetPlayer = playerTransform;
        isFlying = true;
    }

    void Update()
    {
        if (isFlying && targetPlayer != null)
        {
            // 플레이어 위치로 이동
            transform.position = Vector3.MoveTowards(transform.position, targetPlayer.position, flySpeed * Time.deltaTime);

            // 완전히 근접하면 획득 처리
            if (Vector2.Distance(transform.position, targetPlayer.position) < 0.2f)
            {
                Collect(targetPlayer);
            }
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        Collect(collision.transform);
    }

    private void Collect(Transform collector)
    {
        if (isCollected || collector == null)
        {
            return;
        }

        PlayerController player = collector.GetComponentInParent<PlayerController>();
        character_move legacyPlayer = collector.GetComponentInParent<character_move>();
        if (player == null && legacyPlayer == null)
        {
            return;
        }

        isCollected = true;

        if (player != null)
        {
            player.AddSupply();
        }
        else if (GameSessionManager.Instance != null)
        {
            GameSessionManager.Instance.RegisterSupplyCollected();
        }

        Destroy(GetDestroyTarget());
    }

    private void EnsureVisual()
    {
        Sprite supplySprite = SupplyVisuals.GetSupplySprite();
        if (supplySprite == null)
        {
            return;
        }

        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        if (renderer == null)
        {
            renderer = gameObject.AddComponent<SpriteRenderer>();
        }

        renderer.sprite = supplySprite;
        renderer.color = Color.white;
        renderer.sortingOrder = 9;
    }

    private void EnsureCollider()
    {
        BoxCollider2D collider = GetComponent<BoxCollider2D>();
        if (collider == null)
        {
            collider = gameObject.AddComponent<BoxCollider2D>();
        }

        collider.isTrigger = true;
        collider.size = Vector2.one;
    }

    private GameObject GetDestroyTarget()
    {
        if (transform.parent != null && transform.parent.name.Contains("Supply"))
        {
            return transform.parent.gameObject;
        }

        return gameObject;
    }
}
