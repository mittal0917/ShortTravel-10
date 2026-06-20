using UnityEngine;
using UnityEngine.SceneManagement;

public class Scene_Loader : MonoBehaviour
{
    public void LoadGameScene()
    {
        GameProgress.RequestContinueGame();
        SceneManager.LoadScene("GameScene");
    }

    public void LoadNewGameScene()
    {
        GameProgress.RequestNewGame();
        SceneManager.LoadScene("GameScene");
    }
}
