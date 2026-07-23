using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_Lobby : MonoBehaviour
{
    [Header("Panels")]
    public GameObject settingsPanel;

    [Header("Sliders")]
    public Slider sliderSFX;
    public Slider sliderBGM;

    [Header("Toggle")]
    public Toggle toggleMute;

    void Start()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false); // 시작시 설정창 숨기기
            ResizeSettingsControls();
            ApplyGuideToSettingsPanel(settingsPanel);
        }

        SetupLobbyTitleAndButtons();
        BindLobbyButtons();
    }

    // Option 버튼
    public void OnClickOption()
    {
        ApplyGuideToSettingsPanel(settingsPanel);
        settingsPanel.SetActive(true);
        settingsPanel.transform.SetAsLastSibling();
    }

    // X 버튼
    public void OnClickClose()
    {
        settingsPanel.SetActive(false);
    }

    // 효과음 슬라이더
    public void OnChangeSFX(float value)
    {
        // 나중에 사운드 연결
    }

    // 배경음 슬라이더
    public void OnChangeBGM(float value)
    {
        // 나중에 사운드 연결
    }

    // 음소거 토글
    public void OnChangeMute(bool isMute)
    {
        // 나중에 사운드 연결
    }

    private void BindLobbyButtons()
    {
        Button playButton = FindButton("Btn_Play");
        if (playButton != null)
        {
            playButton.onClick.RemoveAllListeners();
            playButton.onClick.AddListener(OnClickPlay);
        }

        Button newGameButton = FindButton("Btn_NewGame");
        if (newGameButton != null)
        {
            newGameButton.onClick.RemoveAllListeners();
            newGameButton.onClick.AddListener(OnClickNewGame);
        }
    }

    private void SetupLobbyTitleAndButtons()
    {
        Button playButton = FindButton("Btn_Play");
        if (playButton != null)
        {
            // 기존 Play 기능 코드는 남겨두고, 화면에서는 버튼 오브젝트만 비활성화합니다.
            playButton.gameObject.SetActive(false);
        }

        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null || canvas.transform.Find("Text_GameTitle") != null)
        {
            return;
        }

        GameObject titleObject = new GameObject("Text_GameTitle");
        titleObject.transform.SetParent(canvas.transform, false);

        Text titleText = titleObject.AddComponent<Text>();
        titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        titleText.text = "Hazard Outbreak";
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.color = new Color(0.92f, 0.05f, 0.08f, 1f);
        titleText.fontSize = 96;
        titleText.horizontalOverflow = HorizontalWrapMode.Overflow;
        titleText.verticalOverflow = VerticalWrapMode.Overflow;

        RectTransform titleRect = titleText.rectTransform;
        titleRect.anchorMin = new Vector2(0f, 1f);
        titleRect.anchorMax = new Vector2(1f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0f, -80f);
        titleRect.sizeDelta = new Vector2(0f, 180f);
        titleObject.transform.SetAsLastSibling();
    }

    private Button FindButton(string objectName)
    {
        Transform target = transform.root.Find(objectName);
        if (target == null)
        {
            GameObject foundObject = GameObject.Find(objectName);
            target = foundObject != null ? foundObject.transform : null;
        }

        return target != null ? target.GetComponent<Button>() : null;
    }

    private void OnClickPlay()
    {
        GameProgress.RequestContinueGame();
        // 로비에서 게임으로 이동할 때 검은 로딩 화면을 먼저 보여줍니다.
        GameSceneTransition.LoadSceneWithLoading("GameScene");
    }

    private void OnClickNewGame()
    {
        GameProgress.RequestNewGame();
        // 새 게임 시작도 같은 로딩 연출을 거쳐 게임씬으로 들어갑니다.
        GameSceneTransition.LoadSceneWithLoading("GameScene");
    }

    private void ResizeSettingsControls()
    {
        RectTransform window = FindChildRect(settingsPanel.transform, "Panel_SettingWindow");
        if (window != null)
        {
            window.anchorMin = new Vector2(0.5f, 0.5f);
            window.anchorMax = new Vector2(0.5f, 0.5f);
            window.sizeDelta = new Vector2(1000f, 1000f);
            window.anchoredPosition = Vector2.zero;
        }

        ResizeChild("Btn_Close", new Vector2(80f, 80f), new Vector2(420f, 420f));
        ResizeChild("Slider_SFX", new Vector2(520f, 44f), new Vector2(160f, 180f));
        ResizeChild("Slider_BGM", new Vector2(520f, 44f), new Vector2(160f, 40f));
        ResizeChild("Toggle_Mute", new Vector2(520f, 64f), new Vector2(160f, -110f));
        ResizeSliderParts("Slider_SFX");
        ResizeSliderParts("Slider_BGM");
    }

    public static void ApplyGuideToSettingsPanel(GameObject targetSettingsPanel)
    {
        if (targetSettingsPanel == null)
        {
            return;
        }

        HideSoundControl(targetSettingsPanel, "Slider_SFX");
        HideSoundControl(targetSettingsPanel, "Slider_BGM");
        HideSoundControl(targetSettingsPanel, "Toggle_Mute");
        HideSoundLabel(targetSettingsPanel, "배경");
        HideSoundLabel(targetSettingsPanel, "음소");
        HideSoundLabel(targetSettingsPanel, "효과");

        RectTransform window = FindChildRect(targetSettingsPanel.transform, "Panel_SettingWindow");
        if (window == null)
        {
            return;
        }

        // 로비와 인게임에서 같은 설명창 크기를 쓰도록 배경 패널을 충분히 크게 맞춥니다.
        window.anchorMin = new Vector2(0.5f, 0.5f);
        window.anchorMax = new Vector2(0.5f, 0.5f);
        window.sizeDelta = new Vector2(1000f, 1000f);
        window.anchoredPosition = Vector2.zero;

        MoveLobbyButtonBelowGuide(targetSettingsPanel);

        RectTransform guideRect = FindChildRect(window, "Text_GameGuide");
        Text guideText;
        if (guideRect == null)
        {
            GameObject guideObject = new GameObject("Text_GameGuide");
            guideObject.transform.SetParent(window, false);
            guideText = guideObject.AddComponent<Text>();
            guideRect = guideText.rectTransform;
        }
        else
        {
            guideText = guideRect.GetComponent<Text>();
        }

        if (guideText == null)
        {
            return;
        }

        guideText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        guideText.text =
            "게임 설명\n\n"
            + "1. WASD와 방향키로 캐릭터를 움직일 수 있습니다.\n\n"
            + "2. F키를 누르면 바닥에 떨어져있는 권총을 획득할 수 있습니다.\n\n"
            + "3. 탈출을 위한 물자를 챙기면 권총의 총알을 획득할 수 있습니다. 하지만 조심하세요. 물자를 획득할 수록 좀비는 더욱 강해지고 빨라질 겁니다!\n\n"
            + "4. 탈출을 위한 물자를 모은다면 맵 가장자리에 탈출구가 생성됩니다.\n\n"
            + "5. 탈출을 위해선 8초간 탈출구 안에 몸을 숨겨야합니다. 탈출구가 좀비로부터 버틸 수 있는 시간은 5초뿐이니 주변의 좀비를 최대한 처리하고 탈출하세요.";
        guideText.alignment = TextAnchor.UpperLeft;
        guideText.color = Color.white;
        guideText.fontSize = 30;
        guideText.horizontalOverflow = HorizontalWrapMode.Wrap;
        guideText.verticalOverflow = VerticalWrapMode.Overflow;
        guideText.lineSpacing = 0.95f;
        guideText.raycastTarget = false;

        guideRect.anchorMin = new Vector2(0.5f, 0.5f);
        guideRect.anchorMax = new Vector2(0.5f, 0.5f);
        guideRect.pivot = new Vector2(0.5f, 0.5f);
        guideRect.anchoredPosition = new Vector2(0f, 20f);
        guideRect.sizeDelta = new Vector2(820f, 660f);
    }

    private static void MoveLobbyButtonBelowGuide(GameObject targetSettingsPanel)
    {
        RectTransform lobbyButtonRect = FindChildRect(targetSettingsPanel.transform, "Btn_Lobby");
        if (lobbyButtonRect == null)
        {
            return;
        }

        // 인게임 설정창의 로비 버튼은 설명 글자와 겹치지 않게 패널 아래쪽으로 내립니다.
        lobbyButtonRect.anchorMin = new Vector2(0.5f, 0.5f);
        lobbyButtonRect.anchorMax = new Vector2(0.5f, 0.5f);
        lobbyButtonRect.pivot = new Vector2(0.5f, 0.5f);
        lobbyButtonRect.anchoredPosition = new Vector2(0f, -410f);
        lobbyButtonRect.sizeDelta = new Vector2(360f, 72f);
        lobbyButtonRect.SetAsLastSibling();
    }

    private static void HideSoundControl(GameObject targetSettingsPanel, string objectName)
    {
        RectTransform rect = FindChildRect(targetSettingsPanel.transform, objectName);
        if (rect != null)
        {
            // 사운드를 사용하지 않기로 했으므로 관련 설정 UI를 화면에서 제거합니다.
            rect.gameObject.SetActive(false);
        }
    }

    private static void HideSoundLabel(GameObject targetSettingsPanel, string labelText)
    {
        foreach (Text text in targetSettingsPanel.GetComponentsInChildren<Text>(true))
        {
            if (text.text == labelText)
            {
                text.gameObject.SetActive(false);
            }
        }
    }

    private void ResizeChild(string objectName, Vector2 size, Vector2 position)
    {
        RectTransform rect = FindChildRect(settingsPanel.transform, objectName);
        if (rect == null)
        {
            return;
        }

        rect.sizeDelta = size;
        rect.anchoredPosition = position;
    }

    private static RectTransform FindChildRect(Transform parent, string objectName)
    {
        foreach (Transform child in parent.GetComponentsInChildren<Transform>(true))
        {
            if (child.name == objectName)
            {
                return child.GetComponent<RectTransform>();
            }
        }

        return null;
    }

    private void ResizeSliderParts(string sliderName)
    {
        RectTransform sliderRect = FindChildRect(settingsPanel.transform, sliderName);
        if (sliderRect == null)
        {
            return;
        }

        RectTransform handleRect = FindNamedChildRect(sliderRect, "Handle");
        if (handleRect != null)
        {
            handleRect.sizeDelta = new Vector2(44f, 44f);
        }
    }

    private RectTransform FindNamedChildRect(Transform parent, string objectName)
    {
        foreach (Transform child in parent.GetComponentsInChildren<Transform>(true))
        {
            if (child != parent && child.name == objectName)
            {
                return child.GetComponent<RectTransform>();
            }
        }

        return null;
    }
}
