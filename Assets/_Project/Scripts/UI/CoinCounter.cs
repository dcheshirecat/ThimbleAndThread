using UnityEngine;
using UnityEngine.UI;

/// The coins you have, shown in every shop and at the stall so you can see what you're spending (owner, 2026-11-01).
public class CoinCounter : MonoBehaviour
{
    public Text text;
    int shown = int.MinValue;

    void Update()
    {
        var save = GameManager.Instance != null ? GameManager.Instance.Save : null;
        if (save == null || save.coins == shown) return;
        shown = save.coins;
        text.text = shown.ToString();
    }
}
