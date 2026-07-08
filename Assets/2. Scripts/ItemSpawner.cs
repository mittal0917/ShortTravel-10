using UnityEngine;
using UnityEngine.Serialization;
using System.Collections.Generic;

public class ItemSpawner : MonoBehaviour
{
    public GameObject Supply;
    [FormerlySerializedAs("gunCount")]
    public int supplyCount = 28;

    public Vector2 minPos;
    public Vector2 maxPos;
    [SerializeField] private float mapEdgeMargin = 5f;
    [SerializeField] private float minDistanceFromPlayer = 8f;
    [SerializeField] private float minDistanceBetweenSupplies = 4f;

    private character_move player;
    private Map_Generator mapGenerator;
    private readonly List<Vector2> spawnedPositions = new List<Vector2>();

    void Start()
    {
        player = FindObjectOfType<character_move>();
        mapGenerator = FindObjectOfType<Map_Generator>();
        ResolveSpawnBounds();

        for (int i = 0; i < supplyCount; i++)
        {
            SpawnSupply();
        }
    }

    private void ResolveSpawnBounds()
    {
        if (minPos != Vector2.zero || maxPos != Vector2.zero)
        {
            return;
        }

        if (mapGenerator != null)
        {
            minPos = new Vector2(mapEdgeMargin, mapEdgeMargin);
            maxPos = new Vector2(
                Mathf.Max(mapEdgeMargin + 1f, mapGenerator.mapWidth - mapEdgeMargin),
                Mathf.Max(mapEdgeMargin + 1f, mapGenerator.mapHeight - mapEdgeMargin));
            return;
        }

        if (player != null)
        {
            Vector2 playerPosition = player.transform.position;
            minPos = playerPosition - new Vector2(28f, 28f);
            maxPos = playerPosition + new Vector2(28f, 28f);
            return;
        }

        minPos = new Vector2(-24f, -24f);
        maxPos = new Vector2(24f, 24f);
    }

    private void SpawnSupply()
    {
        Vector2 randomPos = FindSpawnPosition();
        GameObject supplyObject = Supply != null
            ? Instantiate(Supply, randomPos, Quaternion.identity)
            : CreateRuntimeSupply(randomPos);

        supplyObject.name = "Supply";
        spawnedPositions.Add(randomPos);
    }

    private Vector2 FindSpawnPosition()
    {
        for (int i = 0; i < 80; i++)
        {
            Vector2 candidate = new Vector2(
                Random.Range(minPos.x, maxPos.x),
                Random.Range(minPos.y, maxPos.y));

            if (IsValidSupplyPosition(candidate))
            {
                return candidate;
            }
        }

        return new Vector2(Random.Range(minPos.x, maxPos.x), Random.Range(minPos.y, maxPos.y));
    }

    private GameObject CreateRuntimeSupply(Vector2 position)
    {
        GameObject supplyObject = new GameObject("Supply");
        supplyObject.transform.position = position;

        SpriteRenderer renderer = supplyObject.AddComponent<SpriteRenderer>();
        renderer.sprite = SupplyVisuals.GetSupplySprite();
        renderer.color = Color.white;
        renderer.sortingOrder = 9;

        BoxCollider2D collider = supplyObject.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = Vector2.one;

        supplyObject.AddComponent<SupplyItem>();
        return supplyObject;
    }

    private bool IsValidSupplyPosition(Vector2 candidate)
    {
        // 물, 나무, 돌 같은 충돌 타일 위에는 물자가 생성되지 않도록 맵 생성기와 한 번 더 확인합니다.
        if (mapGenerator != null && !mapGenerator.IsWalkableWorld(candidate))
        {
            return false;
        }

        if (player != null && Vector2.Distance(candidate, player.transform.position) < minDistanceFromPlayer)
        {
            return false;
        }

        foreach (Vector2 position in spawnedPositions)
        {
            if (Vector2.Distance(candidate, position) < minDistanceBetweenSupplies)
            {
                return false;
            }
        }

        return true;
    }
}
