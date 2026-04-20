using UnityEngine;

public class PropBase : MonoBehaviour
{
    public PropData[] propData;
    public PropData[] subPropData;
    public GameObject outline;

    public bool isAlreadyInteracted = false;
    
    private void OnDrawGizmos() {
        
        Gizmos.color = Color.cyan;
        Vector3 center = transform.position + (Vector3)propData[(int)GameManager.Instance.language].interactiveAreaOffset;
        Vector3 size = new Vector3(propData[(int)GameManager.Instance.language].interactiveAreaSize.x, propData[(int)GameManager.Instance.language].interactiveAreaSize.y, 0.1f);
        Gizmos.DrawWireCube(center, size);
    }

    void Update()
    {
        Collider2D[] colliders = Physics2D.OverlapBoxAll(
            (Vector2)transform.position + propData[(int)GameManager.Instance.language].interactiveAreaOffset,
            propData[(int)GameManager.Instance.language].interactiveAreaSize,
            0f
        );

        bool isPlayerNearby = false;

        foreach (var collider in colliders) {
            if (collider.CompareTag("Player"))
            {
                if(Input.GetKeyDown(KeyCode.F) && !DialogueSystem.Instance.isDialogueActive && !isAlreadyInteracted) 
                {
                    DialogueSystem.Instance.StartDialogue(propData[(int)GameManager.Instance.language]);
                    isAlreadyInteracted = true;
                }
                else if(Input.GetKeyDown(KeyCode.F) && !DialogueSystem.Instance.isDialogueActive && isAlreadyInteracted && subPropData != null) 
                {
                    DialogueSystem.Instance.StartDialogue(subPropData[(int)GameManager.Instance.language]);
                }
                isPlayerNearby = true;
            }
        }

        outline.SetActive(isPlayerNearby);
    }
}
