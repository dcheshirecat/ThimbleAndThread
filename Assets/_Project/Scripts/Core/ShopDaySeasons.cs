using System.Collections.Generic;
using UnityEngine;

public enum Weather { Sunny, Cloudy, Rain, Windy, Fog, Snow }

/// The year outside the shop (owner, 2026-11-02 evening): weather, seasonal supplies and festivals.
/// - Weather: one kind a day, picked per save and day (the same after a reload), likelier by season (snow only in winter).
///   It changes who visits (some people love a sunny day, some come in out of the rain), how many, and what they want
///   (warm things in cold or wet weather, light ones in the sun).
/// - Seasonal supplies (SupplyDef.season 1..4): on the market's shelves all through their season, gone the rest of the year.
/// - Festivals: each season's third Sunday, the stall goes to a festival with its own rules (FestivalDef below).
public partial class ShopDay
{
    // ---- Weather ----

    public Weather WeatherOn(int day)
    {
        int season = (Week.DayOfYear(day) - 1) / Week.SeasonDays;   // 0 spring .. 3 winter
        // Weights in Weather order: Sunny, Cloudy, Rain, Windy, Fog, Snow
        int[] w = season switch
        {
            0 => new[] { 35, 25, 30, 10, 0, 0 },
            1 => new[] { 60, 20, 10, 10, 0, 0 },
            2 => new[] { 20, 25, 30, 15, 10, 0 },
            _ => new[] { 15, 25, 20, 10, 10, 20 },
        };
        if (Save.marketSeed == 0) { Save.marketSeed = Random.Range(1, int.MaxValue); GM.Persist(); }   // this save's own weather
        int total = 0;
        foreach (int x in w) total += x;
        var rng = new System.Random(unchecked(Save.marketSeed * 7919 + day * 104729));
        int roll = rng.Next(total);
        for (int i = 0; i < w.Length; i++) { if (roll < w[i]) return (Weather)i; roll -= w[i]; }
        return Weather.Cloudy;
    }

    public Weather Today => WeatherOn(Save.day);

    public static string WeatherWord(Weather w) => w switch
    {
        Weather.Sunny => "Sunny", Weather.Cloudy => "Cloudy", Weather.Rain => "Rain", Weather.Windy => "Windy",
        Weather.Fog => "Sea fog", _ => "Snow",
    };

    public static bool Cold(Weather w) => w == Weather.Rain || w == Weather.Windy || w == Weather.Fog || w == Weather.Snow;

    /// More people come out on a fine day; fewer brave the rain or snow.
    public int WeatherVisits => Today switch { Weather.Sunny => 2, Weather.Rain => -2, Weather.Snow => -3, Weather.Fog => -1, _ => 0 };

    // Who likes which weather (they're likelier to be among the day's visitors).
    static readonly string[] SunLovers = { "sol", "juno", "kit", "felix", "isla", "eli", "sam" };
    static readonly string[] RainLovers = { "tobias", "hal", "gwen", "dot", "midge", "nadia", "owen", "wenna" };

    public bool LikesWeather(string customerId, Weather w) =>
        w == Weather.Sunny ? System.Array.IndexOf(SunLovers, customerId) >= 0
        : Cold(w) && System.Array.IndexOf(RainLovers, customerId) >= 0;

    static readonly string[] WarmItems = { "scarf", "cable_scarf", "sweater", "quilt", "cushion", "plushie" };
    static readonly string[] SunItems = { "tote", "hoop_art", "wall_hanging", "tablecloth", "plushie_sewn" };

    /// Something the day's weather makes people want.
    public bool SuitsWeather(CraftItemDef item)
    {
        if (item == null) return false;
        var w = Today;
        return Cold(w) ? System.Array.IndexOf(WarmItems, item.itemId) >= 0 : w == Weather.Sunny && System.Array.IndexOf(SunItems, item.itemId) >= 0;
    }

    // ---- Seasonal supplies ----

    public int SeasonNumber => (Week.DayOfYear(Save.day) - 1) / Week.SeasonDays + 1;   // 1 spring .. 4 winter

    /// Seasonal supplies are only about in their season.
    public bool InSeason(SupplyDef s) => s == null || s.season == 0 || s.season == SeasonNumber;

    // ---- Festivals ----

    public class FestivalDef
    {
        public string name, blurb, stampText;
        public float pieceBonus = 1f, patternBonus = 1f;   // what shoppers offer, times
        public string[] favourites;                        // items that sell for even more
        public float favouriteBonus = 1f;
        public int gapMin = 4, gapMax = 10;                // game minutes between shoppers
    }

    static readonly FestivalDef[] Festivals =
    {
        new FestivalDef { name = "Blossom Fair", gapMin = 4, gapMax = 9, patternBonus = 1.5f,
            blurb = "Cherry trees in flower all along the square. Crafters come from all over: patterns sell for half as much again.",
            stampText = "A stall at the spring Blossom Fair." },
        new FestivalDef { name = "Regatta Day", gapMin = 3, gapMax = 7, pieceBonus = 1.15f, favourites = new[] { "tote", "hoop_art", "wall_hanging" }, favouriteBonus = 1.4f,
            blurb = "Sails in the bay and crowds on the quay: the busiest day of the summer. Totes and anything for the wall fly off the stall.",
            stampText = "A stall on Regatta Day." },
        new FestivalDef { name = "Harvest Fair", gapMin = 4, gapMax = 9, pieceBonus = 1.3f, favourites = new[] { "scarf", "cable_scarf", "sweater", "quilt", "cushion" }, favouriteBonus = 1.6f,
            blurb = "Pumpkins, cider and the first cold mornings: handmade pieces fetch more, and anything warm most of all.",
            stampText = "A stall at the autumn Harvest Fair." },
        new FestivalDef { name = "Winter Market", gapMin = 3, gapMax = 8, pieceBonus = 1.3f, patternBonus = 1.3f,
            blurb = "Lanterns, mulled apple juice and everyone buying presents: everything sells for more.",
            stampText = "A stall at the Winter Market." },
    };

    public const int FestivalDayOfSeason = 21;   // the season's third Sunday (day 1 is a Monday; seasons are 4 weeks)

    public FestivalDef FestivalOn(int day) => Week.DayOfSeason(day) == FestivalDayOfSeason ? Festivals[(Week.DayOfYear(day) - 1) / Week.SeasonDays] : null;
    public FestivalDef FestivalToday => Save != null ? FestivalOn(Save.day) : null;

    /// What a shopper offers at today's festival, for a piece or a pattern copy.
    int FestivalOffer(int offer, CraftItemDef item, bool pattern)
    {
        var f = FestivalToday;
        if (f == null) return offer;
        float k = pattern ? f.patternBonus : f.pieceBonus;
        if (!pattern && item != null && f.favourites != null && System.Array.IndexOf(f.favourites, item.itemId) >= 0) k *= f.favouriteBonus;
        return Mathf.Max(1, Mathf.RoundToInt(offer * k));
    }

    /// The first time each festival comes round: a stamp and a card about it.
    void AnnounceFestival()
    {
        var f = FestivalToday;
        if (f == null) return;
        string flag = "festival:" + f.name;
        Stamps.Earn(Save, flag, f.name, f.stampText, StampTheme.Shop);
        Explainers.Explain(Save, flag, true, $"{f.name}\n{f.blurb}");
    }
}
