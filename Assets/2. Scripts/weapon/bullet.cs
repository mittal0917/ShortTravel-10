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
}