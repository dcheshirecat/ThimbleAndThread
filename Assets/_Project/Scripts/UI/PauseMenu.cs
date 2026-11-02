using UnityEngine;

/// Leaving the app pauses the game (owner, 2026-11-01): when it goes to the background, a pause menu waits for you
/// with Resume or Main Menu. Nothing moves meanwhile: the shop's clock, visitors, the stall and the minigames.
public class PauseMenu : MonoBehaviour
{
    public GameObject overlay;

    /// True while the pause menu is up. ShopDay's clock and the stall wait for it.
    public static bool Paused { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() => Paused = false;

    void OnApplicationPause(bool paused) { if (paused) Pause(); }
    void OnApplicationFocus(bool focused) { if (!focused && Application.isMobilePlatform) Pause(); }

    bool InAGame
    {
        get
        {
            var gm = GameManager.Instance;
            return gm != null && gm.Save != null && gm.Current != ScreenId.Title && gm.Current != ScreenId.SaveSelect && gm.Current != ScreenId.Tutorial;
        }
    }

    public void Pause()
    {
        if (!InAGame || Paused) return;
        GameManager.Instance.Persist();
        Paused = true;
        Time.timeScale = 0f;
        overlay.transform.SetAsLastSibling();
        overlay.SetActive(true);
    }

    public void Resume()
    {
        Paused = false;
        Time.timeScale = 1f;
        overlay.SetActive(false);
        AudioManager.Play(Sfx.Click);
    }

    public void MainMenu()
    {
        Resume();
        GameManager.Instance.QuitToTitle();
    }
}
