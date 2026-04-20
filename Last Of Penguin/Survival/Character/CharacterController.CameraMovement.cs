using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public partial class CharacterController
{
    [Header("Camera Controllers")]
    [SerializeField] private FirstPersonController firstPersonController;       // 1��Ī ī�޶� ��Ʈ�ѷ�
    [SerializeField] private ThirdPersonController thirdPersonController;       // 3��Ī ī�޶� ��Ʈ�ѷ�
    public Camera firstPersonCamera;                          // 1��Ī ī�޶�
    public Camera thirdPersonCamera;                          // 3��Ī ī�޶�
    public GameObject fogV1;
    public GameObject fogV3;
    public GameObject waterPlane;
    private Camera mainCam;                                                     // ���� Ȱ��ȭ�� ī�޶�
    public Camera MainCam => mainCam;                                           // ���� Ȱ��ȭ�� ī�޶� ������Ƽ
    private bool isFirstPerson = true;                                          // ���� 1��Ī ������� ����

    [Header("FirstPersonItemModel")]
    [SerializeField] private List<ToolItemList> toolItemModelList;              // ������ �𵨸� List
    private ToolItemType currentToolType = ToolItemType.None;                   // ���� ������ enum
    [SerializeField] private SpriteRenderer tphandleItem;                       // 3��Ī ������ �ڵ�
    [SerializeField] private SpriteRenderer fphandleItem;                       // 1��Ī ������ �ڵ�
    private GameObject currentTpItemObject;                                     // ���� ������ 3��Ī ������Ʈ
    public Transform firstPersonRootTransform;

    private Camera ActiveCamera => isFirstPerson ? firstPersonCamera : thirdPersonCamera;

    #region ī�޶� ����
    /// <summary>
    /// ���� ��ȯ �Լ�
    /// </summary>
    private void HandleCameraControl()
    {
        firstPersonController.RotateCamera();
        thirdPersonController.RotateCamera();
    }

    private void SprintCameraFov()
    {
        float targetFOV = isSprint ? sprintFOV : normalFOV;
        currentFOV = Mathf.Lerp(currentFOV, targetFOV, Time.deltaTime / fovLerpSpeed);
        firstPersonCamera.fieldOfView = currentFOV;
        thirdPersonCamera.fieldOfView = currentFOV;
    }

    private void CheckWaterCam()
    {
        Vector3 rootPos = isInWater ? Vector3.up * 0.5f : Vector3.up * 1.5f;
        bool activeItemObj = isInWater ? false : true;
        firstPersonController.fpItemCamera.SetActive(activeItemObj);
        firstPersonRootTransform.transform.localPosition = Vector3.Lerp(firstPersonRootTransform.transform.localPosition, rootPos, Time.deltaTime / 0.3f);
    }

    public void ZoomCameraFov()
    {
        if (Keyboard.current.cKey.isPressed && isFirstPerson)
        {
            currentFOV = Mathf.Lerp(currentFOV, zoomFov, Time.deltaTime / fovLerpSpeed);
            firstPersonCamera.fieldOfView = currentFOV;

            firstPersonController.BackWardItemCamera(fovLerpSpeed);
        }
        else if (Keyboard.current.cKey.wasReleasedThisFrame)
        {
            firstPersonController.RestoreItemCamera();
        }
    }

    public void TogglePerspective()
    {
        isFirstPerson = !isFirstPerson;
        QuickslotNumberBtn.Instance.cantChange = false;
        SetupCameras();
    }

    /// <summary>
    /// ���� ��ȯ �� ī�޶� on/off && �ʱ�ȭ �Լ�
    /// </summary>
    private void SetupCameras()
    {
        firstPersonCamera.gameObject.SetActive(isFirstPerson);
        thirdPersonCamera.gameObject.SetActive(!isFirstPerson);

        if (isFirstPerson)
        {
            mainCam = firstPersonCamera;
            firstPersonController.Init(firstPersonCamera, penguinObject);
        }
        else
        {
            mainCam = thirdPersonCamera;
            thirdPersonController.Init(thirdPersonCamera, penguinObject);
        }
    }

    #endregion
}
