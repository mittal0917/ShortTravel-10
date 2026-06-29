using UnityEngine;

public class difficultPopupManager : MonoBehaviour
{
    public GameObject difficultyPopup; // 난이도 팝업창 오브젝트

    public void OnClickPlayButton()
    {
        // 팝업창을 활성화 (화면을 막고 난이도 버튼을 보여줌)
        difficultyPopup.SetActive(true);
    }

    // 난이도 선택 후 팝업창을 닫을 때 사용할 함수
    public void ClosePopup()
    {
        difficultyPopup.SetActive(false);
    }
}