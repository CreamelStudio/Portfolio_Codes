using UnityEngine;

public class PivotRotationPreview : MonoBehaviour
{
    public float RotationSpeed;
    void Update()
    {
        RotationPivot();
    }

    public void RotationPivot()
    {
        RotationSpeed = SnowmanDecoration.Instance.snowmanRotationSpeed;
            
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        transform.localRotation *= Quaternion.Euler(new Vector3(v, -h, 0) * RotationSpeed * Time.deltaTime);
    }
}
