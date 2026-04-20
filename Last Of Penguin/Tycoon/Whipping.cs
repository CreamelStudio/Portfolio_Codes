using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class WhippingData
{
    public ItemType itemType;

    [Header("Meshes")]
    public GameObject originalMesh;
    public GameObject midMesh;
    public GameObject finalMesh;

    [Header("Result Prefab")]
    public GameObject processedPrefab;

    [Header("Tap Settings")]
    public float maxTapCount;
}
public class Whipping : SupplyIngredients
{
    [Header("Tap Count")]
    public float currentTapCount;

    [Header("UI")]
    [SerializeField] private Slider processSlider;

    [Header("Whipping Data")]
    [SerializeField] private List<WhippingData> whippingDataList;

    private Dictionary<ItemType, WhippingData> whippingDataDict;

    [SerializeField] public bool isProcess;
    [SerializeField] public ItemType whippingItemtype;

    private Animator anim;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        currentTapCount = 0;
        processSlider.gameObject.SetActive(false);

        // Dictionary 초기화
        whippingDataDict = new Dictionary<ItemType, WhippingData>();
        foreach (var data in whippingDataList)
        {
            whippingDataDict[data.itemType] = data;
        }
    }

    public GameObject Process(float dexterityValue)
    {
        if (!isProcess || !whippingDataDict.ContainsKey(whippingItemtype))
            return null;

        WhippingData data = whippingDataDict[whippingItemtype];

        if (data.maxTapCount <= currentTapCount) return null;

        AudioEffectManager.instance.PlaySound(transform.position, 4, Random.Range(1f, 1.2f));

        currentTapCount += dexterityValue;
        processSlider.gameObject.SetActive(true);
        processSlider.value = currentTapCount / data.maxTapCount;

        anim.SetTrigger("Whip");

        UpdatePuddingMesh(data, currentTapCount, data.maxTapCount);

        if (currentTapCount >= data.maxTapCount)
        {
            Debug.Log("가공이 완료되었습니다.");
            currentTapCount = 0;
            isProcess = false;
            processSlider.gameObject.SetActive(false);

            DeactivateAllMeshes(data);

            if (data.processedPrefab != null)
            {
                return Instantiate(data.processedPrefab, transform.position + new Vector3(0, 1, -1), Quaternion.identity);
            }
        }

        return null;
    }

    private void UpdatePuddingMesh(WhippingData data, float currentCount, float maxCount)
    {
        float ratio = currentCount / maxCount;

        data.originalMesh.SetActive(ratio <= 0.1f);
        data.midMesh.SetActive(ratio > 0.1f && ratio < 0.5f);
        data.finalMesh.SetActive(ratio >= 0.5f);
    }

    private void DeactivateAllMeshes(WhippingData data)
    {
        data.originalMesh?.SetActive(false);
        data.midMesh?.SetActive(false);
        data.finalMesh?.SetActive(false);
    }

    public GameObject GetProcessPrefab(ItemType itemType)
    {
        return whippingDataDict.TryGetValue(itemType, out var data) ? data.processedPrefab : null;
    }
}
