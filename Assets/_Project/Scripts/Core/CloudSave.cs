using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

/// Cloud save sync (owner, 2026-11-02 evening). No accounts: turning it on gives the game a private 16-character code
/// (SaveData.cloudCode). The whole save is backed up to the owner's Firebase database at saves/CODE every night when the
/// player goes to bed, and whenever they tap Back Up Now in Settings. On another phone (or after reinstalling), typing
/// the code into Settings brings that game into the save slot. 16 characters from a 32-letter alphabet can't be guessed,
/// so the code is the key. The database needs a rule for saves/ (see CLAUDE.md); until it's added, uploads are refused.
public static class CloudSave
{
    const int Timeout = 20;
    static string Url(string code) => $"{TradeServer.DatabaseUrl.TrimEnd('/')}/saves/{code}.json";

    [Serializable] class Stored { public string data; public int day; public int coins; public string saved; }

    public static string NewCode()
    {
        const string alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        var bytes = new byte[16];
        using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create()) rng.GetBytes(bytes);
        var chars = new char[16];
        for (int i = 0; i < 16; i++) chars[i] = alphabet[bytes[i] % alphabet.Length];
        return new string(chars);
    }

    public static string Clean(string code)
    {
        if (string.IsNullOrWhiteSpace(code)) return null;
        var c = code.Trim().ToUpperInvariant().Replace(" ", "").Replace("-", "");
        return c.Length == 16 ? c : null;
    }

    /// For showing: ABCD-EFGH-JKLM-NPQR.
    public static string Pretty(string code) => string.IsNullOrEmpty(code) || code.Length != 16 ? code
        : $"{code.Substring(0, 4)}-{code.Substring(4, 4)}-{code.Substring(8, 4)}-{code.Substring(12, 4)}";

    /// Backs the save up under its code. done(error or null).
    public static IEnumerator Upload(SaveData save, Action<string> done)
    {
        if (!TradeServer.Ready || save == null || string.IsNullOrEmpty(save.cloudCode)) { done?.Invoke("Cloud save isn't on."); yield break; }
        string when = DateTime.Now.ToString("d MMM, HH:mm");
        var body = JsonUtility.ToJson(new Stored { data = JsonUtility.ToJson(save), day = save.day, coins = save.coins, saved = when });
        using var req = UnityWebRequest.Put(Url(save.cloudCode), body);
        req.SetRequestHeader("Content-Type", "application/json");
        req.timeout = Timeout;
        yield return req.SendWebRequest();
        if (req.result == UnityWebRequest.Result.Success) { save.cloudSavedAt = when; done?.Invoke(null); yield break; }
        done?.Invoke(req.result == UnityWebRequest.Result.ConnectionError ? "No internet connection: it'll back up next time."
                     : req.responseCode == 401 || req.responseCode == 403 ? "The cloud said no (it isn't set up for saves yet)."
                     : $"The cloud didn't answer ({req.responseCode}). It'll try again tonight.");
    }

    /// Fetches the game kept under a code. done(save, day, error).
    public static IEnumerator Download(string code, Action<SaveData, string> done)
    {
        code = Clean(code);
        if (code == null) { done(null, "A cloud code is 16 letters and numbers."); yield break; }
        using var req = UnityWebRequest.Get(Url(code));
        req.timeout = Timeout;
        yield return req.SendWebRequest();
        if (req.result != UnityWebRequest.Result.Success) { done(null, req.result == UnityWebRequest.Result.ConnectionError ? "No internet connection." : $"The cloud didn't answer ({req.responseCode})."); yield break; }
        var text = req.downloadHandler.text;
        if (string.IsNullOrEmpty(text) || text == "null") { done(null, "There's no game saved under that code."); yield break; }
        SaveData data = null;
        try { data = JsonUtility.FromJson<SaveData>(JsonUtility.FromJson<Stored>(text).data); } catch { }
        if (data == null) { done(null, "That game couldn't be read."); yield break; }
        data.cloudCode = code;
        done(data, null);
    }
}
