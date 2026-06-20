using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_Game : MonoBehaviour
{
    [Header("Panels")]
    public GameObject settingsPanel;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsureGameUi()
    {
        if (SceneManager.GetActiveScene().name != "GameScene")
        {
            return;
        }

        GameSessionManager.EnsureExistsInScene();

        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas != null && canvas.GetComponent<UI_Game>() == null)
        {
            canvas.gameObject.AddComponent<UI_Game>();
        }

        UI_Game uiGame = canvas != null ? canvas.GetComponent<UI_Game>() : null;
        if (uiGame != null)
        {
            uiGame.ResolveSettingsPanel();
            uiGame.BindButtons();
        }
    }

    void Awake()
    {
        GameSessionManager.EnsureExistsInScene();
        ResolveSettingsPanel();
        BindButtons();
    }

    void Start()
    {
        GameSessionManager.EnsureExistsInScene();
        ResolveSettingsPanel();
        BindButtons();

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }

    void OnEnable()
    {
        ResolveSettingsPanel();
        BindButtons();
    }

    public void OnClickOption()
    {
        ResolveSettingsPanel();
        BindButtons();

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
            settingsPanel.transform.SetAsLastSibling();
            BindButtons();
        }
        else
        {
            Debug.LogWarning("Panel_Settings를 찾지 못했습니다. UI_Game의 Settings Panel 칸에 Panel_Settings를 연결해주세요.");
        }
    }

    public void OnClickClose()
    {
        ResolveSettingsPanel();

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }

    public void OnClickLobby()
    {
        character_move player = FindObjectOfType<character_move>();
        if (player != null)
        {
            GameProgress.SavePlayerPosition(player.transform.position);
        }

        SceneManager.LoadScene("LobbyScene");
    }

    private void ResolveSettingsPanel()
    {
        if (settingsPanel != null)
        {
            return;
        }

        foreach (GameObject target in Resources.FindObjectsOfTypeAll<GameObject>())
        {
            if (target.name == "Panel_Settings" && target.scene == gameObject.scene)
            {
                settingsPanel = target;
                return;
            }
        }
    }

    public void BindButtons()
    {
        BindButton("Btn_InGameOption", OnClickOption);
        BindButton("Btn_Close", OnClickClose);
        BindButton("Btn_Lobby", OnClickLobby);
    }

    private void BindButton(string buttonName, UnityEngine.Events.UnityAction action)
    {
        Button button = FindSceneButton(buttonName);
        if (button == null)
        {
            return;
        }

        button.onClick.RemoveListener(action);
        button.onClick.AddListener(action);
    }

    private Button FindSceneButton(string buttonName)
    {
        foreach (Button button in Resources.FindObjectsOfTypeAll<Button>())
        {
            if (button.name == buttonName && button.gameObject.scene.name == "GameScene")
            {
                return button;
            }
        }

        return null;
    }
}
