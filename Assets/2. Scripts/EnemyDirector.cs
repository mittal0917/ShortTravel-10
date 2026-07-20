using UnityEngine;

public class EnemyDirector : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private int initialEnemyCount = 4;
    [SerializeField] private float spawnIntervalSeconds = 6f;
    [SerializeField] private float spawnMarginFromEdge = 8f;
    [SerializeField] private float minDistanceFromPlayer = 6f;

    [Header("Supply Scaling")]
    [SerializeField] private int suppliesPerDifficultyLevel = 3;
    [SerializeField] private int maxDifficultyLevel = 6;
    [SerializeField] private int baseZombieHealthSlots = 5;
    [SerializeField] private float baseZombieMoveSpeed = 3f;
    [SerializeField] private float zombieMoveSpeedIncreasePerLevel = 0.25f;
    [SerializeField] private float spawnIntervalDecreasePerLevel = 0.5f;
    [SerializeField] private float minimumSpawnIntervalSeconds = 3f;
    [SerializeField] private bool refillExistingZombieHealthOnLevelUp = true;

    private character_move player;
    private Map_Generator mapGenerator;
    private float baseSpawnIntervalSeconds;
    private float spawnTimer;
    private int difficultyLevel;

    void Awake()
    {
        // 초기 좀비 리스폰 속도를 기존보다 2배 빠르게 시작합니다.
        baseSpawnIntervalSeconds = spawnIntervalSeconds;
    }

    void Start()
    {
        player = FindObjectOfType<character_move>();
        mapGenerator = FindObjectOfType<Map_Generator>();

        for (int i = 0; i < initialEnemyCount; i++)
        {
            SpawnEnemy();
        }
    }

    public void ApplySupplyCount(int supplyCount)
    {
        int nextDifficultyLevel = Mathf.Clamp(
            supplyCount / Mathf.Max(1, suppliesPerDifficultyLevel),
            0,
            maxDifficultyLevel);
        if (nextDifficultyLevel == difficultyLevel)
        {
            return;
        }

        difficultyLevel = nextDifficultyLevel;
        spawnIntervalSeconds = Mathf.Max(
            minimumSpawnIntervalSeconds,
            baseSpawnIntervalSeconds - difficultyLevel * spawnIntervalDecreasePerLevel);

        ApplyZombieStatsToExistingEnemies();
        Debug.Log($"좀비 강화 단계 {difficultyLevel}: 스폰 {spawnIntervalSeconds:0.0}초, 체력 {GetCurrentZombieHealthSlots()}칸, 속도 {GetCurrentZombieMoveSpeed():0.00}");
    }

    void Update()
    {
        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnIntervalSeconds)
        {
            spawnTimer = 0f;
            SpawnEnemy();
        }
    }

    private void SpawnEnemy()
    {
        if (mapGenerator == null)
        {
            mapGenerator = FindObjectOfType<Map_Generator>();
            if (mapGenerator == null)
            {
                return;
            }
        }

        if (player == null)
        {
            player = FindObjectOfType<character_move>();
        }

        Vector3 spawnPosition = FindSpawnPosition();

        GameObject enemyObject = new GameObject("Enemy_RuntimeZombie");
        enemyObject.transform.position = spawnPosition;

        enemyObject.transform.localScale = new Vector3(0.9f, 0.9f, 1f);

        Enemy_NormalZombie zombie = enemyObject.AddComponent<Enemy_NormalZombie>();
        zombie.ConfigureHealthSlots(GetCurrentZombieHealthSlots(), true);
        zombie.ConfigureMoveSpeed(GetCurrentZombieMoveSpeed());
    }

    private int GetCurrentZombieHealthSlots()
    {
        return baseZombieHealthSlots + difficultyLevel;
    }

    private float GetCurrentZombieMoveSpeed()
    {
        return baseZombieMoveSpeed + difficultyLevel * zombieMoveSpeedIncreasePerLevel;
    }

    private void ApplyZombieStatsToExistingEnemies()
    {
        Enemy_NormalZombie[] enemies = FindObjectsOfType<Enemy_NormalZombie>();
        foreach (Enemy_NormalZombie enemy in enemies)
        {
            if (enemy != null)
            {
                enemy.ConfigureHealthSlots(GetCurrentZombieHealthSlots(), refillExistingZombieHealthOnLevelUp);
                enemy.ConfigureMoveSpeed(GetCurrentZombieMoveSpeed());
            }
        }
    }

    private Vector3 FindSpawnPosition()
    {
        float minX = spawnMarginFromEdge;
        float maxX = Mathf.Max(minX + 1f, mapGenerator.mapWidth - spawnMarginFromEdge);
        float minY = spawnMarginFromEdge;
        float maxY = Mathf.Max(minY + 1f, mapGenerator.mapHeight - spawnMarginFromEdge);

        for (int i = 0; i < 20; i++)
        {
            Vector3 candidate = new Vector3(Random.Range(minX, maxX), Random.Range(minY, maxY), 0f);
            // 물가/나무/돌 같은 장애물 위에는 좀비가 생성되지 않게 맵 이동 가능 여부를 확인합니다.
            bool farEnoughFromPlayer = player == null || Vector2.Distance(candidate, player.transform.position) >= minDistanceFromPlayer;
            bool walkableCell = mapGenerator == null || mapGenerator.IsWalkableWorld(candidate);
            if (farEnoughFromPlayer && walkableCell)
            {
                return candidate;
            }
        }

        return new Vector3(Random.Range(minX, maxX), Random.Range(minY, maxY), 0f);
    }

}
