using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameSessionManager : MonoBehaviour
{
    private static GameSessionManager instance;

    [Header("Escape Settings")]
    [SerializeField] private float escapeDetectionRangeTiles = 3f;
    [SerializeField] private float escapeCountdownSeconds = 8f;
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
    private bool isEnding;

    public static GameSessionManager Instance => instance;
    public static bool IsGameEnding => instance != null && instance.isEnding;

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
        EnsureEnemyDirector();
        CreateTimerUi();
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

        if (instance != null)
        {
            instance.isEnding = true;
        }

        Debug.Log($"Game ended: {reason}");
        GameProgress.ClearSave();
        GameProgress.ClearNewGameRequest();
        SceneManager.LoadScene("LobbyScene");
    }

    public void SetSupplyProgress(int currentCount)
    {
        carriedSupplyCount = currentCount;
    }

    public void SetRequiredSupplyCount(int requiredCount)
    {
        requiredSupplyCount = requiredCount;
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

        if (!CanEscape)
        {
            escapeTimer = 0f;
            return;
        }

        float distanceToExit = Vector2.Distance(player.transform.position, mapGenerator.ExitApproachPointWorld);
        if (distanceToExit <= escapeDetectionRangeTiles)
        {
            escapeTimer += Time.deltaTime;
            if (escapeTimer >= escapeCountdownSeconds)
            {
                EndGame("Escape completed");
            }
        }
        else
        {
            escapeTimer = 0f;
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
}
