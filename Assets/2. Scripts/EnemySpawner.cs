using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab; // 생성할 적 프리팹
    public float spawnRadius = 5f; // 생성 범위 반경
    public float spawnInterval = 3f; // 생성 주기 (초)

    private float timer;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            SpawnEnemy();
            timer = 0f;
        }
    }

    void SpawnEnemy()
    {
        if (enemyPrefab == null) return;

        // 현재 스포너 위치 기준으로 반경 spawnRadius 내의 임의의 2D/3D 위치 계산
        // (여기서는 3D 평면 Grid/지면 기준 X, Z 축 랜덤 이동으로 가정)
        Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
        Vector3 spawnPosition = new Vector3(
            transform.position.x + randomCircle.x,
            transform.position.y, // 지면 높이에 맞게 조절
            transform.position.z + randomCircle.y
        );

        // 적 생성
        GameObject spawnedEnemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);

        Debug.Log($"[스포너] 적 생성 완료! 위치: {spawnPosition}");
    }

    // 에디터 씬 창에서 생성 범위를 시각적으로 확인하기 위한 기즈모
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }
}