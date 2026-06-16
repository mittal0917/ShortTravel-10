using System.Collections.Generic;
using UnityEngine;

public class PlayerStatus : MonoBehaviour
{
    [Header("Life Settings")]
    [SerializeField] private int maxLives = 3;
    [SerializeField] private float damageCooldownSeconds = 1f;
    [SerializeField] private Vector3 heartAnchorOffset = new Vector3(0f, 1.1f, 0f);
    [SerializeField] private float heartSpacing = 0.35f;
    [SerializeField] private float heartRadius = 0.12f;
    [SerializeField] private Color activeHeartColor = new Color(1f, 1f, 1f, 1f);
    [SerializeField] private Color inactiveHeartColor = new Color(1f, 1f, 1f, 0.2f);

    private readonly List<SpriteRenderer> lifeRenderers = new List<SpriteRenderer>();
    private int currentLives;
    private float lastDamageTime = float.MinValue;

    void Awake()
    {
        currentLives = maxLives;
        CreateLifeIndicators();
        RefreshLifeIndicators();
    }

    public void TakeDamage(int damageAmount = 1)
    {
        if (Time.time < lastDamageTime + damageCooldownSeconds)
        {
            return;
        }

        if (currentLives <= 0)
        {
            return;
        }

        lastDamageTime = Time.time;
        currentLives = Mathf.Max(0, currentLives - damageAmount);
        RefreshLifeIndicators();

        if (currentLives <= 0)
        {
            GameSessionManager.EndGame("Player died");
        }
    }

    private void CreateLifeIndicators()
    {
        if (lifeRenderers.Count > 0)
        {
            return;
        }

        Sprite circleSprite = BuildCircleSprite();
        float startOffset = -heartSpacing * (maxLives - 1) * 0.5f;

        for (int i = 0; i < maxLives; i++)
        {
            GameObject lifeObject = new GameObject($"Life_{i + 1}");
            lifeObject.transform.SetParent(transform, false);
            lifeObject.transform.localPosition = heartAnchorOffset + new Vector3(startOffset + (heartSpacing * i), 0f, 0f);

            SpriteRenderer renderer = lifeObject.AddComponent<SpriteRenderer>();
            renderer.sprite = circleSprite;
            renderer.sortingOrder = 20;
            renderer.transform.localScale = Vector3.one * heartRadius;
            renderer.drawMode = SpriteDrawMode.Simple;
            lifeRenderers.Add(renderer);
        }
    }

    private void RefreshLifeIndicators()
    {
        for (int i = 0; i < lifeRenderers.Count; i++)
        {
            if (lifeRenderers[i] == null)
            {
                continue;
            }

            lifeRenderers[i].color = i < currentLives ? activeHeartColor : inactiveHeartColor;
            lifeRenderers[i].enabled = i < currentLives;
        }
    }

    private static Sprite BuildCircleSprite()
    {
        const int size = 64;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Bilinear;

        Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
        float radius = size * 0.45f;

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                Color color = distance <= radius ? Color.white : Color.clear;
                texture.SetPixel(x, y, color);
            }
        }

        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
    }
}
