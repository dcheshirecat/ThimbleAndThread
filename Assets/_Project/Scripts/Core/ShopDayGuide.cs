/// The first day, step by step (owner, 2026-11-02: a tutorial that explains each step of the game as you play it, in
/// between the steps of the first day and night). Each card shows once, the first time you reach that point, in a new
/// game (older saves never see it). The crafting steps have their own explainers (Explainers / OrderFlowUI).
public partial class ShopDay
{
    public const string GuideFlag = "guide:on";
    bool Guiding => Save != null && Save.flags.Contains(GuideFlag);

    void Guide(string key, params string[] pages)
    {
        if (!Guiding || Save.flags.Contains($"explained:guide:{key}")) return;
        Explainers.Explain(Save, $"guide:{key}", true, pages);
    }

    /// The guide's cards for each screen, as you come to it.
    void GuideFor(ScreenId id)
    {
        if (!Guiding || playing || Save.pendingScenes.Count > 0 || !Save.player.created) return;
        switch (id)
        {
            case ScreenId.Shop when !Save.night && Save.day == 1:
                Guide("shop",
                    "Your shop\nThis is Thimble & Thread, by day. Behind the counter is you; the door is where customers come in. " +
                    "Swipe left for orders waiting to be picked up, and right for your pattern display.",
                    "The top bar\nTop left: your level (tap the bar to see your skills) and your portrait (tap it to change your look). " +
                    "Then the weekday (tap it for the calendar), the time, your coins, the bell for news, and the gear for settings.",
                    "Customers\nThey come in through the day. When the bell rings, tap the door to let them in. " +
                    "Between customers you can read, and later decorate. When you're ready to stop, tap the OPEN sign to close.");
                break;
            case ScreenId.Shop when !Save.night && Save.day == 2:
                Guide("day2",
                    "A new day\nCustomers come back for finished orders when they like: you won't know when, so keep the pieces wrapped " +
                    "and waiting. Today the Supply Market opens across the square: tap Shops to buy what your orders need.");
                break;
            case ScreenId.Dialogue when !Save.night:
                Guide("talk",
                    "Talking with customers\nRead what they say, then choose. See what they're after lets them tell you; Suggest a " +
                    "pattern shows them one from your display. Anything they order goes in your order book, to make tonight.");
                break;
            case ScreenId.DaySummary:
                Guide("summary", "The end of the day\nWhat you sold and earned today. Next comes the night: the shop's shut and you make things.");
                break;
            case ScreenId.Workroom:
                if (Save.flags.Contains("explained:guide:result"))
                    Guide("bed", "Bedtime\nWhen you've done enough for one night, Go to Bed. Unfinished work keeps; tomorrow the shop opens again.");
                else
                    Guide("workroom", "Night: your workroom\nThis is where you craft. Open the Order Book to see what you've promised people.");
                break;
            case ScreenId.OrderBook:
                Guide("orderbook", "The order book\nEvery order, and how far along it is. Tap one to open it at the workbench.");
                break;
            case ScreenId.Workbench:
                Guide("workbench", "The workbench\nAn order's steps, from first to last, and what it needs. Tap Start to begin: first you'll take " +
                                   "the supplies out of your cupboard.");
                break;
            case ScreenId.Gather:
                Guide("gather", "Gathering\nTap each thing on the list in your cupboard to take it out. When every line is ticked, Start Crafting.");
                break;
            case ScreenId.Result:
                Guide("result", "Finished!\nSet it aside, wrapped. The customer comes back for it another day: that's when they pay, " +
                                "and you'll see how they like it.");
                break;
        }
    }

    /// The bell, the very first time.
    void GuideBell() => Guide("bell", "Ding-dong!\nSomeone's at the door. Tap the door to let them in.");
}
