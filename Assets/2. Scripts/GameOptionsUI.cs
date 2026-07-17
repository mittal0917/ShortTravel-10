using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOptionsUI : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void CreateBinder()
    {
        if (FindObjectOfType<GameOptionsUI>() != null)
        {
            return;
        }

        GameObject binder = new GameObject("GameOptionsUI_Binder");
        DontDestroyOnLoad(binder);
        binder.AddComponent<GameOptionsUI>();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        TryBindActiveScene();
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "GameScene")
        {
            StartCoroutine(BindAfterSceneReady(scene));
        }
    }

    private void TryBindActiveScene()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        if (activeScene.name == "GameScene")
        {
            StartCoroutine(BindAfterSceneReady(activeScene));
        }
    }

    private IEnumerator BindAfterSceneReady(Scene scene)
    {
        yield return null;
        yield return null;

        BindGameSceneButtons(scene);
    }

    private static void BindGameSceneButtons(Scene scene)
    {
        GameObject settingsPanel = FindSceneObject(scene, "Panel_Settings");
        Button optionButton = FindSceneButton(scene, "Btn_InGameOption");
        Button closeButton = FindSceneButton(scene, "Btn_Close");
        Button lobbyButton = FindSceneButton(scene, "Btn_Lobby");

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
            GameSessionManager.SetGamePaused(false);
        }

        if (optionButton != null)
        {
            optionButton.onClick.RemoveAllListeners();
            optionButton.onClick.AddListener(OpenSettings);
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(CloseSettings);
        }

        if (lobbyButton != null)
        {
            lobbyButton.onClick.RemoveAllListeners();
            lobbyButton.onClick.AddListener(GoLobby);
        }
    }

    private static void OpenSettings()
    {
        GameObject settingsPanel = FindSceneObject(SceneManager.GetActiveScene(), "Panel_Settings");
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
            settingsPanel.transform.SetAsLastSibling();
            // 설정창이 떠 있는 동안 게임과 진행 시간이 같이 멈추게 합니다.
            GameSessionManager.SetGamePaused(true);
        }
    }

    private static void CloseSettings()
    {
        GameObject settingsPanel = FindSceneObject(SceneManager.GetActiveScene(), "Panel_Settings");
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
            GameSessionManager.SetGamePaused(false);
        }
    }

    private static void GoLobby()
    {
        GameSessionManager.SetGamePaused(false);
        SaveCurrentPlayerPosition();

        SceneManager.LoadScene("LobbyScene");
    }

    private static void SaveCurrentPlayerPosition()
    {
        character_move player = Object.FindObjectOfType<character_move>();
        if (player != null)
        {
            player.SaveCurrentPosition();
        }
    }

    private static Button FindSceneButton(Scene scene, string objectName)
    {
        GameObject target = FindSceneObject(scene, objectName);
        return target != null ? target.GetComponent<Button>() : null;
    }

    private static GameObject FindSceneObject(Scene scene, string objectName)
    {
        foreach (GameObject target in Resources.FindObjectsOfTypeAll<GameObject>())
        {
            if (target.name == objectName && target.scene == scene)
            {
                return target;
            }
        }

        return null;
    }
}
