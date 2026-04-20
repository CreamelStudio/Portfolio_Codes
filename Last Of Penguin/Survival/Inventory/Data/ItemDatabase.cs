using UnityEngine;

public enum ItemType { Consumable, Material, Equipment, Tool }
public enum ItemMetaType { None, Food, Fertilizer, Block, Seed, Equipment, Tool, WaterBottle }
public enum ToolItemType { None, BoxingGlove, Axe, Fishingrod, Folk, Hammer, Scissor, Slingshot, Spoon, Sword, Torchlight, parka}

[CreateAssetMenu(fileName = "Item", menuName = "Add Item/Item")]
public class ItemDatabase : ScriptableObject
{
    [Header("Item Info")]
    public string itemID;
    public string itemName;
    public string itemDescription;
    public Sprite icon;
    public ItemType type;

    [Header("Item Stack")]
    public bool canStack;
    public bool canCraft;
    public int maxStack = 30;

    [Header("Meta Data")]
    public ItemMetaType metaType;
    public MetaDataBase metaData;

    [Header("ToolItem Info")]
    public ToolItemType toolType;
}
