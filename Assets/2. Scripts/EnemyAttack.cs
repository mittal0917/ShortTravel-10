using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    public float detectRange = 4f; // 플레이어 감지 거리 (N값)
    public LayerMask playerLayer;  // 감지할 플레이어 레이어 (인스펙터에서 Player로 설정)

    void Update()
    {
        DetectPlayer();
    }

    void DetectPlayer()
    {
        // 적 주변 범위 내의 Player 레이어를 가진 컬라이더 검색
        Collider[] hitPlayers = Physics.OverlapSphere(transform.position, detectRange, playerLayer);

        if (hitPlayers.Length > 0)
        {
            // 플레이어가 감지됨
            GameObject player = hitPlayers[0].gameObject;
            Debug.Log($"🚨 [적] 플레이어({player.name}) 감지! 쫓아가거나 공격 준비 중...");
        }
    }

    // 감지 사거리를 씬 창에서 확인
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);
    }
}