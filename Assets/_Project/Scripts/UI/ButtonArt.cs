using UnityEngine;
using UnityEngine.UI;

/// Hand-lettered button art. Each button looks for drawings named after its label in Art/UI/Buttons:
/// "Done for Now" -> btn_done_for_now.png, btn_done_for_now_pressed.png and (optional) btn_done_for_now_disabled.png.
/// When they exist the drawing replaces the plain button and its typed label; when they don't, nothing changes.
/// The label is checked again whenever the game changes it, so buttons whose words change find their art too.
[RequireComponent(typeof(Button))]
public class ButtonArt : MonoBehaviour
{
    public ContentLibrary content;

    Button button;
    Image image;
    Text label;
    Sprite plainSprite;
    Image.Type plainType;
    Color plainColor;
    SpriteState plainState;
    Selectable.Transition plainTransition;
    string shownFor;
    bool drawn, hasDisabled;

    void Awake()
    {
        button = GetComponent<Button>();
        image = button.targetGraphic as Image;
        label = GetComponentInChildren<Text>(true);
        if (image == null) { enabled = false; return; }
        plainSprite = image.sprite;
        plainType = image.type;
        plainColor = image.color;
        plainState = button.spriteState;
        plainTransition = button.transition;
    }

    void LateUpdate()
    {
        string text = label != null ? label.text : null;
        if (text != shownFor) { shownFor = text; Apply(text); }
        if (drawn && !hasDisabled)   // no greyed-out drawing: fade the normal one instead
            image.color = button.IsInteractable() ? Color.white : new Color(1, 1, 1, 0.45f);
    }

    void Apply(string text)
    {
        string key = Key(text);
        Sprite normal = Find(key), pressed = normal != null ? Find(key + "_pressed") : null;
        Sprite disabled = normal != null ? Find(key + "_disabled") : null;
        drawn = normal != null;
        if (drawn)
        {
            image.sprite = normal;
            image.type = Image.Type.Simple;
            image.preserveAspect = true;
            image.color = Color.white;
            button.transition = Selectable.Transition.SpriteSwap;
            button.spriteState = new SpriteState { pressedSprite = pressed ?? normal, highlightedSprite = normal,
                                                   selectedSprite = normal, disabledSprite = disabled ?? normal };
            hasDisabled = disabled != null;
        }
        else
        {
            image.sprite = plainSprite;
            image.type = plainType;
            image.preserveAspect = false;
            image.color = plainColor;
            button.transition = plainTransition;
            button.spriteState = plainState;
        }
        if (label != null) label.enabled = !drawn;
    }

    Sprite Find(string name)
    {
        if (content == null || content.buttonArt == null || string.IsNullOrEmpty(name)) return null;
        foreach (var s in content.buttonArt)
            if (s != null && s.name == name) return s;
        return null;
    }

    /// The file name (without .png) for a label: "Done for Now" -> "btn_done_for_now". Rich-text tags and
    /// punctuation are dropped; "&" becomes "and", "+" "plus" and a lone "-" "minus" ("Price -" -> "btn_price_minus").
    public static string Key(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        text = System.Text.RegularExpressions.Regex.Replace(text, "<[^>]*>", "");
        text = System.Text.RegularExpressions.Regex.Replace(text, @"(?<!\w)-(?!\w)", " minus ").Replace("+", " plus ");
        var sb = new System.Text.StringBuilder("btn_");
        bool gap = false;
        foreach (char c in text.Replace("&", " and ").ToLowerInvariant())
        {
            if (c >= 'a' && c <= 'z' || c >= '0' && c <= '9') { if (gap && sb.Length > 4) sb.Append('_'); sb.Append(c); gap = false; }
            else if (c == '\'' || c == '’') continue;
            else gap = true;
        }
        return sb.Length > 4 ? sb.ToString() : null;
    }
}
