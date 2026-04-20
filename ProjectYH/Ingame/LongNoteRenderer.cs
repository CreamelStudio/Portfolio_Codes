using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LongNoteRenderer : MonoBehaviour
{
    public Transform target;
    public TrailRenderer trail;
    public bool isFollow;

    public void StartFollow(float duration)
    {
        trail = GetComponent<TrailRenderer>();
        trail.time = duration;
        isFollow = true;
    }

    public void StopFollow()
    {
        isFollow = false;
    }

    public void Destroy()
    {
        Destroy(gameObject);
    }

    private void Update()
    {
        if (isFollow)
        {
            transform.position = target.position;
        }
    }
}
