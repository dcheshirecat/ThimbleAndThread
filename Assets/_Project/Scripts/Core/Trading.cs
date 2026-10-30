using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// Trading with friends by share code: no accounts, servers or networking. A code is a short piece of
/// text the player copies and sends (by message, email...); the friend pastes it into their game.
/// - Supplies: a gift parcel. The supplies leave the sender's cupboard when the code is made, so nothing
///   is duplicated. Each save opens a given code only once, and never its own codes.
/// - Patterns: a copy of a designed pattern. The sender keeps theirs; the friend gets it in their book.
/// Format: "TT1." + base64url(JSON packet) + "." + checksum (catches codes that weren't copied in full).
/// Packets hold only permanent ids (supplyId, itemId), so codes keep working across updates.
public class Trading
{
    public const string Prefix = "TT1.";
    public const int PacketVersion = 1;
    const int MaxSentCodes = 20;          // "My codes" keeps the most recent ones
    const int MaxAmount = 9999;
    const int MaxPatternSize = 64;

    [Serializable]
    class Packet
    {
        public int v = PacketVersion;
        public string id;                 // random, so each save can open a code once
        public string kind;               // "supplies" or "pattern"
        public List<SupplyStock> supplies = new List<SupplyStock>();
        public SharedPattern pattern = new SharedPattern();
    }

    /// A pattern in a compact form: the grid is one character per square, run-length encoded.
    [Serializable]
    class SharedPattern
    {
        public string name, itemId;
        public int price, craft, width, height;
        public List<string> palette = new List<string>();   // hex colours
        public List<string> threads = new List<string>();   // supplyId per palette colour
        public string grid;
    }

    readonly ContentLibrary content;
    readonly ShopManager shop;
    readonly Progression progress;

    SaveData Save => GameManager.Instance.Save;

    public Trading(ContentLibrary content, ShopManager shop, Progression progress)
    {
        this.content = content;
        this.shop = shop;
        this.progress = progress;
    }

    // ---- What can be given ----

    /// Supplies that can go in a gift parcel: anything bought, but not books (they're read, not kept)
    /// or the shop's own tools that can never be bought again (the great-aunt's sewing machine).
    public bool CanGive(SupplyDef s) => s != null && s.kind != SupplyKind.Book && s.market != MarketSlot.NotSold;

    // ---- Making codes ----

    /// Takes the parcel out of the cupboard and returns its gift code (null if something isn't owned).
    public string MakeSupplyGift(List<SupplyStock> parcel, out string error)
    {
        error = null;
        var items = new List<SupplyStock>();
        foreach (var p in parcel)
        {
            if (p.amount <= 0) continue;
            var s = content.Supply(p.supplyId);
            if (!CanGive(s) || shop.Stock(p.supplyId) < p.amount) { error = "You don't have all of that any more."; return null; }
            items.Add(new SupplyStock { supplyId = p.supplyId, amount = p.amount });
        }
        if (items.Count == 0) { error = "The parcel is empty. Tap supplies to add them."; return null; }

        var packet = new Packet { id = NewId(), kind = "supplies", supplies = items };
        string code = Encode(packet);
        var needs = items.ConvertAll(x => new MaterialNeed { supplyId = x.supplyId, amount = x.amount });
        shop.Use(needs);
        Remember(packet.id, code, $"Gift: {Describe(items)}");
        progress.AddNews($"You made a gift of {Describe(items)}.");
        GameManager.Instance.Persist();
        return code;
    }

    /// A code for a copy of a designed pattern. The player keeps their own.
    public string SharePattern(OwnedPattern p)
    {
        var d = p.design;
        var shared = new SharedPattern
        {
            name = p.name, itemId = p.itemId, price = p.price, craft = (int)d.craft,
            width = d.width, height = d.height, threads = new List<string>(d.threads), grid = Grid(d),
        };
        foreach (var c in d.palette) shared.palette.Add(ColorUtility.ToHtmlStringRGB(c));
        var packet = new Packet { id = NewId(), kind = "pattern", pattern = shared };
        string code = Encode(packet);
        Remember(packet.id, code, $"Pattern: {p.name}");
        GameManager.Instance.Persist();
        return code;
    }

    void Remember(string id, string code, string summary)
    {
        Save.sentCodes.Insert(0, new SentCode { id = id, code = code, day = Save.day, summary = summary });
        if (Save.sentCodes.Count > MaxSentCodes) Save.sentCodes.RemoveRange(MaxSentCodes, Save.sentCodes.Count - MaxSentCodes);
    }

    // ---- Opening codes ----

    /// Opens a friend's code. Returns what was received, or null with a reason in error.
    public string Redeem(string code, out string error)
    {
        var packet = Decode(code, out error);
        if (packet == null) return null;
        if (Save.receivedCodes.Contains(packet.id)) { error = "You've already opened this one."; return null; }
        if (Save.sentCodes.Exists(x => x.id == packet.id)) { error = "That's one of your own codes. Send it to a friend!"; return null; }

        string got;
        if (packet.kind == "supplies") got = ReceiveSupplies(packet, out error);
        else if (packet.kind == "pattern") got = ReceivePattern(packet.pattern, out error);
        else { error = "This code is from a newer version of the game. Update to open it."; return null; }
        if (got == null) return null;

        Save.receivedCodes.Add(packet.id);
        progress.AddNews($"A friend sent you {got}.");
        GameManager.Instance.Persist();
        return got;
    }

    string ReceiveSupplies(Packet packet, out string error)
    {
        error = null;
        if (packet.supplies == null || packet.supplies.Count == 0) { error = "That parcel is empty."; return null; }
        foreach (var x in packet.supplies)
        {
            var s = content.Supply(x.supplyId);
            if (s == null) { error = "This gift has supplies from a newer version of the game. Update to open it."; return null; }
            if (!CanGive(s) || x.amount <= 0 || x.amount > MaxAmount) { error = "That code doesn't look right."; return null; }
        }
        foreach (var x in packet.supplies) shop.Add(x.supplyId, x.amount);
        return Describe(packet.supplies);
    }

    string ReceivePattern(SharedPattern sp, out string error)
    {
        error = null;
        if (sp == null || content.Item(sp.itemId) == null || !Enum.IsDefined(typeof(CraftType), sp.craft))
        { error = "This pattern is for something from a newer version of the game. Update to open it."; return null; }
        if (sp.width < 1 || sp.height < 1 || sp.width > MaxPatternSize || sp.height > MaxPatternSize || sp.palette.Count > 64)
        { error = "That code doesn't look right."; return null; }
        foreach (var t in sp.threads)
            if (!string.IsNullOrEmpty(t) && content.Supply(t) == null)
            { error = "This pattern uses threads from a newer version of the game. Update to open it."; return null; }

        var design = new Pattern { id = Guid.NewGuid().ToString("N"), displayName = sp.name, craft = (CraftType)sp.craft, width = sp.width, height = sp.height };
        foreach (var hex in sp.palette)
            design.palette.Add(ColorUtility.TryParseHtmlString("#" + hex, out var c) ? c : Color.white);
        design.threads.AddRange(sp.threads);
        if (!ReadGrid(sp.grid, design)) { error = "That code doesn't look right."; return null; }
        Materials.EnsureThreads(design, content);

        string name = string.IsNullOrWhiteSpace(sp.name) ? "A friend's pattern" : sp.name.Trim();
        if (name.Length > 30) name = name.Substring(0, 30);
        design.displayName = name;
        Save.patterns.Add(new OwnedPattern
        {
            id = design.id, name = name, itemId = sp.itemId, price = Mathf.Clamp(sp.price, 1, 999),
            onShelf = false, designed = true, design = design,
        });
        return $"the pattern \"{name}\" (it's in your Pattern Studio)";
    }

    // ---- Encoding ----

    static string NewId() => Guid.NewGuid().ToString("N").Substring(0, 12);

    static string Encode(Packet packet)
    {
        string body = Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonUtility.ToJson(packet)))
            .TrimEnd('=').Replace('+', '-').Replace('/', '_');
        return Prefix + body + "." + Check(body);
    }

    static Packet Decode(string code, out string error)
    {
        error = "That code doesn't look right. Check it was copied in full.";
        if (string.IsNullOrWhiteSpace(code)) { error = "Paste a code from a friend first."; return null; }
        var sb = new StringBuilder();
        foreach (char ch in code) if (!char.IsWhiteSpace(ch)) sb.Append(ch);   // messages often wrap long codes
        string s = sb.ToString();
        if (s.StartsWith("TT") && !s.StartsWith(Prefix)) { error = "This code is from a newer version of the game. Update to open it."; return null; }
        if (!s.StartsWith(Prefix)) return null;
        s = s.Substring(Prefix.Length);
        int dot = s.LastIndexOf('.');
        if (dot <= 0) return null;
        string body = s.Substring(0, dot);
        if (s.Substring(dot + 1) != Check(body)) return null;
        try
        {
            string b64 = body.Replace('-', '+').Replace('_', '/');
            b64 = b64.PadRight(b64.Length + (4 - b64.Length % 4) % 4, '=');
            var packet = JsonUtility.FromJson<Packet>(Encoding.UTF8.GetString(Convert.FromBase64String(b64)));
            if (packet == null || string.IsNullOrEmpty(packet.id)) return null;
            if (packet.v > PacketVersion) { error = "This code is from a newer version of the game. Update to open it."; return null; }
            error = null;
            return packet;
        }
        catch (Exception) { return null; }
    }

    /// FNV-1a, 6 hex digits: enough to catch a code cut short or mistyped (not a lock against editing).
    static string Check(string body)
    {
        uint h = 2166136261;
        foreach (char c in body) { h ^= c; h *= 16777619; }
        return (h & 0xFFFFFF).ToString("X6");
    }

    /// One character per square, row by row: '.' empty, 'a'.. a colour (upper case = half stitch),
    /// with runs written as a count then the character ("12." = twelve empty squares).
    static string Grid(Pattern p)
    {
        var chars = new char[p.width * p.height];
        for (int i = 0; i < chars.Length; i++) chars[i] = '.';
        foreach (var c in p.cells)
            if (c.x >= 0 && c.x < p.width && c.y >= 0 && c.y < p.height && c.colorIndex >= 0 && c.colorIndex < 26)
                chars[c.y * p.width + c.x] = (char)((c.halfStitch ? 'A' : 'a') + c.colorIndex);
        var sb = new StringBuilder();
        for (int i = 0; i < chars.Length;)
        {
            int run = 1;
            while (i + run < chars.Length && chars[i + run] == chars[i]) run++;
            if (run > 2) sb.Append(run);
            else if (run == 2) sb.Append(chars[i]);
            sb.Append(chars[i]);
            i += run;
        }
        return sb.ToString();
    }

    static bool ReadGrid(string grid, Pattern p)
    {
        if (grid == null) return false;
        int pos = 0, count = 0;
        foreach (char ch in grid)
        {
            if (char.IsDigit(ch)) { count = count * 10 + (ch - '0'); if (count > p.width * p.height) return false; continue; }
            int run = Mathf.Max(1, count);
            count = 0;
            if (pos + run > p.width * p.height) return false;
            if (ch != '.')
            {
                bool half = ch >= 'A' && ch <= 'Z';
                int index = half ? ch - 'A' : ch - 'a';
                if (index < 0 || index >= 26 || index >= p.palette.Count) return false;
                for (int k = 0; k < run; k++)
                    p.cells.Add(new StitchCell { x = (pos + k) % p.width, y = (pos + k) / p.width, colorIndex = index, halfStitch = half });
            }
            pos += run;
        }
        return pos == p.width * p.height;
    }

    public string Describe(List<SupplyStock> items)
    {
        var parts = new List<string>();
        foreach (var x in items)
        {
            var s = content.Supply(x.supplyId);
            if (s == null) continue;
            parts.Add(s.kind == SupplyKind.Tool ? s.displayName : $"{x.amount} {s.unit} of {s.displayName}");
        }
        return string.Join(", ", parts);
    }
}
