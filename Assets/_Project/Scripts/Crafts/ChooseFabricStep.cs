using System.Collections.Generic;
using UnityEngine;

/// Recipe step: pick which of your sewing fabrics to make the piece from. Only fabrics you have
/// enough of are offered. Not judged; the choice sets the piece's colour and material tier.
public class ChooseFabricStep : PieceStep
{
    const int Columns = 3;
    const float Swatch = 0.2f;

    readonly List<SupplyDef> options = new List<SupplyDef>();
    readonly List<Vector2> spots = new List<Vector2>();
    SpriteRenderer highlight;
    MeshRenderer preview;

    /// Sewing fabrics with enough pieces for this order.
    public static List<SupplyDef> Options(Order order, ContentLibrary content, ShopManager shop)
    {
        int pieces = order.FabricPieces();
        return new List<SupplyDef>(System.Array.FindAll(content.supplies,
            s => s != null && s.kind == SupplyKind.Fabric && s.craft == CraftType.Sewing && shop.Stock(s.supplyId) >= pieces));
    }

    protected override void Build()
    {
        options.AddRange(Options(order, area.content, area.shop));
        preview = Piece(Shape.Outline, new Color(1, 1, 1, 0.25f), 0);
        preview.transform.localScale *= 0.38f;                       // the piece, small, above the swatches
        preview.transform.position = World(new Vector2(0.31f, 0.6f));
        highlight = Square(Vector2.zero, Swatch + 0.03f, Color.white, 1);
        highlight.enabled = false;

        for (int i = 0; i < options.Count; i++)
        {
            var spot = new Vector2(0.2f + (i % Columns) * 0.3f, 0.44f - (i / Columns) * 0.29f);
            spots.Add(spot);
            Square(spot, Swatch, options[i].color, 2);
            Label(spot + Vector2.down * 0.13f, options[i].displayName, 0.035f, Color.white, 2);
        }
        int chosen = options.FindIndex(s => s.supplyId == order.fabricId);
        if (chosen >= 0) Choose(chosen);
    }

    protected override void OnPress(Vector2 p)
    {
        for (int i = 0; i < spots.Count; i++)
            if (Mathf.Abs(p.x - spots[i].x) <= Swatch / 2 && Mathf.Abs(p.y - spots[i].y) <= Swatch / 2) { Choose(i); return; }
    }

    void Choose(int i)
    {
        order.fabricId = options[i].supplyId;
        highlight.enabled = true;
        highlight.transform.position = World(spots[i]);
        Recolor(preview, options[i].color);
    }

    bool Chosen => options.Exists(s => s.supplyId == order.fabricId);

    public override float SaveWork()
    {
        State.progress = Chosen ? 1f : 0f;
        return Chosen ? 1f : 0f;
    }

    public override string FinishBlocker => Chosen ? null : "Tap a fabric to choose it first.";
}
