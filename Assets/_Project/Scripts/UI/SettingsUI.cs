using UnityEngine;
using UnityEngine.UI;

/// The Settings screen: music and sound volumes (for every save slot), the tutorial, and quitting to the title.
public class SettingsUI : MonoBehaviour
{
    public Text musicLabel, soundLabel;

    void OnEnable()
    {
        if (GameManager.Instance != null) GameManager.Instance.OnScreenChanged += OnScreenChanged;
    }

    void OnDisable()
    {
        if (GameManager.Instance != null) GameManager.Instance.OnScreenChanged -= OnScreenChanged;
    }

    void OnScreenChanged(ScreenId id)
    {
        if (id == ScreenId.Settings) Show();
    }

    public void MusicUp() => Change(true, 1);
    public void MusicDown() => Change(true, -1);
    public void SoundUp() => Change(false, 1);
    public void SoundDown() => Change(false, -1);

    void Change(bool isMusic, int direction)
    {
        var audio = AudioManager.Instance;
        if (audio == null) return;
        if (isMusic) audio.ChangeMusic(direction); else audio.ChangeSound(direction);
        Show();
    }

    void Show()
    {
        musicLabel.text = $"Music {Percent(AudioManager.MusicVolume)}";
        soundLabel.text = $"Sounds {Percent(AudioManager.SoundVolume)}";
    }

    static string Percent(float v) => v <= 0 ? "off" : $"{Mathf.RoundToInt(v * 100)}%";
}
