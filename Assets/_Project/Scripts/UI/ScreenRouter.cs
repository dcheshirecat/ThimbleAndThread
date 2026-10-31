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
    public void GoToTitle() => GameManager.Instance.ChangeScreen(ScreenId.Title);
    public void GoToShop() => GameManager.Instance.ChangeScreen(ScreenId.Shop);
    public void GoToDialogue() => GameManager.Instance.ChangeScreen(ScreenId.Dialogue);
    public void GoToMinigame() => GameManager.Instance.ChangeScreen(ScreenId.Minigame);
    public void GoToResult() => GameManager.Instance.ChangeScreen(ScreenId.Result);
    public void GoToDaySummary() => GameManager.Instance.ChangeScreen(ScreenId.DaySummary);
    public void GoToSaveSelect() => GameManager.Instance.ChangeScreen(ScreenId.SaveSelect);
    public void GoToOrderBook() => GameManager.Instance.ChangeScreen(ScreenId.OrderBook);
    public void GoToHub() => GameManager.Instance.ChangeScreen(GameManager.Instance.HubScreen);
    public void GoToMarket() => GameManager.Instance.ChangeScreen(ScreenId.Market);
    public void GoToPatternStudio() => GameManager.Instance.ChangeScreen(ScreenId.PatternStudio);
    public void GoToJournal() => GameManager.Instance.ChangeScreen(ScreenId.Journal);
    public void GoToSettings() => GameManager.Instance.ChangeScreen(ScreenId.Settings);
    public void GoToTutorial() => GameManager.Instance.ChangeScreen(ScreenId.Tutorial);
    public void GoToBoutique() => GameManager.Instance.ChangeScreen(ScreenId.Boutique);
    public void GoToCustomize() => GameManager.Instance.ChangeScreen(ScreenId.Customize);
    public void GoToTrade() => GameManager.Instance.ChangeScreen(ScreenId.Trade);
    public void GoToWorkbench() => GameManager.Instance.ChangeScreen(ScreenId.Workbench);
}
