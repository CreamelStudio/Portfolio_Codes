using GlobalAudio;
using Island;
using System.Collections.Generic;
using System.ComponentModel.Design;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public partial class CharacterController
{
    [Header("Movement Settings")]
    [SerializeField] private float baseMoveSpeed = 5f;                          // �⺻ �̵� �ӵ�
    private float currentSpeed;                                                 // ���� �̵� �ӵ�
    [SerializeField] private float jumpForce = 9f;                              // ���� ��
    [SerializeField] private Collider bodyCollider;                             // ��� ��ü �ݶ��̴�
    [SerializeField] private PhysicsMaterial jumpPhysicsMaterial;               // ���� �� ����Ǵ� ���� ����
    [SerializeField] private Transform shaddowTransform;
    private Vector3 moveDir;                                                    // �̵� ����
    private Vector2 inputVec;                                                   // �Է� ����
    public bool isGrounded;                                                     // ���鿡 ����ִ��� ����
    private float walkAnimationTimer = 0.4f;

    private float coyoteTime = 0.2f;
    private float coyoteTimeCounter;

    [Header("Sprint Settings")]
    [SerializeField] private float sprintMultiplier = 1.6f;                     // �޸��� ���
    [SerializeField] private float speedLerp = 10f;                             // ������ �ε巴��
    [SerializeField] private float normalFOV;                                   // �⺻ FOV ��
    [SerializeField] private float sprintFOV;                                   // �޸��� FOV ��
    [SerializeField] private float zoomFov;
    [SerializeField] private float fovLerpSpeed;                                // FOV ���� �ӵ�
    [SerializeField] private float currentFOV;                                                   // ���� FOV ��
    private bool isSprint = false;                                              // �޸��� ����
    private float moveSpeedDebuffMultiplier = 1f;                               // �̵��ӵ� ����� �̵��ӵ� ���ҷ� / 1f = ����� ����
    [SerializeField]
    private GameObject groundCheckPos;

    #region �̵�
    public void OnMove(InputAction.CallbackContext context)
    {
        inputVec = context.ReadValue<Vector2>();
        if (isTent && inputVec != Vector2.zero)
        {
            EndTentInteractive();
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (characterState == CharacterState.Dead || characterState == CharacterState.UsingUI)
        {
            return;
        }
        anim.ResetTrigger(CharacterAnimatorParamMapper.EndFishing);
        if (context.performed && coyoteTimeCounter > 0.0f && !isInWater && !isFishing)
        {
            Debug.Log("점프 함수");
            if (LOPNetworkManager.Instance.isConnected && networkIdentity.IsOwner)
            {
                anim.SetTrigger(CharacterAnimatorParamMapper.Jump);
                LOPNetworkManager.Instance.SendAnimationTrigger(networkIdentity.NetworkId, CharacterAnimatorParamMapper.Jump);
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, rb.linearVelocity.z);
            }
            else if (!LOPNetworkManager.Instance.isConnected)
            {
                anim.SetTrigger(CharacterAnimatorParamMapper.Jump);
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, rb.linearVelocity.z);
            }
        }
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        if (context.performed) isSprint = true;
        else if (context.canceled) isSprint = false;
    }

    /// <summary>
    /// �̵� �Լ�
    /// </summary>
    private void HandleMovement()
    {
        if (characterState == CharacterState.Dead || characterState == CharacterState.UsingUI || isFishing || isTent)
        {
            inputVec = Vector3.zero; // �װų� UI ��� �߿��� ������ ����
        }

        // ī�޶� ���� ���� ���
        Vector3 camForward = ActiveCamera.transform.forward;
        Vector3 camRight = ActiveCamera.transform.right;
        camForward.y = 0;
        camRight.y = 0;

        // �Է� ����
        moveDir = (camRight * inputVec.x + camForward * inputVec.y).normalized;

        // ��� �����̸� ������Ʈ ����
        if (!isSprintEnabled) isSprint = false;

        // ��ǥ �ӵ�
        float targetSpeed = baseMoveSpeed * moveSpeedDebuffMultiplier * (isSprint ? sprintMultiplier : 1f) * temperatureSpeedMultiplier; // Add temperatureSpeedMultiplier

        // ������ �ӵ� ���
        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.fixedDeltaTime * speedLerp);

        // ���� �ӵ� ����
        Vector3 velocity = moveDir * currentSpeed;
        velocity.y = rb.linearVelocity.y; // �߷�/���� ����

        // Rigidbody �ӵ� ����
        rb.linearVelocity = velocity;
    }

    private void OnFinishJump()
    {
        if (Keyboard.current.spaceKey.wasReleasedThisFrame)
        {
            coyoteTimeCounter = 0.0f;
        }
    }

    /// <summary>
    /// ���� Ȯ�� �Լ�
    /// </summary>
    private void GroundCheck()
    {
        Vector3 origin = transform.position + Vector3.up * 0.3f;
        isGrounded = Physics.BoxCast(origin, transform.localScale / 3, Vector3.down, out RaycastHit hit, transform.rotation, groundCheckDistance, groundLayers);
    }

    private void CorrectDownwardVelocity()
    {
        if (isGrounded && rb.linearVelocity.y < 0f)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, -2f, rb.linearVelocity.z);
        }
    }

    private void HandleWaterClimb()
    {
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, 1.5f, groundLayers))
        {
            rb.AddForce(Vector3.up, ForceMode.VelocityChange);
        }
    }

    public void WalkSound()
    {
        if (LOPNetworkManager.Instance.isConnected && !networkIdentity.IsOwner) return;
        Vector3 checkPos = this.transform.position + Vector3.down * 0.5f;
        if (map.IsVoxelInMap(checkPos))
        {
            BlockData block = map.GetBlockInChunk(checkPos, ChunkType.Water);
            switch (block.id)
            {
                case BlockConstants.Air:
                    break;
                case BlockConstants.Snow:
                    AudioPlayer.PlaySound(penguinAudioObject, snowWalkSFX, 1f);
                    break;
                case BlockConstants.Ice:
                    AudioPlayer.PlaySound(penguinAudioObject, iceWalkSFX, 1f);
                    break;
                default:
                    AudioPlayer.PlaySound(penguinAudioObject, dirtWalkSFX, 1f);
                    break;
            }
        }
    }

    private void ShaddowMovement()
    {
        if (Physics.Raycast(transform.position + Vector3.up, Vector3.down, out RaycastHit hit, Mathf.Infinity, groundLayers))
        {
            shaddowTransform.position = hit.point + Vector3.up * 0.07f;
        }
    }

    #endregion
}