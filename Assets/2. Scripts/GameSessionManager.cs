using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameSessionManager : MonoBehaviour
{
    private static GameSessionManager instance;
    private static float timeScaleBeforePause = 1f;

    [Header("Escape Settings")]
    [SerializeField] private float escapeDetectionRangeTiles = 3f;
    [SerializeField] private float escapeCountdownSeconds = 8f;
    [SerializeField] private int escapeDoorMaxHealth = 5;
    [SerializeField] private float escapeDoorDamageIntervalSeconds = 1f;
    [SerializeField] private bool requireSuppliesToEscape;
    [SerializeField] private int requiredSupplyCount;
    [SerializeField] private int carriedSupplyCount;

    [Header("UI Settings")]
    [SerializeField] private Color timerTextColor = Color.white;
    [SerializeField] private int timerFontSize = 36;

    private character_move player;
    private Map_Generator mapGenerator;
    private EnemyDirector enemyDirector;
    private Text timerText;
    private float elapsedSeconds;
    private float escapeTimer;
    private int escapeDoorHealth;
    private float lastEscapeDoorDamageTime = float.MinValue;
    private bool escapeDoorClosed;
    private bool escapeDoorBroken;
    private bool isEnding;

    public static GameSessionManager Instance => instance;
    public static bool IsGameEnding => instance != null && instance.isEnding;
    public static bool IsEscapeDoorBlocking => instance != null && instance.escapeDoorClosed && !instance.escapeDoorBroken;
    public static bool IsGamePaused { get; private set; }

    public bool CanEscape => !requireSuppliesToEscape || carriedSupplyCount >= requiredSupplyCount;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsureManager()
    {
        EnsureExistsInScene();
    }

    public static void EnsureExistsInScene()
    {
        if (SceneManager.GetActiveScene().name != "GameScene")
        {
            return;
        }

        if (FindObjectOfType<GameSessionManager>() != null)
        {
            return;
        }

        GameObject managerObject = new GameObject("GameSessionManager");
        managerObject.AddComponent<GameSessionManager>();
    }

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    void Start()
    {
        player = FindObjectOfType<character_move>();
        mapGenerator = FindObjectOfType<Map_Generator>();
        escapeDoorHealth = Mathf.Max(1, escapeDoorMaxHealth);
        EnsureEnemyDirector();
        EnsureItemSpawner();
        CreateTimerUi();
        SyncSupplyUi();
        UpdateTimerText();
    }

    void Update()
    {
        if (isEnding)
        {
            return;
        }

        elapsedSeconds += Time.deltaTime;
        UpdateTimerText();
        UpdateEscapeState();
    }

    public static void EndGame(string reason)
    {
        if (instance != null && instance.isEnding)
        {
            return;
        }

        SetGamePaused(false);

        if (instance != null)
        {
            instance.isEnding = true;
        }

        Debug.Log($"Game ended: {reason}");
        GameProgress.ClearSave();
        GameProgress.ClearNewGameRequest();
        SceneManager.LoadScene("LobbyScene");
    }

    public static void SetGamePaused(bool paused)
    {
        if (paused == IsGamePaused)
        {
            return;
        }

        if (paused)
        {
            // 설정창이 열린 동안에는 게임 진행과 진행 시간 계산이 함께 멈추도록 전체 시간을 정지합니다.
            timeScaleBeforePause = Time.timeScale > 0f ? Time.timeScale : 1f;
            Time.timeScale = 0f;
            IsGamePaused = true;
            return;
        }

        Time.timeScale = timeScaleBeforePause > 0f ? timeScaleBeforePause : 1f;
        IsGamePaused = false;
    }

    public void SetSupplyProgress(int currentCount)
    {
        carriedSupplyCount = currentCount;
        ApplySupplyDifficulty();
        SyncSupplyUi();
    }

    public void SetRequiredSupplyCount(int requiredCount)
    {
        requiredSupplyCount = requiredCount;
        SyncSupplyUi();
    }

    public void RegisterSupplyCollected()
    {
        carriedSupplyCount++;
        ApplySupplyDifficulty();
        GrantSupplyAmmo();
        SyncSupplyUi();
    }

    private void SyncSupplyUi()
    {
        SupplyManager supplyManager = SupplyManager.Instance != null
            ? SupplyManager.Instance
            : FindObjectOfType<SupplyManager>();

        if (supplyManager != null)
        {
            supplyManager.SetSupplyProgress(carriedSupplyCount, requiredSupplyCount > 0 ? requiredSupplyCount : 5);
        }
    }

    private void ApplySupplyDifficulty()
    {
        if (enemyDirector == null)
        {
            EnsureEnemyDirector();
        }

        if (enemyDirector != null)
        {
            enemyDirector.ApplySupplyCount(carriedSupplyCount);
        }
    }

    private void GrantSupplyAmmo()
    {
        PlayerAttack playerAttack = FindObjectOfType<PlayerAttack>();
        if (playerAttack != null)
        {
            playerAttack.AddAmmoFromSupply();
        }
    }

    private void UpdateEscapeState()
    {
        if (player == null)
        {
            player = FindObjectOfType<character_move>();
            if (player == null)
            {
                escapeTimer = 0f;
                return;
            }
        }

        if (mapGenerator == null)
        {
            mapGenerator = FindObjectOfType<Map_Generator>();
            if (mapGenerator == null || !mapGenerator.HasExitHole)
            {
                escapeTimer = 0f;
                return;
            }
        }
        else if (!mapGenerator.HasExitHole)
        {
            escapeTimer = 0f;
            return;
        }

        if (!CanEscape)
        {
            ResetEscapeAttempt(false);
            return;
        }

        bool playerInExitDoorArea = mapGenerator.IsInExitDoorArea(player.transform.position);
        float distanceToExit = Vector2.Distance(player.transform.position, mapGenerator.ExitApproachPointWorld);
        if (playerInExitDoorArea && distanceToExit <= escapeDetectionRangeTiles)
        {
            // 플레이어가 1x3 탈출구 영역에 진입하면 즉시 문을 닫아 좀비가 따라오지 못하게 합니다.
            CloseEscapeDoor();
            if (escapeDoorBroken)
            {
                escapeTimer = 0f;
                return;
            }

            escapeTimer += Time.deltaTime;
            if (escapeTimer >= escapeCountdownSeconds)
            {
                EndGame("Escape completed");
            }
        }
        else
        {
            ResetEscapeAttempt(!playerInExitDoorArea);
        }
    }

    public static bool TryBlockEnemyMovementAtEscapeDoor(Vector2 currentPosition, Vector2 nextPosition, out Vector2 blockedPosition)
    {
        blockedPosition = currentPosition;
        if (!IsEscapeDoorBlocking || instance == null || instance.mapGenerator == null)
        {
            return false;
        }

        if (!instance.mapGenerator.TryBlockEnemyAtClosedExitDoor(currentPosition, nextPosition, out blockedPosition))
        {
            return false;
        }

        instance.ApplyEscapeDoorDamage();
        return true;
    }

    private void CloseEscapeDoor()
    {
        if (escapeDoorClosed || escapeDoorBroken)
        {
            return;
        }

        escapeDoorClosed = true;
        escapeDoorHealth = Mathf.Clamp(escapeDoorHealth, 1, Mathf.Max(1, escapeDoorMaxHealth));
        lastEscapeDoorDamageTime = Time.time;

        if (mapGenerator != null)
        {
            mapGenerator.SetExitDoorClosed(true);
        }

        Debug.Log($"탈출구 문이 닫혔습니다. 문 체력: {escapeDoorHealth}");
    }

    private void ApplyEscapeDoorDamage()
    {
        if (!escapeDoorClosed || escapeDoorBroken)
        {
            return;
        }

        // 여러 좀비가 동시에 붙어도 문은 지정된 간격마다 1씩만 피해를 받습니다.
        if (Time.time < lastEscapeDoorDamageTime + escapeDoorDamageIntervalSeconds)
        {
            return;
        }

        lastEscapeDoorDamageTime = Time.time;
        escapeDoorHealth = Mathf.Max(0, escapeDoorHealth - 1);
        Debug.Log($"탈출구 문이 피해를 입었습니다. 남은 체력: {escapeDoorHealth}");

        if (escapeDoorHealth <= 0)
        {
            BreakEscapeDoor();
        }
    }

    private void BreakEscapeDoor()
    {
        escapeDoorBroken = true;
        escapeDoorClosed = false;
        escapeTimer = 0f;

        // 즉시 사망시키지 않고 문만 열어, 좀비가 자연스럽게 들어와 플레이어를 공격하게 둡니다.
        if (mapGenerator != null)
        {
            mapGenerator.SetExitDoorClosed(false);
        }

        Debug.Log("탈출구 문이 파괴되어 탈출 시도가 중단되었습니다.");
    }

    private void ResetEscapeAttempt(bool reopenDoor)
    {
        escapeTimer = 0f;

        // 문 뒤에 있는 동안에는 탈출 시도가 잠시 끊겨도 문을 다시 열지 않습니다.
        if (!reopenDoor || !escapeDoorClosed || escapeDoorBroken)
        {
            return;
        }

        escapeDoorClosed = false;
        if (mapGenerator != null)
        {
            mapGenerator.SetExitDoorClosed(false);
        }
    }

    private void CreateTimerUi()
    {
        if (timerText != null)
        {
            return;
        }

        Canvas parentCanvas = FindSceneCanvas();
        if (parentCanvas == null)
        {
            Debug.LogWarning("GameScene Canvas를 찾지 못해 타이머 UI를 만들지 못했습니다.");
            return;
        }

        GameObject textObject = new GameObject("GameTimerText");
        textObject.transform.SetParent(parentCanvas.transform, false);

        timerText = textObject.AddComponent<Text>();
        timerText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        timerText.fontSize = timerFontSize;
        timerText.alignment = TextAnchor.UpperCenter;
        timerText.color = timerTextColor;
        timerText.horizontalOverflow = HorizontalWrapMode.Overflow;
        timerText.verticalOverflow = VerticalWrapMode.Overflow;

        Outline outline = textObject.AddComponent<Outline>();
        outline.effectColor = new Color(0f, 0f, 0f, 0.85f);
        outline.effectDistance = new Vector2(2f, -2f);

        RectTransform textRect = timerText.rectTransform;
        textRect.anchorMin = new Vector2(0.5f, 1f);
        textRect.anchorMax = new Vector2(0.5f, 1f);
        textRect.pivot = new Vector2(0.5f, 1f);
        textRect.anchoredPosition = new Vector2(0f, -24f);
        textRect.sizeDelta = new Vector2(420f, 60f);
        textRect.localScale = Vector3.one;
        textObject.transform.SetAsLastSibling();

    }

    private void UpdateTimerText()
    {
        if (timerText == null)
        {
            return;
        }

        timerText.text = $"진행 시간 {Mathf.FloorToInt(elapsedSeconds)}초";
    }

    private Canvas FindSceneCanvas()
    {
        foreach (Canvas canvas in FindObjectsOfType<Canvas>())
        {
            if (canvas.gameObject.scene.name != "GameScene")
            {
                continue;
            }

            if (canvas.isRootCanvas)
            {
                return canvas;
            }
        }

        return null;
    }

    private void EnsureEnemyDirector()
    {
        enemyDirector = FindObjectOfType<EnemyDirector>();
        if (enemyDirector != null)
        {
            return;
        }

        GameObject directorObject = new GameObject("EnemyDirector");
        enemyDirector = directorObject.AddComponent<EnemyDirector>();
    }

    private void EnsureItemSpawner()
    {
        if (FindObjectOfType<ItemSpawner>() != null)
        {
            return;
        }

        GameObject spawnerObject = new GameObject("ItemSpawner");
        spawnerObject.AddComponent<ItemSpawner>();
    }
}
