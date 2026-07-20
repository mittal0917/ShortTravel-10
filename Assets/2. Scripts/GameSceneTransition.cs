using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameSceneTransition : MonoBehaviour
{
    private static GameSceneTransition instance;

    [Header("Ending")]
    [SerializeField] private float endingFadeSeconds = 5f;
    [SerializeField] private int endingFontSize = 92;

    [Header("Loading")]
    [SerializeField] private float minimumLoadingSeconds = 1.2f;
    [SerializeField] private float loadingDotIntervalSeconds = 0.35f;
    [SerializeField] private int loadingFontSize = 70;

    private Canvas overlayCanvas;
    private Image blackOverlay;
    private Text messageText;
    private bool isTransitioning;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void CreateOnStart()
    {
        EnsureExists();
    }

    public static void LoadSceneWithLoading(string sceneName)
    {
        EnsureExists();
        instance.StartLoading(sceneName);
    }

    public static void PlayEnding(string message, Color textColor, string nextSceneName)
    {
        EnsureExists();
        instance.StartEnding(message, textColor, nextSceneName);
    }

    private static void EnsureExists()
    {
        if (instance != null)
        {
            return;
        }

        GameObject transitionObject = new GameObject("GameSceneTransition");
        DontDestroyOnLoad(transitionObject);
        instance = transitionObject.AddComponent<GameSceneTransition>();
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void StartLoading(string sceneName)
    {
        if (isTransitioning)
        {
            return;
        }

        StartCoroutine(LoadingRoutine(sceneName));
    }

    private void StartEnding(string message, Color textColor, string nextSceneName)
    {
        if (isTransitioning)
        {
            return;
        }

        StartCoroutine(EndingRoutine(message, textColor, nextSceneName));
    }

    private IEnumerator LoadingRoutine(string sceneName)
    {
        isTransitioning = true;
        Time.timeScale = 1f;
        EnsureOverlay();
        ShowOverlay(1f, Color.white);

        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(sceneName);
        if (loadOperation != null)
        {
            loadOperation.allowSceneActivation = false;
        }

        float elapsed = 0f;
        while (elapsed < minimumLoadingSeconds || (loadOperation != null && loadOperation.progress < 0.9f))
        {
            elapsed += Time.unscaledDeltaTime;
            UpdateLoadingText(elapsed);
            yield return null;
        }

        if (loadOperation != null)
        {
            loadOperation.allowSceneActivation = true;
            while (!loadOperation.isDone)
            {
                elapsed += Time.unscaledDeltaTime;
                UpdateLoadingText(elapsed);
                yield return null;
            }
        }

        HideOverlay();
        isTransitioning = false;
    }

    private IEnumerator EndingRoutine(string message, Color textColor, string nextSceneName)
    {
        isTransitioning = true;
        Time.timeScale = 0f;
        EnsureOverlay();
        ShowOverlay(0f, textColor);
        messageText.text = message;

        float elapsed = 0f;
        while (elapsed < endingFadeSeconds)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(elapsed / endingFadeSeconds);

            // 게임 종료 시 화면과 글씨가 함께 천천히 나타나도록 투명도를 올립니다.
            blackOverlay.color = new Color(0f, 0f, 0f, progress);
            messageText.color = new Color(textColor.r, textColor.g, textColor.b, progress);
            yield return null;
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(nextSceneName);
        HideOverlay();
        isTransitioning = false;
    }

    private void EnsureOverlay()
    {
        if (overlayCanvas != null)
        {
            return;
        }

        GameObject canvasObject = new GameObject("TransitionCanvas");
        canvasObject.transform.SetParent(transform, false);
        overlayCanvas = canvasObject.AddComponent<Canvas>();
        overlayCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        overlayCanvas.sortingOrder = 10000;
        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.matchWidthOrHeight = 1f;
        canvasObject.AddComponent<GraphicRaycaster>();

        GameObject backgroundObject = new GameObject("BlackOverlay");
        backgroundObject.transform.SetParent(canvasObject.transform, false);
        blackOverlay = backgroundObject.AddComponent<Image>();
        blackOverlay.color = Color.black;
        RectTransform backgroundRect = blackOverlay.rectTransform;
        backgroundRect.anchorMin = Vector2.zero;
        backgroundRect.anchorMax = Vector2.one;
        backgroundRect.offsetMin = Vector2.zero;
        backgroundRect.offsetMax = Vector2.zero;

        GameObject textObject = new GameObject("TransitionText");
        textObject.transform.SetParent(canvasObject.transform, false);
        messageText = textObject.AddComponent<Text>();
        messageText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        messageText.alignment = TextAnchor.MiddleCenter;
        messageText.horizontalOverflow = HorizontalWrapMode.Overflow;
        messageText.verticalOverflow = VerticalWrapMode.Overflow;

        RectTransform textRect = messageText.rectTransform;
        textRect.anchorMin = new Vector2(0.5f, 0.5f);
        textRect.anchorMax = new Vector2(0.5f, 0.5f);
        textRect.pivot = new Vector2(0.5f, 0.5f);
        textRect.anchoredPosition = Vector2.zero;
        textRect.sizeDelta = new Vector2(900f, 220f);
    }

    private void ShowOverlay(float backgroundAlpha, Color textColor)
    {
        overlayCanvas.gameObject.SetActive(true);
        blackOverlay.color = new Color(0f, 0f, 0f, backgroundAlpha);
        messageText.color = textColor;
        messageText.fontSize = endingFontSize;
        messageText.text = string.Empty;
    }

    private void HideOverlay()
    {
        if (overlayCanvas != null)
        {
            overlayCanvas.gameObject.SetActive(false);
        }
    }

    private void UpdateLoadingText(float elapsed)
    {
        int dotCount = Mathf.FloorToInt(elapsed / loadingDotIntervalSeconds) % 3 + 1;

        // 로딩 중에는 Loading 뒤의 마침표가 1개부터 3개까지 반복되게 합니다.
        messageText.fontSize = loadingFontSize;
        messageText.color = Color.white;
        messageText.text = "Loading" + new string('.', dotCount);
    }
}
