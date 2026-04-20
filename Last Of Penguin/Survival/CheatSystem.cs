using NUnit.Framework.Constraints;
using UnityEngine;

public class CheatSystem : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.F10))
        {
            InventoryManager.Instance.AddItem(new InventoryItem(ItemGenerator.Instance.GetItemData("item_wood"), 10));
            InventoryManager.Instance.AddItem(new InventoryItem(ItemGenerator.Instance.GetItemData("item_iron_ingot"), 5));
        }
    }
}
