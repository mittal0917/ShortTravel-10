using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float hp = 100f;
    private Rigidbody2D rb;

    void Start() => rb = GetComponent<Rigidbody2D>();

    public void TakeDamage(float damage)
    {
        hp -= damage;
        Debug.Log($"{gameObject.name}가 {damage}의 피해를 입음. 남은 HP: {hp}");
        if (hp <= 0) Destroy(gameObject);
    }

    public void Knockback(Vector2 direction, float force)
    {
        if (rb != null)
        {
            rb.AddForce(direction * force, ForceMode2D.Impulse);
        }
    }
}