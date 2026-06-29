using UnityEngine;

public class EnemyDirector : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private int initialEnemyCount = 4;
    [SerializeField] private float spawnIntervalSeconds = 12f;
    [SerializeField] private float spawnMarginFromEdge = 8f;
    [SerializeField] private float minDistanceFromPlayer = 6f;

    [Header("Supply Scaling")]
    [SerializeField] private int suppliesPerDifficultyLevel = 3;
    [SerializeField] private int maxDifficultyLevel = 6;
    [SerializeField] private int baseZombieHealthSlots = 5;
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

        ApplyZombieHealthToExistingEnemies();
        Debug.Log($"좀비 강화 단계 {difficultyLevel}: 스폰 {spawnIntervalSeconds:0.0}초, 체력 {GetCurrentZombieHealthSlots()}칸");
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

        SpriteRenderer renderer = enemyObject.AddComponent<SpriteRenderer>();
        renderer.sprite = BuildSquareSprite();
        renderer.color = new Color(0.72f, 0.9f, 0.72f, 1f);
        renderer.sortingOrder = 10;
        enemyObject.transform.localScale = new Vector3(0.9f, 0.9f, 1f);

        Enemy_NormalZombie zombie = enemyObject.AddComponent<Enemy_NormalZombie>();
        zombie.ConfigureHealthSlots(GetCurrentZombieHealthSlots(), true);
    }

    private int GetCurrentZombieHealthSlots()
    {
        return baseZombieHealthSlots + difficultyLevel;
    }

    private void ApplyZombieHealthToExistingEnemies()
    {
        Enemy_NormalZombie[] enemies = FindObjectsOfType<Enemy_NormalZombie>();
        foreach (Enemy_NormalZombie enemy in enemies)
        {
            if (enemy != null)
            {
                enemy.ConfigureHealthSlots(GetCurrentZombieHealthSlots(), refillExistingZombieHealthOnLevelUp);
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
            if (player == null || Vector2.Distance(candidate, player.transform.position) >= minDistanceFromPlayer)
            {
                return candidate;
            }
        }

        return new Vector3(Random.Range(minX, maxX), Random.Range(minY, maxY), 0f);
    }

    private static Sprite BuildSquareSprite()
    {
        const int size = 64;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                texture.SetPixel(x, y, Color.white);
            }
        }

        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
    }
}
