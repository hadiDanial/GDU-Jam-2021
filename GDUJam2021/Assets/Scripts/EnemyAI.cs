using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

[RequireComponent(typeof(Enemy))]
public class EnemyAI : MonoBehaviour
{
    [Header("Enemy AI")]
    [SerializeField] internal float attackDistance = 0.75f, attackTime = 0.5f, timeBetweenAttacks = 1f;
    [SerializeField] internal float detectionRadius = 5f;
    [SerializeField] internal float minJumpHeight = 0.25f, minJumpDistance = 0.75f;
    [SerializeField] internal bool canAttackVertically = false;
    [SerializeField] internal bool canDamagePlayer = false;
    [SerializeField] public List<Transform> patrolPoints;

    [SerializeField] private float nextWaypointDistance = 0.25f;

    private Enemy enemy;
    private Seeker seeker;
    private Path path;
    private Rigidbody2D rb;
    private Transform target, prevTarget;
    private PlayerController playerController;
    private int patrolIndex, pathIndex;
    private float timeToAttack = 0;
    private bool followingPlayer = false;
    private bool doneAttacking = true;
    private bool canAttack = true;
    private bool reachedEndOfPath;
    private bool waitForRepath;

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
    }

    private void CalculatePath()
    {
        if (seeker.IsDone())
            seeker.StartPath(transform.position, target.position, OnPathComplete);
    }

    void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            path = p;
            pathIndex = 0;
            waitForRepath = false;
        }
    }


    #region AI
    private void Update()
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
                if (distanceToPlayer < attackDistance && timeToAttack <= 0)
                    Attack();
            }
            else
            {
                Patrol();
            }
            EnemyMovement();
        }


        float distance = Vector2.Distance(rb.position, path.vectorPath[pathIndex]);
        if (distance < nextWaypointDistance && pathIndex < path.vectorPath.Count)
            pathIndex++;        
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
        // Check if player is in radius

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
        StartCoroutine(AttackCoroutine());
    }

    public IEnumerator AttackCoroutine()
    {
        doneAttacking = false;
        canAttack = false;
        canDamagePlayer = true;
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
        canAttack = true;
        rb.velocity = Vector2.zero;
        yield return new WaitForSeconds(1f);
        doneAttacking = true;
    }

    private void Damage()
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
