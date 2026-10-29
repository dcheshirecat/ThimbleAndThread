using System;
using UnityEngine;

/// Shows the panel for the current screen and hides the rest.
/// Build each screen as a panel under one Canvas, then list them here.
public class ScreenRouter : MonoBehaviour
{
    [Serializable]
    public class Entry { public ScreenId id; public GameObject root; }

    public Entry[] screens;

    void OnEnable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnScreenChanged += Show;
            Show(GameManager.Instance.Current);
        }
    }

    void OnDisable()
    {
        if (GameManager.Instance != null) GameManager.Instance.OnScreenChanged -= Show;
    }

    void Show(ScreenId id)
    {
        foreach (var e in screens)
            if (e.root != null) e.root.SetActive(e.id == id);
    }

    // For UnityEvents on buttons (the Inspector can't pass enums easily to every version of Button)
    public void GoToShop() => GameManager.Instance.ChangeScreen(ScreenId.Shop);
    public void GoToMarket() => GameManager.Instance.ChangeScreen(ScreenId.Market);
    public void GoToPatternStudio() => GameManager.Instance.ChangeScreen(ScreenId.PatternStudio);
    public void GoToJournal() => GameManager.Instance.ChangeScreen(ScreenId.Journal);
    public void GoToSettings() => GameManager.Instance.ChangeScreen(ScreenId.Settings);
    public void GoToWorkbench() => GameManager.Instance.ChangeScreen(ScreenId.Workbench);
}
