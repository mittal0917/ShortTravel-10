using UnityEngine;

public class EnemyDirector : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private int initialEnemyCount = 4;
    [SerializeField] private float spawnIntervalSeconds = 12f;
    [SerializeField] private float spawnMarginFromEdge = 8f;
    [SerializeField] private float minDistanceFromPlayer = 6f;

    private character_move player;
    private Map_Generator mapGenerator;
    private float spawnTimer;

    void Start()
    {
        player = FindObjectOfType<character_move>();
        mapGenerator = FindObjectOfType<Map_Generator>();

        for (int i = 0; i < initialEnemyCount; i++)
        {
            SpawnEnemy();
        }
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

        enemyObject.AddComponent<Enemy_NormalZombie>();
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
