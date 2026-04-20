using GlobalAudio;
using UnityEngine;

public class MinimapSwitcher : Popup<MinimapSwitcher>
{
    public override PopupType PopupType => PopupType.MiniMapSwitcher;
    public static MinimapSwitcher Instance;

    public GameObject bigMap;
    public GameObject minimap;
    public Camera minimapCamera;

    [SerializeField] private AudioClip clickClip;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        bigMap.SetActive(false);
        minimap.SetActive(true);
    }

    public void ToggleMinimap(bool miniMapSwitch)
    {
        bigMap.SetActive(miniMapSwitch);
        minimap.SetActive(!miniMapSwitch);

    }

    protected override void OnShow(PopupArgument popupArguments)
    {
        AudioManager.Instance.PlaySFX(clickClip);
        bigMap.SetActive(true);
        minimap.SetActive(false);
        minimapCamera.orthographicSize = 100f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Show()
    {
        OnShow(new PopupArgument());
    }

    public void HideMap()
    {
        OnHide();
    }

    protected override void OnHide()
    {
        AudioManager.Instance.PlaySFX(clickClip);
        bigMap.SetActive(false);
        minimap.SetActive(true);
        minimapCamera.orthographicSize = 60f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

}