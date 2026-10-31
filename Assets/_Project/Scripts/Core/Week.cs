using UnityEngine;

/// The days of the week: day 1 is a Monday. Each weekday has its own colour and symbol
/// (Art/UI/Weekdays/weekday_<n>_<name>.png, drawn white so the badge's colour shows through).
public static class Week
{
    public static readonly string[] Names = { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };

    static readonly Color[] Colors =
    {
        new Color(0.45f, 0.62f, 0.85f),   // Monday: moon blue
        new Color(0.88f, 0.45f, 0.40f),   // Tuesday: coral
        new Color(0.30f, 0.68f, 0.66f),   // Wednesday: teal
        new Color(0.58f, 0.46f, 0.78f),   // Thursday: violet
        new Color(0.90f, 0.52f, 0.68f),   // Friday: rose
        new Color(0.92f, 0.62f, 0.30f),   // Saturday: orange
        new Color(0.88f, 0.72f, 0.22f),   // Sunday: gold
    };

    /// 0 = Monday ... 6 = Sunday.
    public static int Index(int day) => ((day - 1) % 7 + 7) % 7;
    public static string Name(int day) => Names[Index(day)];
    public static Color ColorOf(int day) => Colors[Index(day)];
    public static bool IsSaturday(int day) => Index(day) == 5;
    public static bool IsSunday(int day) => Index(day) == 6;

    /// Which week of the game it is (the first is week 1).
    public static int Number(int day) => (day - 1) / 7 + 1;

    /// The symbol art's file name for a weekday index (0 = Monday).
    public static string SymbolFile(int index) => $"weekday_{index + 1}_{Names[index].ToLowerInvariant()}.png";
}
