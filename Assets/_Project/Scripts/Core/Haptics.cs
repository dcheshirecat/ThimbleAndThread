using UnityEngine;

/// A little buzz on the phone (e.g. the doorbell), so the player notices with the sound off.
/// It can be turned off in Settings (PlayerPrefs "haptics").
public static class Haptics
{
    const string Key = "haptics";

    public static bool Enabled
    {
        get => PlayerPrefs.GetInt(Key, 1) == 1;
        set { PlayerPrefs.SetInt(Key, value ? 1 : 0); PlayerPrefs.Save(); }
    }

    public static void Buzz()
    {
        if (!Enabled) return;
#if UNITY_ANDROID || UNITY_IOS
        Handheld.Vibrate();
#endif
    }
}
