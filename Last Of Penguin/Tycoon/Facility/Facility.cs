using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FacilityType
{
    enProductionFacility, // 생산시설
    enSupplyFacility, // 재료 수급 시설
    enenSubmissionGacility, // 제출시설     
    enShelf, // 선반
    enRaill, // 레일
    enAshBm, // 쓰레기통
    enProcessFacility, //가공시설

    MAX_FacilityType

}
public class Facility : MonoBehaviour
{
    public MeshRenderer meshRenderer;

    public SkinnedMeshRenderer skinnedMeshRenderer;

    public FacilityType facilityType;

    public Material originalMaterial;
    public Material CoatingMaterial;

    [Header("ShelfInfo")]
    [SerializeField] private Transform onItemPos;

    [Header("OnItem")]
    public GameObject currentOnItem;
    private bool isOnItem;

    private const int NO_DATA = int.MinValue;
    private int currentOnItemViewId = default;


    public void ShelfOnItem(GameObject obj)
    {
        if (!isOnItem)
        {
            isOnItem = true;
            currentOnItem = obj;
            currentOnItem.transform.position = onItemPos.position;
            //currentOnItem.transform.rotation = onItemPos.transform.rotation;
            currentOnItem.transform.rotation = Quaternion.identity;
            currentOnItem.transform.parent = transform;

            var itemScript = obj.GetComponent<SupplyIngredients>();

            if (itemScript.itemType == ItemType.enPlate || itemScript.itemType == ItemType.enIce)
            {
                AudioEffectManager.instance.PlaySound(transform.position, 1, Random.Range(1f, 1.1f));
            }
            else if (itemScript.itemType == ItemType.enStrawberry || itemScript.itemType == ItemType.enChocolate || itemScript.itemType == ItemType.enVanilla)
            {
                if (itemScript.isProcessed)
                {
                    AudioEffectManager.instance.PlaySound(transform.position, 5, Random.Range(1f, 1.1f));
                }
                else
                {
                    AudioEffectManager.instance.PlaySound(transform.position, 2, Random.Range(1f, 1.1f));
                }
            }
            Rigidbody curOnItemRb = currentOnItem.GetComponent<Rigidbody>();
            if (curOnItemRb != null)
            {
                // 손에 들 때, 모든 축을 고정
                curOnItemRb.constraints = RigidbodyConstraints.FreezePosition | RigidbodyConstraints.FreezeRotation;
            }
        }
    }

    public virtual GameObject ReturnItemHand(ref GameObject handItem)
    {
        if (!isOnItem)
        {
            Debug.Log("선반 위에 아이템이 없습니다");
        }
        else if (isOnItem && currentOnItem != null)
        {
            handItem = currentOnItem;
            currentOnItem = null;
            isOnItem = false;
        }
        return handItem;
    }
}