using UnityEngine;

public class SupplyItem : MonoBehaviour
{
    public float flySpeed = 8f;
    private Transform targetPlayer;
    private bool isFlying = false;

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
                if (targetPlayer.TryGetComponent<PlayerController>(out var player))
                {
                    player.AddSupply();
                }
                Destroy(gameObject);
            }
        }
    }
}