using UnityEngine;
using UnityEngine.Serialization;

public class ItemSpawner : MonoBehaviour
{
    public GameObject Supply;
    [FormerlySerializedAs("gunCount")]
    public int supplyCount = 20;

    public Vector2 minPos;
    public Vector2 maxPos;

    private character_move player;
    private Map_Generator mapGenerator;

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

        if (player != null)
        {
            Vector2 playerPosition = player.transform.position;
            minPos = playerPosition - new Vector2(18f, 18f);
            maxPos = playerPosition + new Vector2(18f, 18f);

            if (mapGenerator != null)
            {
                minPos = new Vector2(
                    Mathf.Clamp(minPos.x, 4f, mapGenerator.mapWidth - 4f),
                    Mathf.Clamp(minPos.y, 4f, mapGenerator.mapHeight - 4f));
                maxPos = new Vector2(
                    Mathf.Clamp(maxPos.x, 4f, mapGenerator.mapWidth - 4f),
                    Mathf.Clamp(maxPos.y, 4f, mapGenerator.mapHeight - 4f));
            }

            return;
        }

        minPos = new Vector2(-12f, -12f);
        maxPos = new Vector2(12f, 12f);
    }

    private void SpawnSupply()
    {
        Vector2 randomPos = FindSpawnPosition();
        GameObject supplyObject = Supply != null
            ? Instantiate(Supply, randomPos, Quaternion.identity)
            : CreateRuntimeSupply(randomPos);

        supplyObject.name = "Supply";
    }

    private Vector2 FindSpawnPosition()
    {
        for (int i = 0; i < 20; i++)
        {
            Vector2 candidate = new Vector2(
                Random.Range(minPos.x, maxPos.x),
                Random.Range(minPos.y, maxPos.y));

            if (player == null || Vector2.Distance(candidate, player.transform.position) >= 5f)
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
        renderer.sprite = BuildSquareSprite();
        renderer.color = new Color(0.95f, 0.78f, 0.18f, 1f);
        renderer.sortingOrder = 9;

        BoxCollider2D collider = supplyObject.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = Vector2.one;

        supplyObject.AddComponent<SupplyItem>();
        return supplyObject;
    }

    private static Sprite BuildSquareSprite()
    {
        const int size = 32;
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
