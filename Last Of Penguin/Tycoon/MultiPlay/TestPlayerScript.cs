using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TestPlayerScript : MonoBehaviour
{
    public Vector3 inputVelocity;
    public float moveSpeed;
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

    }

    private void Update()
    {
        rb.linearVelocity = inputVelocity;
    }

    public virtual void OnMove(InputAction.CallbackContext context)
    {
        Vector2 moveInput = context.ReadValue<Vector2>();
        inputVelocity = new Vector3(moveInput.x, 0, moveInput.y) * moveSpeed; // �ӵ� �ݿ�
        if (context.performed) Debug.Log("�����̴���");
    }

    public void Move()
    {
        Vector3 currentVelocity = rb.linearVelocity;
        currentVelocity.y = rb.linearVelocity.y;
        rb.linearVelocity = Vector3.Lerp(currentVelocity, inputVelocity, 0.07f);
    }
}
