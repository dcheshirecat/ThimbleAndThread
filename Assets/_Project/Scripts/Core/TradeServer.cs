using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

/// Six-character trade codes (owner's choice, 2026-10-30: an online service). The long offline code
/// (Trading) is stored in the owner's Firebase Realtime Database under trades/CODE, and the friend gets it
/// back with the short code. Opening a code deletes it on the server, so it can only be opened once; the
/// delete is conditional on the copy just read (an ETag), so two friends can't both open it.
/// The database's rules (in CLAUDE.md) only allow creating a new code, reading one, or deleting one.
/// No accounts and no keys: the database address isn't a secret. Without it, or offline, the long code
/// still works.
public static class TradeServer
{
    /// The Realtime Database address, e.g. "https://thimble-trades-default-rtdb.europe-west1.firebasedatabase.app".
    /// Empty = online codes are off (the long codes still work).
    public const string DatabaseUrl = "https://thimble-trades-default-rtdb.firebaseio.com";

    /// Letters and digits that can't be mixed up (no I, O, 0 or 1).
    const string Alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
    public const int Length = 6;
    const int Timeout = 15;

    public static bool Ready => DatabaseUrl.Length > 0;

    /// A short code as typed: upper case, without spaces or dashes; null if it can't be one.
    public static string Clean(string typed)
    {
        if (string.IsNullOrEmpty(typed)) return null;
        var s = typed.Trim().ToUpperInvariant().Replace(" ", "").Replace("-", "");
        if (s.Length != Length) return null;
        foreach (char c in s) if (Alphabet.IndexOf(c) < 0) return null;
        return s;
    }

    static string NewCode()
    {
        var chars = new char[Length];
        for (int i = 0; i < Length; i++) chars[i] = Alphabet[UnityEngine.Random.Range(0, Alphabet.Length)];
        return new string(chars);
    }

    static string Url(string code) => $"{DatabaseUrl.TrimEnd('/')}/trades/{code}.json";

    [Serializable] class Stored { public string packet; }

    /// Puts a long code on the server under a new short code. done(shortCode, error).
    public static IEnumerator Upload(string longCode, Action<string, string> done)
    {
        if (!Ready) { done(null, "Online codes aren't set up yet."); yield break; }
        string body = "{\"packet\":\"" + longCode + "\",\"made\":{\".sv\":\"timestamp\"}}";
        for (int attempt = 0; attempt < 5; attempt++)
        {
            string code = NewCode();
            using var req = UnityWebRequest.Put(Url(code), body);
            req.SetRequestHeader("Content-Type", "application/json");
            req.timeout = Timeout;
            yield return req.SendWebRequest();
            if (req.result == UnityWebRequest.Result.Success) { done(code, null); yield break; }
            if (req.responseCode == 401 || req.responseCode == 403) continue;   // that code's taken: try another
            done(null, Offline(req));
            yield break;
        }
        done(null, "Couldn't find a free code just now. Try again in a moment.");
    }

    /// Fetches a short code's long code and removes it from the server, so it opens only once.
    /// check(longCode) can refuse it first (e.g. your own code), and then it stays on the server.
    /// done(longCode, error).
    public static IEnumerator Claim(string shortCode, Func<string, string> check, Action<string, string> done)
    {
        if (!Ready) { done(null, "Online codes aren't set up yet."); yield break; }
        string etag, longCode;
        using (var get = UnityWebRequest.Get(Url(shortCode)))
        {
            get.SetRequestHeader("X-Firebase-ETag", "true");
            get.timeout = Timeout;
            yield return get.SendWebRequest();
            if (get.result != UnityWebRequest.Result.Success) { done(null, Offline(get)); yield break; }
            string json = get.downloadHandler.text;
            if (string.IsNullOrEmpty(json) || json == "null") { done(null, "There's no trade with that code. It may have been opened already, or a letter's wrong."); yield break; }
            longCode = JsonUtility.FromJson<Stored>(json)?.packet;
            etag = get.GetResponseHeader("ETag");
        }
        if (string.IsNullOrEmpty(longCode)) { done(null, "That code doesn't look right."); yield break; }
        string refused = check?.Invoke(longCode);
        if (refused != null) { done(null, refused); yield break; }
        using (var del = UnityWebRequest.Delete(Url(shortCode)))
        {
            if (!string.IsNullOrEmpty(etag)) del.SetRequestHeader("if-match", etag);
            del.timeout = Timeout;
            yield return del.SendWebRequest();
            if (del.responseCode == 412) { done(null, "Someone else just opened that code."); yield break; }
            if (del.result != UnityWebRequest.Result.Success) { done(null, Offline(del)); yield break; }
        }
        done(longCode, null);
    }

    static string Offline(UnityWebRequest req) =>
        req.result == UnityWebRequest.Result.ConnectionError
            ? "No internet connection. Long codes work without one."
            : $"The trading service didn't answer ({req.responseCode}). Try again later, or use the long code.";
}
