using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using Work.KJY.Code.Core;
using Work.KJY.Code.Manager;

public class UseBuffItem : MonoBehaviour
{
    public static UseBuffItem Instance;
    
    public GameObject useBuffPannel;

    public ItemBase SprayerItem;
    public ItemBase TeruTeruBozuItem;

    public TMP_Text SprayerItemCountText;
    public TMP_Text TeruTeruBozuItemCountText;

    public GameObject SprayerCheckIcon;
    public GameObject TeruTeruBozuCheckIcon;

    public bool isUsedSprayer;
    public bool isUsedTeruTeruBozu;

    void Awake()
    {
        if(Instance == null) Instance = this;
        else Destroy(gameObject);
        
    }


    void FixedUpdate()
    {
        SprayerItemCountText.text = Inventory.Instance.GetItemCount(SprayerItem).ToString();
        TeruTeruBozuItemCountText.text = Inventory.Instance.GetItemCount(TeruTeruBozuItem).ToString();
    }

    public void OpenUseBuffPannel()
    {
        useBuffPannel.SetActive(true);

        if(isUsedSprayer) UseSprayer();
        if(isUsedTeruTeruBozu) UseTeruTeruBozu();
    }

    public void CloseUseBuffPannel()
    {
        IrisFadeManager.Instance.FadeIn(1f, "PlayerTestScene");
    }


    public void UseSprayer()
    {
        if(isUsedSprayer == false && Inventory.Instance.UseBuffItem(SprayerItem))
        {
            SprayerCheckIcon.SetActive(true);
            isUsedSprayer = true;
        }
        else
        {
            Inventory.Instance.UnUseBuffItem(SprayerItem);
            SprayerCheckIcon.SetActive(false);
            isUsedSprayer = false;
        }
        
    }

    public void UseTeruTeruBozu()
    {
        if(isUsedTeruTeruBozu == false && Inventory.Instance.UseBuffItem(TeruTeruBozuItem))
        {
            TeruTeruBozuCheckIcon.SetActive(true);
            isUsedTeruTeruBozu = true;
        }
        else
        {
            Inventory.Instance.UnUseBuffItem(TeruTeruBozuItem);
            TeruTeruBozuCheckIcon.SetActive(false);
            isUsedTeruTeruBozu = false;
        }
    }
}
