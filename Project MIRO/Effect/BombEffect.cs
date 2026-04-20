using FMODUnity;
using UnityEngine;

public class BombEffect : MonoBehaviour
{
    public EventReference bombSound;

    void Start()
    {
        RuntimeManager.PlayOneShot(bombSound, transform.position);
        Destroy(gameObject, 1f);
    }
}
