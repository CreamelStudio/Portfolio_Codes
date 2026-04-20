using UnityEngine;

public class CameraPlayerTracker : MonoBehaviour
{
    public Transform player;
    public Vector3 offset;

    public float minYPos = -1f;
    public float zPos = -10f;
    public float smoothSpeed = 0.125f;

    void LateUpdate()
    {
        if (player != null)
        {
            Vector3 newPosition = player.position + offset;
            newPosition.y = Mathf.Max(newPosition.y, minYPos);
            newPosition.z = zPos;
            transform.position = Vector3.Lerp(transform.position, newPosition, smoothSpeed);
        }
    }
}
