using UnityEngine;
using UnityEngine.UI;

public class TerrainCamera : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Slider cameraSensSlider;
    [SerializeField] private Slider moveSpeedSlider;
    [SerializeField] private Slider runSpeedSlider;

    [Header("Other System")]
    [SerializeField] private Terrain baseTerrain;
    [SerializeField] private Vector3 Offset;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float fastMoveSpeed;
    [SerializeField] private float mouseSpeed;

    private float mouseY;
    private float mouseX;

    private float translationX;
    private float translationY;

    private Camera mainCamera;
    private Transform mainCameraTransform;

    [SerializeField]
    private Vector3 startCameraRotation;

    private void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogWarning("메인 카메라를 찾지 못해 TerrainCamera를 초기화할 수 없습니다.");
            enabled = false;
            return;
        }

        mainCameraTransform = mainCamera.transform;
        AdjustCameraPosition();
        Vector3 currentRotation = mainCameraTransform.eulerAngles;
        mouseX = currentRotation.y;
        mouseY = currentRotation.x;
    }

    private void SetSettings()
    {
        mouseSpeed = cameraSensSlider.value;
        moveSpeed = moveSpeedSlider.value;
        fastMoveSpeed = runSpeedSlider.value;
    }

    private void FixedUpdate()
    {
        SetSettings();
        CameraKeyMove();
    }

    private void Update()
    {
        if (Input.GetMouseButton(1)) CameraRotate();

    }

    void AdjustCameraPosition()
    {
        Vector3 terrainSize = baseTerrain.terrainData.size;
        Vector3 terrainCenter = new Vector3(baseTerrain.transform.position.x + terrainSize.x / 2, baseTerrain.transform.position.y, baseTerrain.transform.position.z + terrainSize.z / 2);

        float distanceToCenter = Mathf.Max(terrainSize.x, terrainSize.z) * 0.7f;
        float cameraHeight = distanceToCenter / Mathf.Tan(45 * Mathf.Deg2Rad);

        mainCameraTransform.position = new Vector3(terrainCenter.x, terrainCenter.y + cameraHeight, terrainCenter.z - distanceToCenter) + Offset;
        mainCameraTransform.rotation = Quaternion.Euler(45f, 0f, 0f);
        mainCameraTransform.LookAt(terrainCenter + Offset);
    }

    void CameraKeyMove()
    {
        translationX = Input.GetAxisRaw("CameraHorizontal");
        translationY = Input.GetAxisRaw("CameraVertical");
        int spaceInput = Input.GetKey(KeyCode.Space) ? 1 : 0;
        int ShiftInput = Input.GetKey(KeyCode.LeftShift) ? -1 : 0;
        Vector3 moveVec = new Vector3();
        if (Input.GetKey(KeyCode.LeftControl)) moveVec = new Vector3(translationX * fastMoveSpeed, (spaceInput) * fastMoveSpeed, translationY * fastMoveSpeed);
        else moveVec = new Vector3(translationX * moveSpeed, ((spaceInput) * moveSpeed) + ((ShiftInput) * moveSpeed), translationY * moveSpeed);
        mainCameraTransform.position += transform.TransformDirection(moveVec);

    }
    void CameraRotate()
    {
        mouseX += Input.GetAxis("Mouse X") * mouseSpeed;
        mouseY -= Input.GetAxis("Mouse Y") * mouseSpeed;
        this.transform.localEulerAngles = new Vector3(mouseY, mouseX, 0) + startCameraRotation;
    }
}
