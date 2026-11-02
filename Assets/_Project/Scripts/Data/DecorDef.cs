using UnityEngine;

public enum DecorBoost { None, Experience, Visitors, Friendship }   // append only

public enum DecorSlot { Wall, Counter, Floor, Corner, CounterLeft, PickupWall, PickupShelf }   // append only: assets and saves store these as numbers

// Furniture or a decoration for the player's shop, from Marsh's Workshop (owner, 2026-10-31). Each goes in one spot
// in the shop (DecorSlot); placed pieces add charm, and customers pay a little more in a charming shop (ShopDay.Charm).
// Create via Assets > Create > Craft > Decor.
[CreateAssetMenu(menuName = "Craft/Decor")]
public class DecorDef : ScriptableObject
{
    public string decorId;               // permanent once saves exist
    public string displayName;
    public DecorSlot slot;
    public int price = 40;
    public int charm = 2;                // each point: customers pay 1% more (ShopDay.MaxCharm caps it)
    public int minLevel = 1;             // your level before Benji will sell it
    public Color color = Color.white;    // the placeholder's colour until there's art
    public Sprite icon;                  // shown in the workshop and in your shop (512 x 512)
    [Header("Where it's sold, and what it does (owner, 2026-11-01)")]
    public string shop = "";             // "" = Marsh's Workshop, "curios" = Quill's Curiosities
    public DecorBoost boost;
    public int boostAmount;              // percent
    public int boostFrom, boostTo;       // Visitors: the hours it works (e.g. 11 to 13)
    public CustomerDef boostFriend;      // Friendship: whose
}
