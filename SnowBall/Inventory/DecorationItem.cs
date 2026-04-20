using System;
using UnityEngine;

[Serializable, CreateAssetMenu(fileName = "New Decoration Item", menuName = "Items/Decoration Item")]
public class DecorationItem : ItemBase
{
    public int decorationObjectIndex;
    public DecorationType decorationType;

    public int itemValuePrice;

    // 배치형 옵션
    public bool isPlaceable;
    public GameObject placeablePrefab;
    public Vector3 rotationOffset;
    public PlacementTarget placementTarget = PlacementTarget.Up;
    public float surfaceOffset = 0.02f;
}

