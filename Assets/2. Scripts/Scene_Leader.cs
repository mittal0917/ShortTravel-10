using UnityEngine;
using UnityEngine.SceneManagement;

public class Scene_Loader : MonoBehaviour
{
    public void LoadGameScene()
    {
        GameProgress.RequestContinueGame();
        // 인스펙터에 직접 연결된 버튼도 로딩 화면을 거쳐 이동하게 합니다.
        GameSceneTransition.LoadSceneWithLoading("GameScene");
    }

    public void LoadNewGameScene()
    {
        GameProgress.RequestNewGame();
        // 새 게임 버튼도 같은 로딩 화면을 사용합니다.
        GameSceneTransition.LoadSceneWithLoading("GameScene");
    }
}
