using UnityEngine;

// A commission: a special request from one customer. The player designs a brand-new pattern for it
// (the design isn't judged), then makes the item. It may need a rarer material. Each commission is
// offered until it's delivered, once. After delivery its pattern goes into the player's book.
// Create via Assets > Create > Craft > Commission.
// Lives in its own file: Unity only links a ScriptableObject asset to its class when the file name matches.
[CreateAssetMenu(menuName = "Craft/Commission")]
public class CommissionDef : ScriptableObject
{
    public string commissionId;            // stable id used in save files
    public CustomerDef customer;
    public CraftItemDef item;              // what gets made
    public int minRelationshipLevel;       // offered once the friendship reaches this level
    [TextArea(3, 8)] public string request;   // what they say, often indirectly
    public SupplyDef requiredSupply;       // optional: the fabric or yarn it must be made from (e.g. 100% alpaca)
    public int bonusCoins = 20;            // on top of the usual price
    public int bonusFriendship = 15;       // on top of the usual friendship
}
