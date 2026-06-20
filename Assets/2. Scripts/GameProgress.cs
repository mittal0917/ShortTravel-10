using UnityEngine;

public static class GameProgress
{
    private const string SavedKey = "GameProgress.HasSave";
    private const string PlayerXKey = "GameProgress.PlayerX";
    private const string PlayerYKey = "GameProgress.PlayerY";
    private const string NewGameRequestKey = "GameProgress.NewGameRequest";

    public static void RequestContinueGame()
    {
        PlayerPrefs.SetInt(NewGameRequestKey, 0);
        PlayerPrefs.Save();
    }

    public static void RequestNewGame()
    {
        ClearSave();
        PlayerPrefs.SetInt(NewGameRequestKey, 1);
        PlayerPrefs.Save();
    }

    public static bool ConsumeNewGameRequest()
    {
        bool requested = PlayerPrefs.GetInt(NewGameRequestKey, 0) == 1;
        ClearNewGameRequest();
        return requested;
    }

    public static void ClearNewGameRequest()
    {
        PlayerPrefs.SetInt(NewGameRequestKey, 0);
        PlayerPrefs.Save();
    }

    public static bool TryLoadPlayerPosition(out Vector3 position)
    {
        if (PlayerPrefs.GetInt(SavedKey, 0) != 1)
        {
            position = Vector3.zero;
            return false;
        }

        position = new Vector3(
            PlayerPrefs.GetFloat(PlayerXKey),
            PlayerPrefs.GetFloat(PlayerYKey),
            0f);
        return true;
    }

    public static void SavePlayerPosition(Vector3 position)
    {
        PlayerPrefs.SetInt(SavedKey, 1);
        PlayerPrefs.SetFloat(PlayerXKey, position.x);
        PlayerPrefs.SetFloat(PlayerYKey, position.y);
        PlayerPrefs.Save();
    }

    public static void ClearSave()
    {
        PlayerPrefs.DeleteKey(SavedKey);
        PlayerPrefs.DeleteKey(PlayerXKey);
        PlayerPrefs.DeleteKey(PlayerYKey);
        PlayerPrefs.Save();
    }
}
