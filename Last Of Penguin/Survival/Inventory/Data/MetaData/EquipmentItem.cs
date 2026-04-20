using System.Collections.Generic;
using UnityEngine;

public enum EquipmentSlot { Hat, Outfit, Accessory, Bag }

[CreateAssetMenu(fileName = "EquipmentMeta", menuName = "Add ItemMeta/Equipment")]
public class EquipmentItem : MetaDataBase
{
    public EquipmentSlot equipmentSlot;
    public float defense;
    public List<string> effectTags;
    public string effectDescription;
}