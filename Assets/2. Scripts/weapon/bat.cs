using UnityEngine;

public class bat : MonoBehaviour
{
    public float attackRange = 2f;      // 공격 거리
    public float attackAngle = 90f;     // 부채꼴 각도 (90도)
    public float damage = 30f;
    public float knockbackForce = 10f;
    public float attackCooldown = 1.2f;
    public LayerMask enemyLayer;

    private float cooldownTimer;

    void Update()
    {
        cooldownTimer -= Time.deltaTime;

        // 주변 적 탐색
        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, attackRange, enemyLayer);

        if (enemies.Length > 0 && cooldownTimer <= 0)
        {
            AttackSplash(enemies);
        }
    }

    void AttackSplash(Collider2D[] targets)
    {
        bool hitVisual = false;
        Vector3 forwardDir = transform.right;

        foreach (var col in targets)
        {
            Vector3 targetDir = (col.transform.position - transform.position).normalized;
            float angle = Vector3.Angle(forwardDir, targetDir);

            if (angle <= attackAngle / 2f)
            {
                if (col.TryGetComponent<Enemy>(out var enemy))
                {
                    enemy.TakeDamage(damage);
                    enemy.Knockback(targetDir, knockbackForce);
                    hitVisual = true;
                }
            }
        }

        if (hitVisual)
        {
            Debug.Log("방망이 휘두르기!");
            cooldownTimer = attackCooldown;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Vector3 leftBoundary = Quaternion.AngleAxis(-attackAngle / 2f, transform.forward) * transform.right;
        Vector3 rightBoundary = Quaternion.AngleAxis(attackAngle / 2f, transform.forward) * transform.right;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary * attackRange);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary * attackRange);
    }
}