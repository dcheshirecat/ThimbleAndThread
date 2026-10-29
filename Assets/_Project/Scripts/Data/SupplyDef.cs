using UnityEngine;

// Cloth, yarn, thread, etc. Create via Assets > Create > Craft > Supply.
[CreateAssetMenu(menuName = "Craft/Supply")]
public class SupplyDef : ScriptableObject
{
    public string supplyId;
    public string displayName;
    public CraftType craft;
    public Sprite icon;
    public int price = 10;
    [Range(1, 3)] public int tier = 1;   // higher tier = nicer materials = higher sale price
}
