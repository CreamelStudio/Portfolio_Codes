using System;
using UnityEngine;

public enum PlacementTarget { Up, Down, Both }

public enum ItemType
{
    Deco,
    Buff,
}

public enum DecorationType
{
    Muffler,
    Arm,
    Hat,
    Button,
    Eye,
    Mouth,
    Nose,
}

[Serializable, CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class ItemBase : ScriptableObject{
    public string itemName;
    [TextArea(3, 10)]public string itemDescription;
    public ItemType itemType;
    public Sprite itemIcon;
}


