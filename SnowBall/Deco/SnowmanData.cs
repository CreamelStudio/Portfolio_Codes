using System.Collections.Generic;
using UnityEngine;

public class SnowmanData
{
    public float snowmanUpSize;
    public float snowmanDownSize;

    public List<DecorationItem> decorationItems;

    public SnowmanData(float snowmanUpSize, float snowmanDownSize, List<DecorationItem> decorationItems)
    {
        this.snowmanUpSize = snowmanUpSize;
        this.snowmanDownSize = snowmanDownSize;
        this.decorationItems = decorationItems;
    }

    public void AddDecorationItem(DecorationItem item)
    {
        decorationItems.Add(item);
    }

    public void RemoveDecorationItem(DecorationItem item)
    {
        decorationItems.Remove(item);
    }
}
