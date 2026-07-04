using UnityEngine;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    [Header("이동 및 설정")]
    public float moveSpeed = 5f;

    [Header("물자 수집 설정")]
    public float supplyDetectRadius = 3f; // N칸 이내 자동 흡입
    public int currentSupplies = 0;
    public int targetSupplies = 5; // 게임 클리어를 위한 목표 개수

    [Header("상호작용 설정")]
    public float interactRadius = 2f;
    public LayerMask interactableLayer;

    [Header("무기 슬롯")]
    public Transform weaponParent; // 무기가 부착될 플레이어 하위 위치
    private GameObject currentWeapon;

    private Vector2 moveInput;
    private Rigidbody2D rb;
    private PlayerAttack playerAttack;
    private SpriteSheetCharacterAnimator spriteAnimator;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerAttack = GetComponent<PlayerAttack>();
        if (playerAttack == null)
        {
            playerAttack = gameObject.AddComponent<PlayerAttack>();
        }

        spriteAnimator = GetComponent<SpriteSheetCharacterAnimator>();
        if (spriteAnimator == null)
        {
            spriteAnimator = gameObject.AddComponent<SpriteSheetCharacterAnimator>();
        }

        spriteAnimator.ConfigureForPlayer();
    }

    void Update()
    {
        // 1. 이동 입력
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");

        // 2. 상호작용 (F키)
        if (Input.GetKeyDown(KeyCode.F))
        {
            TryInteract();
        }

        // 3. 주변 물자 자동 흡입 탐지
        DetectSupplies();
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + moveInput.normalized * moveSpeed * Time.fixedDeltaTime);
    }

    // F키 상호작용 로직
    private void TryInteract()
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, interactRadius, interactableLayer);
        float closestDistance = Mathf.Infinity;
        IInteractable closestInteractable = null;

        foreach (var collider in hitColliders)
        {
            if (collider.TryGetComponent<IInteractable>(out var interactable))
            {
                float distance = Vector2.Distance(transform.position, collider.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestInteractable = interactable;
                }
            }
        }

        if (closestInteractable != null)
        {
            closestInteractable.Interact(this);
        }
    }

    // 물자 자동 흡입 로직
    private void DetectSupplies()
    {
        // Supply 스크립트가 있는 레이어(예: Item)만 탐색하는 것이 성능상 좋습니다.
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, supplyDetectRadius);
        foreach (var col in colliders)
        {
            if (col.TryGetComponent<SupplyItem>(out var supply))
            {
                supply.StartMagnet(this.transform);
            }
        }
    }

    // 무기 장착 메서드 (기존 무기가 있다면 파괴하거나 바닥에 버림)
    public void EquipWeapon(GameObject weaponPrefab)
    {
        if (currentWeapon != null)
        {
            Destroy(currentWeapon);
        }

        currentWeapon = Instantiate(weaponPrefab, weaponParent.position, weaponParent.rotation, weaponParent);
    }

    public void AddSupply()
    {
        currentSupplies++;
        if (GameSessionManager.Instance != null)
        {
            GameSessionManager.Instance.RegisterSupplyCollected();
            GameSessionManager.Instance.SetRequiredSupplyCount(targetSupplies);
        }

        Debug.Log($"물자 획득! 현재 물자: {currentSupplies} / {targetSupplies}");

        if (currentSupplies >= targetSupplies)
        {
            Debug.Log("★ 게임 클리어 조건 달성! ★");
            // 여기에 승리 팝업이나 씬 전환 로직 추가
        }
    }

    private void OnDrawGizmosSelected()
    {
        // 에디터에서 범위 시각화
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, supplyDetectRadius); // 물자 흡입 범위

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, interactRadius); // F키 상호작용 범위
    }
}
