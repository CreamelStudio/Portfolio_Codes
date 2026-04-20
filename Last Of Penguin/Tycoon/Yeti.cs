using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.Rendering.VolumeComponent;

public class Yeti : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Transform playerPos;
    [SerializeField] private GameObject particleEffect;
    [SerializeField] private ParticleSystem hitParticles;
    [SerializeField] private GameObject panicParticleObj;
    [SerializeField] private ParticleSystem panicParticle;

    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float turnAngleMin = 90f;
    public float turnAngleMax = 180f;
    public float detectionRadius = 10f;

    private Vector3 moveDirection;
    private Animator animator;
    private Transform foodTarget;

    private bool isChasingFood = false;
    private bool isMovementStopped = false;
    private bool isRotationStopped = false;
    private bool canAttachToPlayer = true;
    private bool isRunAway = false;

    private void Start()
    {
        animator = GetComponent<Animator>();
        agent.speed = moveSpeed;
        moveDirection = transform.forward;
        StartCoroutine(ChangeDirectionRandomly());
    }

    private void Update()
    {
        if (isChasingFood)
        {
            HandleFoodChase();
        }
        else if (!isMovementStopped)
        {
            RandomMovement();
            DetectAndChaseFood();
        }

        if (isRunAway)
        {
            RandomMovement();
        }
    }

    private void HandleFoodChase()
    {
        if (foodTarget == null || !foodTarget.GetComponent<SupplyIngredients>().isThrow)
        {
            StopChasingFood();
            return;
        }
        agent.SetDestination(foodTarget.position);
    }

    private void RandomMovement()
    {
        if (!agent.hasPath)
        {
            transform.position += moveDirection * moveSpeed * Time.deltaTime;
        }
    }

    private IEnumerator ChangeDirectionRandomly()
    {
        while (true)
        {
            if (isChasingFood || isMovementStopped)
            {
                yield return null;
                continue;
            }

            yield return new WaitForSeconds(Random.Range(3f, 5f));

            float randomYRotation = Random.Range(turnAngleMin, turnAngleMax);
            transform.Rotate(0, randomYRotation, 0);
            moveDirection = transform.forward;
            animator.SetBool("isMove", true);
        }
    }

    private void DetectAndChaseFood()
    {
        if (isMovementStopped || isRotationStopped) return;

        foreach (var collider in Physics.OverlapSphere(transform.position, detectionRadius))
        {
            if (collider.CompareTag("Food") && collider.GetComponent<SupplyIngredients>().isThrow)
            {
                foodTarget = collider.transform;
                isChasingFood = true;
                animator.SetBool("isMove", true);
                return;
            }
        }
    }

    private void StopChasingFood()
    {
        isChasingFood = false;
        foodTarget = null;
        agent.ResetPath();
        animator.SetBool("isMove", false);
        StartCoroutine(ChangeDirectionRandomly());
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Food"))
        {
            StartCoroutine(HandleFoodCollision(collision.gameObject));
        }
        else if (collision.gameObject.CompareTag("Player") && canAttachToPlayer)
        {
            if (Random.value <= 1f / 6f)
            {
                StartCoroutine(MovePlayerForSeconds(collision.transform, 2f));
                StartCoroutine(ActivateEffectForSeconds(particleEffect, 2f));
                StartCoroutine(PreventReattachment(3f));
            }
            else{
                playerPos = collision.transform;
                StartCoroutine(RunAwayEnable(0.6f, Random.Range(1f, 3f)));
                transform.LookAt(playerPos);
                transform.Rotate(0, 180, 0);
                moveDirection = transform.forward;
                animator.SetBool("isMove", true);

            }
        }
        else if (!collision.gameObject.CompareTag("Ground"))
        {
            StopMovement();
        }
    }

    private void StopMovement()
    {
        moveDirection = Vector3.zero;
        animator.SetBool("isMove", false);
    }

    private IEnumerator HandleFoodCollision(GameObject food)
    {
        animator.SetTrigger("isEat");
        agent.enabled = false;
        isMovementStopped = true;
        isRotationStopped = true;
        isChasingFood = false;
        foodTarget = null;

        yield return new WaitForSeconds(0.6f);
        Destroy(food);
        yield return ReactivateAgentAfterDelay(2.6f);
    }

    private IEnumerator ReactivateAgentAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        agent.enabled = true;
        isMovementStopped = false;
        isRotationStopped = false;
    }

    private IEnumerator MovePlayerForSeconds(Transform playerTransform, float duration)
    {
        float elapsedTime = 0f;
        var playerScript = playerTransform.GetComponent<Character>();
        playerScript.isMove = false;

        while (elapsedTime < duration)
        {
            transform.position = playerTransform.position + Vector3.up;
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        playerScript.isMove = true;
    }

    private IEnumerator ActivateEffectForSeconds(GameObject effect, float duration)
    {
        effect.SetActive(true);
        yield return new WaitForSeconds(duration);
        effect.SetActive(false);
    }

    private IEnumerator PreventReattachment(float duration)
    {
        canAttachToPlayer = false;
        yield return new WaitForSeconds(duration);
        canAttachToPlayer = true;
    }

    private IEnumerator RunAwayEnable(float duration, float panicDuration)
    {
        StopCoroutine(ChangeDirectionRandomly());

        //Movement Stop
        agent.enabled = false;
        isMovementStopped = true;
        isRotationStopped = true;
        canAttachToPlayer = false;

        //Start RunWay;
        isRunAway = true;

        yield return new WaitForSeconds(duration);
        isRunAway = false;

        //Start Panic;
        panicParticleObj.SetActive(true);
        panicParticle.Play();
        yield return new WaitForSeconds(panicDuration);


        agent.enabled = true;
        isMovementStopped = false;
        isRotationStopped = false;
        canAttachToPlayer = true;
        panicParticle.Stop();
        panicParticleObj.SetActive(false);
        
        //Random Start
        float randomYRotation = Random.Range(turnAngleMin, turnAngleMax);
        transform.Rotate(0, randomYRotation, 0);
        moveDirection = transform.forward;
        animator.SetBool("isMove", true);
        StopCoroutine(ChangeDirectionRandomly());
    }
}
