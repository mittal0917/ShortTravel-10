using UnityEngine;
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
        settingsPanel.SetActive(false); // 시작시 설정창 숨기기
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
}
