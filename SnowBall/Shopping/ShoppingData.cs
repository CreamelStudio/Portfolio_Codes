using UnityEngine;
using System;

[Serializable, CreateAssetMenu(fileName = "New Shopping Data", menuName = "Items/Shopping Data")]
public class ShoppingData : ScriptableObject
{
    public int price;
    public int amount = 1;
    public ItemBase item;
}