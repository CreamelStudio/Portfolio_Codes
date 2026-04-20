using UnityEngine;
using FMODUnity;
public class GunController : MonoBehaviour
{
    public EventReference shootSound;

    public GameObject bulletPrefab; // �Ѿ� ������
    public Transform firePoint;     // �ѱ� ��ġ
    public float bulletSpeed = 10f;

    void Update()
    {
        RotateToMouse();

        if (Input.GetMouseButtonDown(0)) // ���콺 ���� Ŭ��
        {
            Shoot();
        }
    }

    void RotateToMouse()
    {
        // ���콺 ��ǥ �� ���� ��ǥ ��ȯ
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = mousePos - transform.position;

        // ȸ���� ���
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    void Shoot()
    {
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        RuntimeManager.PlayOneShot(shootSound, transform.position);

        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        rb.linearVelocity = firePoint.right * bulletSpeed; // �ѱ� ���� ���� �߻�
    }
}
