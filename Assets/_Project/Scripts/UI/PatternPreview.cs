using UnityEngine;

/// Draws a pattern as a small pixel image: one pixel per square, on cream "fabric".
/// Whoever shows it should Destroy the texture when it's replaced.
public static class PatternPreview
{
    public static Texture2D Make(Pattern pattern)
    {
        int w = Mathf.Max(1, pattern.width), h = Mathf.Max(1, pattern.height);
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp };
        var fabric = new Color(0.96f, 0.93f, 0.86f);
        var pixels = new Color[w * h];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = fabric;
        foreach (var c in pattern.cells)
            if (c.x >= 0 && c.x < w && c.y >= 0 && c.y < h && c.colorIndex < pattern.palette.Count)
                pixels[c.y * w + c.x] = pattern.palette[c.colorIndex];
        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
    }
}
