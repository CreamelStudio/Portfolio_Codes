using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthSystem : MonoBehaviour
{
    public float hp;

    public void Hert(float val)
    {
        hp -= val;
        OnHert();
    }

    private void OnHert()
    {
        Debug.Log("Hert!");
    }
}
