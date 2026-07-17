using UnityEngine;

public class pistol : MonoBehaviour
{
    public float attackRange = 5f;
    public float damage = 120f;
    public int maxAmmo = 33;
    public float reloadTime = 2.0f;
    public float fireRate = 1f;
    public LayerMask enemyLayer;

    private int currentAmmo;
    private float fireTimer;
    private float reloadTimer;
    private bool isReloading = false;

    void Start()
    {
        currentAmmo = maxAmmo;
    }

    void Update()
    {
        if (isReloading)
        {
            reloadTimer -= Time.deltaTime;
            if (reloadTimer <= 0)
            {
                currentAmmo = maxAmmo;
                isReloading = false;
                Debug.Log("권총 재장전 완료!");
            }
            return;
        }

        fireTimer -= Time.deltaTime;

        // 범위 내 적 탐지
        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, attackRange, enemyLayer);

        if (enemies.Length > 0 && fireTimer <= 0)
        {
            // 가장 가까운 적 타겟팅
            Enemy closestEnemy = GetClosestEnemy(enemies);
            if (closestEnemy != null)
            {
                Shoot(closestEnemy);
            }
        }
    }

    Enemy GetClosestEnemy(Collider2D[] enemies)
    {
        Enemy closest = null;
        float minDist = Mathf.Infinity;
        foreach (var col in enemies)
        {
            if (col.TryGetComponent<Enemy>(out var e))
            {
                float dist = Vector2.Distance(transform.position, col.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    closest = e;
                }
            }
        }
        return closest;
    }

    void Shoot(Enemy target)
    {
        currentAmmo--;
        fireTimer = fireRate;

        Debug.Log($"탕! 권총 발사 (남은 총알: {currentAmmo}/{maxAmmo})");
        target.TakeDamage(damage);

        if (currentAmmo <= 0)
            StartReload();
    }

    void StartReload()
    {
        isReloading = true;
        reloadTimer = reloadTime;
        Debug.Log("총알 소진, 재장전 중...");
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
