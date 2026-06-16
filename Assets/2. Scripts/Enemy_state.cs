using UnityEngine;

public class Enemy_NormalZombie : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private float health = 100f;       // 체력
    [SerializeField] private float moveSpeed = 3.0f;   // 이동 속도

    private Rigidbody2D rb; // 2D 프로젝트 기준 (3D라면 Rigidbody)

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        Move();
    }

    private void Move()
    {
        // 간단한 왼쪽 이동 예시 (방향은 로직에 따라 수정 가능)
        transform.Translate(Vector2.left * moveSpeed * Time.deltaTime);

        // 애니메이션 작동 확인용 로그
        Debug.Log("Zombie is Moving: Playing Walk Animation...");
    }

    // 외부에서 데미지를 입힐 때 사용할 함수 예시
    public void TakeDamage(float damage)
    {
        health -= damage;
        Debug.Log($"Zombie Hit! Remaining Health: {health}");

        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Zombie Died: Playing Death Animation...");
        Destroy(gameObject);
    }
}