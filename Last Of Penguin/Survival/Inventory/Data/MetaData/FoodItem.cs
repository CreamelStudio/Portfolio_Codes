using System.Collections.Generic;
using UnityEngine;

public enum FoodType { Raw, Cooked, Meal, Dessert }

[CreateAssetMenu(fileName = "FoodMeta", menuName = "Add ItemMeta/Food")]
public class FoodItem : MetaDataBase
{
    public FoodType foodType;
    public float healAmount;
    public float hungerRestore;
    public List<string> effectTags;
    public string effectDescription;
    public float effectDuration;
}