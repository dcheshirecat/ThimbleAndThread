using UnityEngine;

// What a shop upgrade does when bought. Append new values at the end: assets store these as numbers.
public enum UpgradeEffect { DisplaySpot, ExtraVisitor, CraftLamp }

// Something to improve the shop with, bought once at the Supply Market ("For the Shop" shelf):
// more display space, a sign that brings more visitors, a better lamp to learn faster by...
// Create via Assets > Create > Craft > Upgrade.
// Lives in its own file: Unity only links a ScriptableObject asset to its class when the file name matches.
[CreateAssetMenu(menuName = "Craft/Upgrade")]
public class UpgradeDef : ScriptableObject
{
    public string upgradeId;               // stable id used in save files
    public string displayName;
    [TextArea(2, 4)] public string description;
    public Sprite icon;
    public int price = 100;
    public int chapter = 1;                // offered from this story chapter on
    public UpgradeEffect effect;
    public int amount = 1;                 // e.g. how many display spots, or the % of extra experience
}
