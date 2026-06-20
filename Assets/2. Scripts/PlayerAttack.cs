using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public float attackRange = 5f; // 감지 및 공격 거리 (N값)
    public LayerMask enemyLayer;   // 감지할 적 레이어 (인스펙터에서 Enemy로 설정)

    void Update()
    {
        DetectAndAttack();
    }

    void DetectAndAttack()
    {
        // 플레이어 주변 범위 내의 Enemy 레이어를 가진 컬라이더들을 모두 검색
        Collider[] hitEnemies = Physics.OverlapSphere(transform.position, attackRange, enemyLayer);

        if (hitEnemies.Length > 0)
        {
            // 가장 가까운 적을 공격하거나 전체 공격하는 로직 (현재는 디버그 로그만)
            foreach (Collider enemy in hitEnemies)
            {
                Debug.Log($"⚔️ [플레이어] {enemy.name}을(를) 자동으로 공격하고 있습니다!");
            }
        }
    }

    // 공격 사거리를 씬 창에서 확인
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}