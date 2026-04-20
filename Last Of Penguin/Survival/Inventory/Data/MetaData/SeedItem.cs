using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SeedMeta", menuName = "Add ItemMeta/Seed")]
public class SeedItem : MetaDataBase
{
    public string cropId;
    public List<string> platableBlockIds;
}
