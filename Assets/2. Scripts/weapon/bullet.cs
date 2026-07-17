using UnityEngine;

public class bullet : MonoBehaviour
{
    public float speed = 15f; // 총알 속도
    public float lifetime = 3f; // 총알 생존 시간 (메모리 관리용)

    void Start()
    {
        // 생성되자마자 lifetime 후에 스스로 파괴됨
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        // 총알의 오른쪽(X축 정면) 방향으로 이동
        transform.Translate(Vector2.right * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        DestroyWhenHitZombie(other.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        DestroyWhenHitZombie(collision.gameObject);
    }

    private void DestroyWhenHitZombie(GameObject target)
    {
        if (target == null)
        {
            return;
        }

        // 좀비에게 맞은 총알은 몸을 뚫고 지나가지 않도록 충돌 즉시 제거합니다.
        if (target.GetComponent<Enemy_NormalZombie>() != null
            || target.GetComponent<Enemy>() != null
            || target.GetComponent<ZombieHealth>() != null)
        {
            Destroy(gameObject);
        }
    }
}
