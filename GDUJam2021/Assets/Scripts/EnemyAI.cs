using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using System;

[RequireComponent(typeof(Enemy))]
public class EnemyAI : MonoBehaviour
{
    [Header("Enemy AI")]
    [SerializeField] internal float attackDistance = 0.75f, attackTime = 0.5f, timeBetweenAttacks = 1f;
    [SerializeField] internal float detectionRadius = 5f;
    [SerializeField] internal float minJumpHeight = 0.25f, minJumpDistance = 0.75f;
    [SerializeField] internal bool canAttackInAir = false;
    [SerializeField] internal bool canAttackVertically = false;
    [SerializeField] internal bool canDamagePlayer = false;
    [SerializeField] public List<Transform> patrolPoints;
    [SerializeField] public Sprite idleSprite, attackSprite, deadSprite, heldSprite;

    [SerializeField] private float nextWaypointDistance = 0.25f;

    internal Enemy enemy;
    internal Seeker seeker;
    internal Path path;
    internal Rigidbody2D rb;
    internal Transform target, prevTarget;
    internal PlayerController playerController;
    internal Coroutine attackCoroutine;
    internal int patrolIndex, pathIndex;
    internal float timeToAttack = 0;
    internal bool followingPlayer = false;
    internal bool doneAttacking = true;
    internal bool canAttack = true;
    internal bool reachedEndOfPath;
    internal bool waitForRepath;

    private const float PLAYER_WIDTH = 0.5f;
    private void Awake()
    {
        enemy = GetComponent<Enemy>();
        rb = GetComponent<Rigidbody2D>();
        seeker = GetComponent<Seeker>();
        playerController = FindObjectOfType<PlayerController>();
    }

    private void Start()
    {
        InvokeRepeating("CalculatePath", 0, 0.5f);
        target = patrolPoints[0];
        prevTarget = target;
        enemy.spriteRenderer.sprite = idleSprite;
    }

    internal virtual void CalculatePath()
    {
        if (seeker.IsDone())
            seeker.StartPath(transform.position, target.position, OnPathComplete);
    }

    internal void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            path = p;
            pathIndex = 0;
            waitForRepath = false;
        }
    }

    internal void PauseAI()
    {
        if (attackCoroutine != null)
            StopCoroutine(attackCoroutine);
        canAttack = true;
        doneAttacking = true;
        canDamagePlayer = false;
    }


    #region AI
    internal virtual void Update()
    {
        timeToAttack -= Time.deltaTime;
        if (path == null || enemy.currentEntityState != EntityState.Active) return;
        if (pathIndex >= path.vectorPath.Count)
        {
            reachedEndOfPath = true;
            return;
        }
        else reachedEndOfPath = false;


        float distanceToPlayer = SelectTarget();
        CheckPreviousTarget();

        if (doneAttacking)
        {
            if (followingPlayer)
            {
                enemy.spriteRenderer.sprite = attackSprite;
                if (distanceToPlayer < attackDistance && timeToAttack <= 0)
                    Attack();
            }
            else
            {
                enemy.spriteRenderer.sprite = idleSprite;
                Patrol();
            }
            EnemyMovement();
        }


        float distance = Vector2.Distance(rb.position, path.vectorPath[pathIndex]);
        if (distance < nextWaypointDistance && pathIndex < path.vectorPath.Count)
            pathIndex++;        
    }

    internal void SetHeldSprite()
    {
        enemy.spriteRenderer.sprite = heldSprite;
    }
    internal void SetDeadSprite()
    {
        enemy.spriteRenderer.sprite = deadSprite;
    }

    private float SelectTarget()
    {
        float distanceToPlayer = Vector2.Distance(playerController.transform.position, transform.position);
        if (distanceToPlayer < detectionRadius)
        {
            followingPlayer = true;
            target = playerController.transform;
        }
        else
        {
            followingPlayer = false;
            target = patrolPoints[patrolIndex];
        }

        return distanceToPlayer;
    }

    private bool CheckPreviousTarget()
    {
        if (prevTarget != target)
        {
            pathIndex = 0;
            prevTarget = target;
            return true;

        }
        else return false;
    }

    internal virtual void EnemyMovement()
    {
        enemy.input = ((Vector2)path.vectorPath[pathIndex] - rb.position).normalized;
        enemy.SetMovementVector();
        Jump();
    }

    private void Patrol()
    {
        if (followingPlayer) return;
        if (Vector2.Distance(rb.position, target.position) <= nextWaypointDistance) 
        {
            patrolIndex++;
            patrolIndex = patrolIndex % patrolPoints.Count;
            target = patrolPoints[patrolIndex];
        }
    }

    private void Jump()
    {
        float yDiff = path.vectorPath[pathIndex].y - rb.position.y;
        float xDiff = Mathf.Abs(path.vectorPath[pathIndex].x - rb.position.x);

        if (target.position.y < rb.position.y) waitForRepath = true;

        if (yDiff >= minJumpHeight && xDiff < minJumpDistance && !waitForRepath)
        {
            enemy.Jump();
        }
    }

    public void Attack()
    {
        if (!canAttack) return;
        if (!canAttackInAir && !enemy.isGrounded) return;
        if (attackCoroutine != null)
            StopCoroutine(attackCoroutine);
        attackCoroutine = StartCoroutine(AttackCoroutine());
    }

    internal virtual IEnumerator AttackCoroutine()
    {
        doneAttacking = false;
        canAttack = false;
        canDamagePlayer = true;
        rb.velocity = Vector2.zero;
        enemy.ResetVelocityAndInput();
        //float sign = Mathf.Sign(enemy.input.x);
        Vector2 originalPosition = transform.position;
        float x = playerController.transform.position.x;
        float y = (canAttackVertically) ? playerController.transform.position.y : transform.position.y;
        Vector2 attackOffset = new Vector2(x, y);// - PLAYER_WIDTH * Vector2.right;//(Vector2)transform.position + (sign * attackDistance * Vector2.right).normalized * attackDistance;
        Tween myTween = rb.DOMove(attackOffset, attackTime);
        yield return myTween.WaitForCompletion();
        canDamagePlayer = false;
        myTween = rb.DOMove(originalPosition, attackTime / 2f);
        yield return myTween.WaitForCompletion();
        timeToAttack = timeBetweenAttacks;
        rb.velocity = Vector2.zero;
        yield return new WaitForSeconds(1f);
        canAttack = true;
        doneAttacking = true;
    }

    internal virtual void Damage()
    {
        if (canDamagePlayer)
            playerController.Damage(1, true);
    }
    #endregion

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.GetComponent<PlayerController>())
            Damage();
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackDistance);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, nextWaypointDistance);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
