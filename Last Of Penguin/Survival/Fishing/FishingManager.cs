using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore.Text;
using static WorldInputManager;

public class FishingManager : MonoBehaviour
{
    public static FishingManager Instance;

    // [SerializeField] private GameObject fishingUI;
    // [SerializeField] private GameObject moveFish;
    // [SerializeField] private GameObject caughtIcon;
    // [SerializeField] private TMP_Text txt_success;
    // [SerializeField] private TMP_Text txt_currentFishing;
    public bool isFishing = false;
    public bool enterFishing = false;

    [Header("Caught Fish")]
    [SerializeField] private float caughtTimemmin;
    [SerializeField] private float caughtTimemmax;
    public bool isCaught = false;

    [Header("Fish Movement")]
    [SerializeField] private float moveDistance = 300f;
    [SerializeField] private float movementSpeedmin;
    [SerializeField] private float movementSpeedmax;

    private float movementSpeed;

    [SerializeField] private float successDistance;

    private bool isMovingRight = true;
    private CharacterController doCharacter;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void FishingStart(CharacterController character)
    {
        Debug.Log("Fishing Start!");
        doCharacter = character;
        enterFishing = true;

        QuickslotNumberBtn.Instance.cantChange = true;

        FishingPopup.Instance.Show(null);
        FishingPopup.Instance.ShowWaitingState();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        float caughtFishTime = Random.Range(caughtTimemmin, caughtTimemmax);
        Invoke("ShowCaughtIcon", caughtFishTime);
    }

    private void ShowCaughtIcon()
    {
        isCaught = true;

        FishingPopup.Instance.ShowCaughtState();

        Invoke("EndFishing", 5f);
    }

    public void ShowFishingUI()
    {
        CancelInvoke("EndFishing");
        isFishing = true;

        // Popup의 ShowFishingState 메서드 호출
        FishingPopup.Instance.ShowFishingState();

        movementSpeed = Random.Range(movementSpeedmin, movementSpeedmax);
        Invoke("EndFishing", 5f);
    }

    private void Update()
    {
        //if (PanelBase.ActivePanelCount > 0)
        //{
        //    return;
        //}

        MovementFish();
    }

    private void MovementFish()
    {
        if (!isFishing) return;

        GameObject fishObj = FishingPopup.Instance.GetMoveFish();

        if (fishObj.transform.localPosition.x >= moveDistance) isMovingRight = false;
        else if (fishObj.transform.localPosition.x <= -moveDistance) isMovingRight = true;

        if (isMovingRight)
        {
            fishObj.transform.position += Vector3.right * movementSpeed * Time.deltaTime;
        }
        else
        {
            fishObj.transform.position += Vector3.left * movementSpeed * Time.deltaTime;
        }
    }

    public void EndFishing()
    {
        WorldInputManager.Instance.gameInputType = GameInputType.None;
        Debug.Log("Fishing End!");
        CancelInvoke("ShowCaughtIcon");
        CancelInvoke("EndFishing");

        isFishing = false;
        isCaught = false;
        enterFishing = false;

        QuickslotNumberBtn.Instance.cantChange = false;

        if (doCharacter != null)
        {
            doCharacter.OnFinishFishing();
            doCharacter = null;
        }
        FishingPopup.Instance.Hide();
    }

    public IEnumerator Co_isFishingEnd()
    {
        WorldInputManager.Instance.gameInputType = GameInputType.None;
        bool isSuccess = false;
        CancelInvoke("EndFishing");
        isFishing = false;
        enterFishing = false;

        QuickslotNumberBtn.Instance.cantChange = false;

        float fishPosX = FishingPopup.Instance.GetMoveFish().transform.localPosition.x;
        isSuccess = Mathf.Abs(fishPosX) <= successDistance;

        FishingPopup.Instance.ShowResult(isSuccess);

        if (isSuccess)
        {
            InventoryManager.Instance.AddItem(new InventoryItem(ItemGenerator.Instance.GetItemData("item_fish_commonCarp"), 1));
            WorldInputManager.Instance.gameInputType = GameInputType.None;
        }

        yield return new WaitForSeconds(0.4f);
        EndFishing();
    }
}