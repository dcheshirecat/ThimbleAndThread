using UnityEngine;

/// The tool in the player's hand, drawn over the work and animated with each stitch (owner, 2026-10-30:
/// the needle making each stitch, the hook making each loop). A sewing needle pokes through the fabric,
/// a crochet hook dips into the loop and pulls it through, knitting needles cross and swing as each stitch
/// slides across. Placeholder line drawings until the owner's art exists.
public class CraftTool : MonoBehaviour
{
    public enum Kind { Needle, Hook, KnittingNeedles, SeamRipper }

    const float ActionTime = 0.2f;

    Kind kind;
    float size;
    Vector3 target;
    Transform body;          // the part that moves in an action (for knitting, the right needle)
    float action = 1f;       // 0..1 through the current action; 1 = resting
    Vector3 actionOffset;    // where the action pushes the tool, at its deepest
    Material material;
    int sorting;

    /// Makes a tool, hidden until it's first moved somewhere. size: world units for one tool length.
    public static CraftTool Make(Transform parent, Kind kind, float size, Material material, int sorting)
    {
        var go = new GameObject($"Tool_{kind}");
        go.transform.SetParent(parent, false);
        var t = go.AddComponent<CraftTool>();
        t.kind = kind; t.size = size; t.material = material; t.sorting = sorting;
        t.Draw();
        go.SetActive(false);
        return t;
    }

    void Draw()
    {
        var steel = new Color(0.80f, 0.82f, 0.86f);
        switch (kind)
        {
            case Kind.Needle:
                body = Part("Needle");
                Stroke(body, steel, 0.045f, new Vector2(0, 0), new Vector2(0.16f, 0.95f));
                Stroke(body, new Color(0.35f, 0.35f, 0.40f), 0.02f, new Vector2(0.145f, 0.84f), new Vector2(0.155f, 0.9f));   // the eye
                Stroke(body, new Color(0.75f, 0.30f, 0.35f), 0.02f, new Vector2(0.15f, 0.88f), new Vector2(0.35f, 1.05f), new Vector2(0.3f, 1.25f));   // a bit of thread
                break;
            case Kind.Hook:
                body = Part("Hook");
                var lilac = new Color(0.62f, 0.50f, 0.78f);
                Stroke(body, steel, 0.06f, new Vector2(0.02f, 0.1f), new Vector2(0.12f, 0.6f));
                Stroke(body, lilac, 0.13f, new Vector2(0.12f, 0.6f), new Vector2(0.24f, 1.15f));   // the soft grip
                Stroke(body, steel, 0.05f, new Vector2(0.02f, 0.1f), new Vector2(-0.03f, 0.02f), new Vector2(-0.08f, 0.07f));   // the hook
                break;
            case Kind.SeamRipper:   // a stitch picker: a forked steel tip on a coloured handle
                body = Part("SeamRipper");
                Stroke(body, steel, 0.04f, new Vector2(0, 0.02f), new Vector2(0.08f, 0.45f));
                Stroke(body, steel, 0.03f, new Vector2(0, 0.02f), new Vector2(-0.06f, 0.1f));    // the long prong
                Stroke(body, new Color(0.95f, 0.35f, 0.40f), 0.03f, new Vector2(0.015f, 0.08f), new Vector2(0.06f, 0.08f));   // the little red ball
                Stroke(body, new Color(0.95f, 0.55f, 0.60f), 0.14f, new Vector2(0.08f, 0.45f), new Vector2(0.2f, 1.1f));   // the handle
                break;
            default:
                var wood = new Color(0.80f, 0.64f, 0.42f);
                var left = Part("LeftNeedle");
                Stroke(left, wood, 0.06f, new Vector2(-1.1f, -0.55f), new Vector2(0.25f, 0.12f));
                Stroke(left, new Color(0.55f, 0.35f, 0.25f), 0.14f, new Vector2(-1.12f, -0.56f), new Vector2(-1.05f, -0.52f));   // knob
                body = Part("RightNeedle");
                Stroke(body, wood, 0.06f, new Vector2(1.1f, -0.55f), new Vector2(-0.25f, 0.12f));
                Stroke(body, new Color(0.55f, 0.35f, 0.25f), 0.14f, new Vector2(1.12f, -0.56f), new Vector2(1.05f, -0.52f));
                break;
        }
    }

    Transform Part(string name)
    {
        var t = new GameObject(name).transform;
        t.SetParent(transform, false);
        return t;
    }

    /// A line in the tool's own units (1 = one tool length), with a soft shadow under it.
    void Stroke(Transform parent, Color color, float width, params Vector2[] points)
    {
        foreach (var (c, w, offset, order) in new[] { (new Color(0, 0, 0, 0.25f), width * 1.4f, new Vector2(0.03f, -0.03f), 0), (color, width, Vector2.zero, 1) })
        {
            var go = new GameObject("Stroke");
            go.transform.SetParent(parent, false);
            var l = go.AddComponent<LineRenderer>();
            l.sharedMaterial = material;
            l.useWorldSpace = false;
            l.startColor = l.endColor = c;
            l.startWidth = l.endWidth = w * size;
            l.numCapVertices = 4;
            l.sortingOrder = sorting + order;
            l.positionCount = points.Length;
            for (int i = 0; i < points.Length; i++) l.SetPosition(i, (Vector3)((points[i] + offset) * size));
        }
    }

    /// Moves the tool to a world point (gliding there, or at once).
    public void MoveTo(Vector3 world, bool instant = false)
    {
        world.z = 0;
        target = world;
        if (instant || !gameObject.activeSelf) transform.position = world;
        gameObject.SetActive(true);
    }

    public void Hide() => gameObject.SetActive(false);

    /// One stitch: the needle (or hook) pushes down through the fabric and comes back up; knitting
    /// needles swing the new stitch across.
    public void Poke()
    {
        actionOffset = kind == Kind.KnittingNeedles ? new Vector3(-0.15f, 0.1f, 0) * size : new Vector3(-0.08f, -0.35f, 0) * size;
        action = 0f;
    }

    /// The hook pulls the loop through, along the flick.
    public void Pull(Vector2 direction)
    {
        actionOffset = (Vector3)(direction.normalized * 0.45f * size);
        action = 0f;
    }

    void Update()
    {
        float dt = Mathf.Max(Time.deltaTime, 1f / 60f);
        transform.position = Vector3.Lerp(transform.position, target, 1f - Mathf.Exp(-25f * dt));
        if (body == null) return;
        if (action < 1f) action = Mathf.Min(1f, action + dt / ActionTime);
        float a = Mathf.Sin(action * Mathf.PI);   // in and back out
        body.localPosition = actionOffset * a;
        if (kind == Kind.KnittingNeedles) body.localRotation = Quaternion.Euler(0, 0, -14f * a);
    }
}
