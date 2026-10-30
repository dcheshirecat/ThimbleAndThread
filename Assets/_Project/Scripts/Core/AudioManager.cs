using UnityEngine;

public enum Sfx { Click, Coin, Stitch, StepDone, Bell }

/// Music and sound effects, with the volumes from the Settings screen (kept in PlayerPrefs, so they're
/// the same for every save slot). Music follows the time of day: the day tune in the shop and on the
/// title, the night tune in the workroom. Clips come from Assets/_Project/Audio (see Docs/ADDING_ART.md);
/// until the real sounds exist, each missing effect is a soft generated placeholder and missing music is silence.
[DefaultExecutionOrder(-90)]
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Music (loops)")] public AudioClip musicDay, musicNight;
    [Header("Sound effects")] public AudioClip click, coin, stitch, stepDone, bell;

    public const string MusicKey = "music_volume", SoundKey = "sound_volume";
    const float Step = 0.1f;

    AudioSource music, sound;
    int lastCoins = -1;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() => Instance = null;

    public static float MusicVolume => PlayerPrefs.GetFloat(MusicKey, 0.6f);
    public static float SoundVolume => PlayerPrefs.GetFloat(SoundKey, 0.8f);

    /// Plays an effect if there is an AudioManager (none in some tests).
    public static void Play(Sfx s)
    {
        if (Instance != null) Instance.PlaySound(s);
    }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
        music = gameObject.AddComponent<AudioSource>();
        music.loop = true;
        music.playOnAwake = false;
        sound = gameObject.AddComponent<AudioSource>();
        sound.playOnAwake = false;
        if (click == null) click = Placeholder("click", 0.05f, (t, i) => Tone(t, 1400, 60));
        if (coin == null) coin = Placeholder("coin", 0.35f, (t, i) => t < 0.08f ? Tone(t, 988, 18) : Tone(t - 0.08f, 1319, 9));
        if (stitch == null) stitch = Placeholder("stitch", 0.07f, (t, i) => Noise(i) * Mathf.Exp(-t * 70) * 0.5f + Tone(t, 520, 50) * 0.4f);
        if (stepDone == null) stepDone = Placeholder("stepDone", 0.6f, (t, i) => Tone(t, 523, 6) + (t > 0.12f ? Tone(t - 0.12f, 659, 6) : 0) + (t > 0.24f ? Tone(t - 0.24f, 784, 5) : 0));
        if (bell == null) bell = Placeholder("bell", 0.9f, (t, i) => Tone(t, 1568, 5) * 0.6f + Tone(t, 2349, 7) * 0.3f);
    }

    void Start()
    {
        var gm = GameManager.Instance;
        if (gm == null) return;
        gm.OnScreenChanged += OnScreenChanged;
        gm.OnCoinsChanged += OnCoinsChanged;
        gm.OnGameLoaded += _ => lastCoins = gm.Save.coins;
        ApplyVolumes();
        OnScreenChanged(gm.Current);
    }

    void OnDestroy()
    {
        var gm = GameManager.Instance;
        if (gm == null || Instance != this) return;
        gm.OnScreenChanged -= OnScreenChanged;
        gm.OnCoinsChanged -= OnCoinsChanged;
    }

    void OnScreenChanged(ScreenId id)
    {
        if (id == ScreenId.Result) PlaySound(Sfx.StepDone);
        var save = GameManager.Instance.Save;
        PlayMusic(save != null && save.night && id != ScreenId.Title && id != ScreenId.SaveSelect ? musicNight : musicDay);
    }

    void OnCoinsChanged(int coins)
    {
        if (lastCoins >= 0 && coins > lastCoins) PlaySound(Sfx.Coin);
        lastCoins = coins;
    }

    void PlayMusic(AudioClip clip)
    {
        if (music.clip == clip && (clip == null || music.isPlaying)) return;
        music.clip = clip;
        if (clip != null) music.Play(); else music.Stop();
    }

    public void PlaySound(Sfx s)
    {
        var clip = s switch { Sfx.Click => click, Sfx.Coin => coin, Sfx.Stitch => stitch, Sfx.StepDone => stepDone, _ => bell };
        if (clip != null && SoundVolume > 0) sound.PlayOneShot(clip);
    }

    // ---- Volumes (Settings screen) ----

    public void ChangeMusic(int direction) => SetVolume(MusicKey, MusicVolume + direction * Step);
    public void ChangeSound(int direction)
    {
        SetVolume(SoundKey, SoundVolume + direction * Step);
        PlaySound(Sfx.Coin);   // a sample at the new volume
    }

    void SetVolume(string key, float v)
    {
        PlayerPrefs.SetFloat(key, Mathf.Round(Mathf.Clamp01(v) * 10) / 10f);
        PlayerPrefs.Save();
        ApplyVolumes();
    }

    void ApplyVolumes()
    {
        music.volume = MusicVolume * 0.5f;   // music sits under the effects
        sound.volume = SoundVolume;
    }

    // ---- Placeholder sounds (generated, no files) ----

    const int Rate = 44100;

    static AudioClip Placeholder(string name, float seconds, System.Func<float, int, float> wave)
    {
        int n = Mathf.CeilToInt(seconds * Rate);
        var data = new float[n];
        for (int i = 0; i < n; i++)
        {
            float t = i / (float)Rate;
            float fade = Mathf.Clamp01((seconds - t) / 0.02f);   // no click at the end
            data[i] = Mathf.Clamp(wave(t, i) * 0.35f * fade, -1, 1);
        }
        var clip = AudioClip.Create("placeholder_" + name, n, 1, Rate, false);
        clip.SetData(data, 0);
        return clip;
    }

    /// A sine note that fades out (decay = how fast), with a 3 ms fade in.
    static float Tone(float t, float hz, float decay) =>
        t < 0 ? 0 : Mathf.Sin(2 * Mathf.PI * hz * t) * Mathf.Exp(-t * decay) * Mathf.Clamp01(t / 0.003f);

    static float Noise(int i)
    {
        uint h = (uint)i * 2654435761u;
        h ^= h >> 13; h *= 1274126177u;
        return (h & 0xFFFF) / 32768f - 1f;
    }
}
