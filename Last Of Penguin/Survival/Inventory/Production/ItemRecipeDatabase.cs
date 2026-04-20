using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Production", menuName = "Add Item Recipe/ItemRecipe")]
public class ItemRecipeDatabase : ScriptableObject
{
    public string returnItemId;
    public ItemForProdcution[] needItems;
}

[Serializable]
public struct ItemForProdcution
{
    public string itemId;
    public int amount;

    public ItemForProdcution(string itemId, int amount)
    {
        this.itemId = itemId;
        this.amount = amount;
    }
}
