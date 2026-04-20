using UnityEngine;

public class IconController : MonoBehaviour
{
    public Transform targetPosition;
    public float height;

    private void Start()
    {
        if (targetPosition == null)
        {
            try
            {
                targetPosition = GameManager.Instance.characterController.transform;
            }
            catch (System.Exception ex)
            {
                return;
            }

            if (targetPosition == null)
            {
                Debug.LogError("characterPosition disappear");
            }
        }
    }

    private void LateUpdate()
    {
        if (targetPosition == null)
        {
            try
            {
                targetPosition = GameManager.Instance.characterController.transform;
            }
            catch (System.Exception ex)
            {
                return;
            }

            if (targetPosition == null)
            {
                Debug.LogError("characterPosition disappear");
            }
        }

        transform.position = new Vector3(targetPosition.position.x, height, targetPosition.position.z);
        transform.rotation = Quaternion.Euler(0, targetPosition.eulerAngles.y + 180f, 0);
    }
}
