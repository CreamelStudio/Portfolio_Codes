using UnityEngine;
using GlobalAudio;
using System.Collections;
using Unity.Collections.LowLevel.Unsafe;

public class DropItemManage : MonoBehaviour
{
    public SpriteRenderer plane;
    public bool isEnable = true; // ��� ������ Ȱ��ȭ ����
    public InventoryItem item; // ��� ������ ����
    [SerializeField] private AudioClip itempPickUpClip;

    private Coroutine enableDestroy;

    public void DropItemUIInit()
    {
        plane.sprite = item.item.icon;
    }

    private void OnEnable()
    {
        enableDestroy = StartCoroutine(AutoDesrtoy());
    }

    private void OnDisable()
    {
        StopCoroutine(enableDestroy);
    }

    private void OnTriggerStay(Collider other)
    {
        if (isEnable && other.gameObject.CompareTag("Player"))
        {
            isEnable = false; // 만약 플레이어가 버렸을 때 즉시 줍기 방지
            AudioPlayer.PlaySound(this.gameObject, itempPickUpClip);
            CharacterController controller = other.GetComponent<CharacterController>();

            if (controller.inv.IsFullInventory() || controller == null)
                return;

            if (LOPNetworkManager.Instance.isConnected)
            {
                NetworkIdentity identity = controller.GetComponent<NetworkIdentity>();

                if (identity.IsOwner) 
                {
                    controller.inv.AddItem(item);
                }
                LOPNetworkManager.Instance.NetworkDestroy(gameObject);
            }
            else if (!LOPNetworkManager.Instance.isConnected)
            {
                controller.inv.AddItem(item);
                Destroy(gameObject);
            }
        }
    }

    public IEnumerator EnablePickUp()
    {
        yield return new WaitForSeconds(1.3f);
        isEnable = true;
    }

    public IEnumerator AutoDesrtoy()
    {
        yield return new WaitForSeconds(120f);

        if (LOPNetworkManager.Instance.isConnected)
        {
            LOPNetworkManager.Instance.NetworkDestroy(gameObject);
        }
        else if(LOPNetworkManager.Instance.isConnected == false)
        {
            Destroy(gameObject);
        }
    }
}
