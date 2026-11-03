using System.Collections.Generic;
using UnityEngine;

/// Pets for the shop (owner, 2026-11-02 evening), from Wren's Pet Emporium (Poppy Wren, level 4). A pet is a DecorDef with
/// pet set (shop "pets"): put it out with the pencil like any piece, and it adds charm. Out in the shop it wanders off now
/// and then to nap on your other pieces (UI/DecorPet.cs); tap it to stroke it (once a day each: a little experience) and,
/// the first time, to give it a name. Names are kept in Save.flags as "petname:<decorId>=<name>" (no new save field).
public partial class ShopDay
{
    public const string PetOwnerId = "poppy", PetFlag = "shop:pets";
    public const int PetLevel = 4;

    public string PetName(string decorId)
    {
        string prefix = $"petname:{decorId}=";
        var f = Save.flags.Find(x => x.StartsWith(prefix));
        return f != null ? f.Substring(prefix.Length) : null;
    }

    public void SetPetName(string decorId, string name)
    {
        name = (name ?? "").Trim();
        if (name.Length == 0) return;
        if (name.Length > 16) name = name.Substring(0, 16);
        Save.flags.RemoveAll(x => x.StartsWith($"petname:{decorId}="));
        Save.flags.Add($"petname:{decorId}={name}");
        Stamps.Earn(Save, "pet:named", "Part of the family", $"{name} has a name and a home.", StampTheme.Shop);
        GM.Persist();
    }

    /// Stroking a pet: a happy line, and a little experience once a day each.
    public string StrokePet(string decorId)
    {
        var d = Decor(decorId);
        if (d == null) return null;
        string name = PetName(decorId) ?? $"The {d.displayName.ToLower()}";
        string flag = $"stroked:{decorId}:{Save.day}";
        if (!Save.flags.Contains(flag))
        {
            Save.flags.Add(flag);
            Progress.AddBonusXp(3);
            GM.Persist();
        }
        bool cat = decorId.Contains("cat"), dog = decorId.Contains("dog") || decorId.Contains("terrier");
        return cat ? Pick($"{name} purrs and stretches out a paw.", $"{name} leans into your hand, purring like a sewing machine.", $"{name} blinks at you slowly. That's a cat's kiss.")
             : dog ? Pick($"{name} wags so hard the whole back end wiggles.", $"{name} rolls over for a belly rub.", $"{name} brings you a sock. It's not yours.")
             : Pick($"{name} twitches a nose at you, very pleased.", $"{name} settles happily under your hand.", $"{name} chirps a little hello.");
    }

    public void ChatWithPetOwner() => ChatWith(PetOwnerId, ScreenId.PetShop);
    public void LeavePetShop() => LeaveShop(PetOwnerId);
}
