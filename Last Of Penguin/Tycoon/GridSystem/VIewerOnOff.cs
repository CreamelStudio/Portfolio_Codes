using UnityEngine;
using UnityEngine.EventSystems;

public class ViewerOnOff : MonoBehaviour
{
    [SerializeField] private GameObject viewer;

    private Camera mainCamera;
    private int floorLayerMask;
    private bool isViewerVisible;

    private void Awake()
    {
        mainCamera = Camera.main;
        floorLayerMask = LayerMask.GetMask("Floor");

        if (viewer != null)
        {
            isViewerVisible = viewer.activeSelf;
        }
    }

    private void Update()
    {
        if (viewer == null || mainCamera == null)
        {
            return;
        }

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition); // 마우스 위치에서 레이를 쏜다.
        bool hitFloor = Physics.Raycast(ray, out _, Mathf.Infinity, floorLayerMask);
        bool isPointerOverUi = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        bool shouldShowViewer = hitFloor && !isPointerOverUi;

        if (shouldShowViewer == isViewerVisible)
        {
            return;
        }

        viewer.SetActive(shouldShowViewer);
        isViewerVisible = shouldShowViewer;
    }
}
