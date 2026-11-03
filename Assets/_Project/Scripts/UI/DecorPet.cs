using UnityEngine;
using UnityEngine.UI;

/// A pet out in the shop (ShopDayPets): every so often it wanders over to nap on one of your other pieces, with a little
/// "z z z" over it; stroking it (a tap) wakes it. Where it naps isn't saved: it goes back to its spot when the shop is
/// drawn again.
public class DecorPet : MonoBehaviour
{
    public ShopSceneUI scene;
    float nextMove;
    Vector3 target, home;
    Text zzz;
    bool napping;

    void Start()
    {
        home = target = transform.position;
        nextMove = Time.unscaledTime + Random.Range(8f, 20f);
        var go = new GameObject("Zzz", typeof(RectTransform), typeof(Text));
        go.transform.SetParent(transform, false);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = rt.anchorMax = new Vector2(0.75f, 1.05f);
        rt.sizeDelta = new Vector2(160, 80);
        zzz = go.GetComponent<Text>();
        zzz.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        zzz.fontSize = 56;
        zzz.color = new Color(0.25f, 0.25f, 0.4f, 0.8f);
        zzz.raycastTarget = false;
        zzz.text = "";
    }

    void Update()
    {
        if (scene == null || (scene.decorEditor != null && scene.decorEditor.Editing)) return;
        transform.position = Vector3.Lerp(transform.position, target, Time.unscaledDeltaTime * 2f);
        if (napping) zzz.text = Mathf.Repeat(Time.unscaledTime, 1.5f) < 0.75f ? "z z" : "z z z";
        if (Time.unscaledTime < nextMove) return;
        nextMove = Time.unscaledTime + Random.Range(25f, 50f);
        // Somewhere to nap: another piece out in the shop, or back home.
        var spots = scene.DecorShown.FindAll(p => p != null && p.gameObject != gameObject && p.gameObject.activeInHierarchy && p.GetComponent<DecorPet>() == null);
        if (spots.Count > 0 && Random.value < 0.7f)
        {
            var rt = (RectTransform)spots[Random.Range(0, spots.Count)].transform;
            target = rt.TransformPoint(new Vector3(0, rt.rect.height * 0.35f, 0));
            napping = true;
        }
        else { target = home; napping = Random.value < 0.4f; }
        if (!napping) zzz.text = "";
    }

    /// Stroked: it wakes up.
    public void Wake()
    {
        napping = false;
        if (zzz != null) zzz.text = "";
        nextMove = Time.unscaledTime + Random.Range(20f, 40f);
    }
}
