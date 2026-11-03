using UnityEngine;
using UnityEngine.UI;

/// The Settings screen: music and sound volumes (for every save slot), the tutorial, and quitting to the title.
public class SettingsUI : MonoBehaviour
{
    public Text musicLabel, soundLabel, hapticsLabel;
    public Text cloudLabel, cloudActionLabel;    // cloud save (CloudSave)
    public InputField cloudField;
    string cloudMessage = "";
    bool busy, confirmRestore;
    SaveData pending;

    static GameManager GM => GameManager.Instance;

    /// Turn cloud save on (a new code, backed up at once), or back up now.
    public void CloudAction()
    {
        if (busy || GM.Save == null) return;
        if (string.IsNullOrEmpty(GM.Save.cloudCode)) GM.Save.cloudCode = CloudSave.NewCode();
        busy = true;
        cloudMessage = "Backing up...";
        Show();
        StartCoroutine(CloudSave.Upload(GM.Save, error =>
        {
            busy = false;
            cloudMessage = error ?? "Backed up. Write your code down somewhere safe: it's the key to this game.";
            GM.Persist();
            Show();
        }));
    }

    /// Bring a game from the cloud into this slot. Tap once to look it up, again to confirm.
    public void CloudRestore()
    {
        if (busy || GM.Save == null) return;
        if (confirmRestore && pending != null)
        {
            SaveSystem.Save(pending, GM.Slot);
            int slot = GM.Slot;
            pending = null;
            confirmRestore = false;
            cloudMessage = "";
            GM.PlaySlot(slot);
            return;
        }
        busy = true;
        cloudMessage = "Looking it up...";
        Show();
        StartCoroutine(CloudSave.Download(cloudField.text, (data, error) =>
        {
            busy = false;
            pending = data;
            confirmRestore = data != null;
            cloudMessage = error ?? $"Found it: day {data.day}, {data.coins} coins. Tap Restore again to replace the game in this slot with it.";
            Show();
        }));
    }

    public void ToggleHaptics() { Haptics.Enabled = !Haptics.Enabled; if (Haptics.Enabled) Haptics.Buzz(); Show(); }

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
        if (id != ScreenId.Settings) return;
        cloudMessage = ""; confirmRestore = false; pending = null;
        if (GameManager.Instance.Save != null) GameManager.Instance.Persist();   // paused and saved
        Show();
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
        if (hapticsLabel != null) hapticsLabel.text = Haptics.Enabled ? "Vibration: On" : "Vibration: Off";
        if (cloudLabel == null) return;
        var save = GM != null ? GM.Save : null;
        bool on = save != null && !string.IsNullOrEmpty(save.cloudCode);
        cloudActionLabel.text = on ? "Back Up Now" : "Turn On Cloud Save";
        cloudLabel.text = (on ? $"<b>Cloud save is on.</b> Your code: <b>{CloudSave.Pretty(save.cloudCode)}</b>" + (save.cloudSavedAt != "" ? $"  ·  last backed up {save.cloudSavedAt}" : "")
                              : "<b>Cloud save is off.</b> Turn it on to back this game up every night, and bring it to another phone with its code.")
                        + (cloudMessage != "" ? $"\n{cloudMessage}" : "");
    }

    static string Percent(float v) => v <= 0 ? "off" : $"{Mathf.RoundToInt(v * 100)}%";
}
