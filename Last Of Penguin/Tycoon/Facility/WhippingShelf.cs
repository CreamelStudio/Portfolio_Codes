using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WhippingShelf : Shelf
{
    [Header("WhippingShelf")]
    [SerializeField] private GameObject whippingObject;
    
    private void Start()
    {
        GameObject whipping = Instantiate(whippingObject);
        ShelfOnItem(whipping);
    }

    public override GameObject ReturnItemHand(ref GameObject handItem)
    {
        //return base.ReturnItemHand(ref handItem);
        return handItem;
    }
}
