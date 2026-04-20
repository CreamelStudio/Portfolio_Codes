using UnityEngine;

public enum FertilizerEffectType { GrowthBoost, HeatResistance, ColdResistance, PoisonResistance }

[CreateAssetMenu(fileName = "FertilizerMeta", menuName = "Add ItemMeta/Fertilizer")]
public class FertilizerItem : MetaDataBase
{
    public FertilizerEffectType effectType;
    public int growthLevel;
}