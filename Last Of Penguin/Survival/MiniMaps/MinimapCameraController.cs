using UnityEngine;

public class MinimapCameraController : MonoBehaviour
{
    public Transform characterPosition;
    public float height = 100f;

    public float heightMax;
    public float heightMin;
    public float wheelSpeed;

    private void Start()
    {
        if (characterPosition == null)
        {
            try
            {
                characterPosition = GameManager.Instance.characterController.transform;
            }
            catch (System.Exception ex)
            {
                return;
            }

            if (characterPosition == null)
            {
                Debug.LogError("characterPosition disappear");
            }
        }
    }

    private void LateUpdate()
    {
        if (characterPosition == null)
        {
            try
            {
                characterPosition = GameManager.Instance.characterController.transform;
            }
            catch (System.Exception ex)
            {
                return;
            }

            if (characterPosition == null)
            {
                Debug.LogError("characterPosition disappear");
            }
        }
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f && Input.GetKey(KeyCode.LeftControl))
        {
            height -= scroll * wheelSpeed;
            height = Mathf.Clamp(height, heightMin, heightMax);
        }

        if (characterPosition != null)
        {
            transform.position = new Vector3(characterPosition.position.x, height, characterPosition.position.z);
            transform.rotation = Quaternion.Euler(90f, 0, 0f);
        }
    }
}
