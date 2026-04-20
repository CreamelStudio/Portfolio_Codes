using UnityEngine;
using UnityEngine.EventSystems;
public class EditorCameraMove : MonoBehaviour
{
    public float moveSpeed;
    public float wheelSpeed;
    float orthographicSize;

    private void Start()
    {
        orthographicSize = Camera.main.orthographicSize;
    }

    void Update()
    {
        float v = Input.GetAxisRaw("Vertical");
        float h = Input.GetAxisRaw("Horizontal");

        transform.position += new Vector3(h, v, 0).normalized * Time.deltaTime * moveSpeed;

        if (EventSystem.current.IsPointerOverGameObject()) return;

        float mouseWheel = Input.GetAxis("Mouse ScrollWheel");
        orthographicSize += mouseWheel * Time.deltaTime * wheelSpeed;
        orthographicSize = Mathf.Clamp(orthographicSize, 0.1f, 500);
        Camera.main.orthographicSize = orthographicSize;
    }
}
