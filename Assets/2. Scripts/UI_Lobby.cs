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
        }

        BindLobbyButtons();
    }

    // Option 버튼
    public void OnClickOption()
    {
        settingsPanel.SetActive(true);
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
        SceneManager.LoadScene("GameScene");
    }

    private void OnClickNewGame()
    {
        GameProgress.RequestNewGame();
        SceneManager.LoadScene("GameScene");
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

    private RectTransform FindChildRect(Transform parent, string objectName)
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
