using UnityEngine;

public class difficultManager : MonoBehaviour
{
    //쉬움일시 currentDifficulty=0,보통일시 =1,어려움일시 =2
    public static difficultManager Instance; // 어디서든 접근 가능하게 싱글톤 설정

    // 난이도를 구분할 열거형(Enum)
    public enum Difficulty { Easy, Normal, Hard }
    public Difficulty currentDifficulty;

    // 난이도별로 바뀔 실제 게임 내 제한 변수들
    public float batDurability;
    public int maxAmmo;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        DontDestroyOnLoad(gameObject); // 씬이 넘어가도 파괴되지 않음
    }

    // 난이도를 설정하는 함수 (버튼에서 호출함)
    public void SetDifficulty(int difficultyIndex)
    {
        currentDifficulty = (Difficulty)difficultyIndex;

        // 난이도별 데이터 세팅
        switch (currentDifficulty)
        {
            case Difficulty.Easy:
                batDurability = 100f;
                maxAmmo = 99;
                break;
            case Difficulty.Normal:
                batDurability = 50f;
                maxAmmo = 30;
                break;
            case Difficulty.Hard:
                batDurability = 20f; // 내구도 약해짐
                maxAmmo = 10;        // 탄약 보유량 제한
                break;
        }

        Debug.Log($"난이도 변경 완료: {currentDifficulty}, 내구도: {batDurability}, 최대탄약: {maxAmmo}");

        // 여기에 게임 시작 씬으로 넘어가거나 팝업을 닫는 로직 추가
    }
}