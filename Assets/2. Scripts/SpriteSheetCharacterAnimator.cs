using System.Collections.Generic;
using UnityEngine;

public class SpriteSheetCharacterAnimator : MonoBehaviour
{
    public enum CharacterAsset
    {
        PlayerAdventurer,
        Zombie
    }

    [SerializeField] private CharacterAsset characterAsset = CharacterAsset.PlayerAdventurer;
    [SerializeField] private float framesPerSecond = 8f;
    [SerializeField] private int frameWidth = 64;
    [SerializeField] private int frameHeight = 64;
    [SerializeField] private float pixelsPerUnit = 64f;
    [SerializeField] private int sortingOrder = 5;
    [SerializeField] private Vector3 visualScale = Vector3.one;

    private readonly Dictionary<string, Sprite[]> clips = new Dictionary<string, Sprite[]>();
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private character_move playerMovement;
    private string currentClipKey;
    private int frameIndex;
    private float frameTimer;
    private bool playingDeath;
    private bool isInitialized;

    public void ConfigureForPlayer()
    {
        characterAsset = CharacterAsset.PlayerAdventurer;
        frameWidth = 64;
        frameHeight = 64;
        pixelsPerUnit = 64f;
        framesPerSecond = 8f;
        sortingOrder = 5;
        // 새 맵 이미지의 오브젝트 밀도에 맞춰 플레이어가 화면에서 너무 작아 보이지 않도록 시각 크기만 키웁니다.
        visualScale = new Vector3(1.35f, 1.35f, 1f);
        RefreshConfiguration();
    }

    public void ConfigureForZombie()
    {
        characterAsset = CharacterAsset.Zombie;
        frameWidth = 32;
        frameHeight = 32;
        pixelsPerUnit = 32f;
        framesPerSecond = 6f;
        sortingOrder = 10;
        visualScale = new Vector3(1.35f, 1.35f, 1f);
        RefreshConfiguration();
    }

    public void PlayDeath()
    {
        playingDeath = true;
        SetClip("death_down", true);
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerMovement = GetComponent<character_move>();
        CreateRenderer();
        LoadClips();
        HidePlaceholderRenderersIfReady();
        isInitialized = true;
    }

    void Update()
    {
        if (spriteRenderer == null || clips.Count == 0)
        {
            return;
        }

        if (!playingDeath)
        {
            Vector2 direction = GetDirection();
            bool isMoving = GetVelocity().sqrMagnitude > 0.01f || IsPlayerInputMoving();
            string clipKey = BuildClipKey(isMoving, direction);
            SetClip(clipKey, false);
        }

        AdvanceFrame();
    }

    private void CreateRenderer()
    {
        GameObject visual = new GameObject("SpriteVisual");
        visual.transform.SetParent(transform, false);
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localScale = visualScale;

        spriteRenderer = visual.AddComponent<SpriteRenderer>();
        spriteRenderer.sortingOrder = sortingOrder;
    }

    private void LoadClips()
    {
        clips.Clear();
        currentClipKey = null;

        if (characterAsset == CharacterAsset.PlayerAdventurer)
        {
            AddClip("idle_down", "Sprites/PlayerAdventurer/idle_down");
            AddClip("idle_left_down", "Sprites/PlayerAdventurer/idle_left_down");
            AddClip("idle_right_down", "Sprites/PlayerAdventurer/idle_right_down");
            AddClip("idle_up", "Sprites/PlayerAdventurer/idle_up");
            AddClip("walk_down", "Sprites/PlayerAdventurer/walk_down");
            AddClip("walk_left_down", "Sprites/PlayerAdventurer/walk_left_down");
            AddClip("walk_right_down", "Sprites/PlayerAdventurer/walk_right_down");
            AddClip("walk_up", "Sprites/PlayerAdventurer/walk_up");
            AddClip("death_down", "Sprites/PlayerAdventurer/death_normal_down");
            return;
        }

        AddClip("idle_down", "Sprites/Zombies/1Zombie-Idle", 0);
        AddClip("walk_down", "Sprites/Zombies/1Zombie-Run", 0);
        AddClip("death_down", "Sprites/Zombies/1Zombie-Death1", 0);
        if (!clips.ContainsKey("idle_down"))
        {
            AddClip("idle_down", "Sprites/Zombies/1Zombie-Portrait", 0, 16, 16, 16f);
        }

        if (!clips.ContainsKey("walk_down") && clips.ContainsKey("idle_down"))
        {
            clips["walk_down"] = clips["idle_down"];
        }
    }

    private void RefreshConfiguration()
    {
        if (!isInitialized || spriteRenderer == null)
        {
            return;
        }

        spriteRenderer.sortingOrder = sortingOrder;
        spriteRenderer.transform.localScale = visualScale;
        LoadClips();
        HidePlaceholderRenderersIfReady();
    }

    private void AddClip(string key, string resourcePath, int rowFromTop = 0)
    {
        AddClip(key, resourcePath, rowFromTop, frameWidth, frameHeight, pixelsPerUnit);
    }

    private void AddClip(string key, string resourcePath, int rowFromTop, int clipFrameWidth, int clipFrameHeight, float clipPixelsPerUnit)
    {
        Texture2D texture = Resources.Load<Texture2D>(resourcePath);
        if (texture == null)
        {
            Debug.LogWarning($"Sprite sheet not found or failed to import: Resources/{resourcePath}");
            return;
        }

        texture.filterMode = FilterMode.Point;
        int columns = Mathf.Max(1, texture.width / clipFrameWidth);
        int rowY = Mathf.Max(0, texture.height - ((rowFromTop + 1) * clipFrameHeight));
        Sprite[] frames = new Sprite[columns];

        for (int i = 0; i < columns; i++)
        {
            Rect rect = new Rect(i * clipFrameWidth, rowY, clipFrameWidth, clipFrameHeight);
            frames[i] = Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f), clipPixelsPerUnit);
        }

        clips[key] = frames;
    }

    private void SetClip(string key, bool restart)
    {
        if (!clips.ContainsKey(key))
        {
            key = clips.ContainsKey("idle_down") ? "idle_down" : null;
        }

        if (string.IsNullOrEmpty(key))
        {
            return;
        }

        if (!restart && currentClipKey == key)
        {
            return;
        }

        currentClipKey = key;
        frameIndex = 0;
        frameTimer = 0f;
        spriteRenderer.sprite = clips[currentClipKey][frameIndex];
    }

    private void AdvanceFrame()
    {
        if (string.IsNullOrEmpty(currentClipKey) || !clips.ContainsKey(currentClipKey))
        {
            return;
        }

        Sprite[] frames = clips[currentClipKey];
        if (frames.Length <= 1)
        {
            return;
        }

        frameTimer += Time.deltaTime;
        if (frameTimer < 1f / framesPerSecond)
        {
            return;
        }

        frameTimer = 0f;
        frameIndex = (frameIndex + 1) % frames.Length;
        spriteRenderer.sprite = frames[frameIndex];
    }

    private string BuildClipKey(bool isMoving, Vector2 direction)
    {
        if (characterAsset == CharacterAsset.Zombie)
        {
            return isMoving ? "walk_down" : "idle_down";
        }

        string state = isMoving ? "walk" : "idle";
        return state + "_" + GetDirectionSuffix(direction);
    }

    private string GetDirectionSuffix(Vector2 direction)
    {
        if (direction.y > 0.35f)
        {
            return "up";
        }

        if (direction.x < -0.35f)
        {
            return "left_down";
        }

        if (direction.x > 0.35f)
        {
            return "right_down";
        }

        return "down";
    }

    private Vector2 GetDirection()
    {
        if (playerMovement != null && playerMovement.FacingDirection.sqrMagnitude > 0.001f)
        {
            return playerMovement.FacingDirection.normalized;
        }

        if (characterAsset == CharacterAsset.PlayerAdventurer)
        {
            Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            if (input.sqrMagnitude > 0.001f)
            {
                return input.normalized;
            }
        }

        Vector2 velocity = GetVelocity();
        return velocity.sqrMagnitude > 0.001f ? velocity.normalized : Vector2.down;
    }

    private Vector2 GetVelocity()
    {
        if (rb != null)
        {
            return rb.velocity;
        }

        return Vector2.zero;
    }

    private bool IsPlayerInputMoving()
    {
        if (characterAsset != CharacterAsset.PlayerAdventurer)
        {
            return false;
        }

        return Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.01f
            || Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0.01f;
    }

    private void HidePlaceholderRenderersIfReady()
    {
        if (clips.Count == 0)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = false;
            }

            return;
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }

        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>(true);
        foreach (SpriteRenderer renderer in renderers)
        {
            if (renderer == spriteRenderer)
            {
                continue;
            }

            string objectName = renderer.gameObject.name;
            if (objectName.StartsWith("Life_")
                || objectName == "HealthSlot"
                || objectName == "HealthSlots")
            {
                continue;
            }

            if (renderer.transform == transform || renderer.transform.parent == transform)
            {
                renderer.enabled = false;
            }
        }
    }
}
