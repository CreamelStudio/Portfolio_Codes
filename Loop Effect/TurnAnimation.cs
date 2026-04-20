using UnityEngine;

public class TurnAnimation : MonoBehaviour
{
    public float rotationSpeed;
    void Update()
    {
        transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
    }
}
