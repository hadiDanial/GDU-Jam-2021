using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

[RequireComponent(typeof(Enemy))]
public class EnemyAI : MonoBehaviour
{
    [Header("Enemy AI")]
    [SerializeField] internal float attackDistance = 0.75f, attackTime = 0.5f;
    [SerializeField] internal float detectionRadius = 5f;
    [SerializeField] internal float minJumpHeight = 0.5f, minJumpDistance = 1;
    [SerializeField] internal bool canDamagePlayer = false;
    [SerializeField] public List<Transform> patrolPoints;

    [SerializeField] private float nextWaypointDistance = 0.25f;

    private Enemy enemy;
    private Seeker seeker;
    private Path path;
    private Rigidbody2D rb;
    private Transform target;
    private bool followingPlayer = false;
    private bool canAttack = true;
    private int patrolIndex, pathIndex;
    private bool reachedEndOfPath;

    private void Awake()
    {
        enemy = GetComponent<Enemy>();
        rb = GetComponent<Rigidbody2D>();
        seeker = GetComponent<Seeker>();
    }

    private void Start()
    {
        InvokeRepeating("CalculatePath", 0, 0.5f);
        target = patrolPoints[0];
    }

    private void CalculatePath()
    {
        if (seeker.IsDone())
            seeker.StartPath(transform.position, patrolPoints[patrolIndex].position, OnPathComplete);
    }

    #region AI
    private void Update()
    {
        if (path == null || enemy.currentEntityState != EntityState.Active) return;
        if (pathIndex >= path.vectorPath.Count)
        {
            reachedEndOfPath = true;
            return;
        }
        else reachedEndOfPath = false;
        enemy.input = ((Vector2)path.vectorPath[pathIndex] - rb.position).normalized;
        enemy.SetMovementVector();
        Jump();
        float distance = Vector2.Distance(rb.position, path.vectorPath[pathIndex]);
        if (distance < nextWaypointDistance)
            pathIndex++;
        // Check if player is in radius

        // Follow player or patrol.
        if (followingPlayer)
        {

        }
        else
        {
            Patrol();
        }
        //else if(path.dis)
    }

    private void Jump()
    {
        float yDiff = path.vectorPath[pathIndex].y  - rb.position.y;
        float xDiff = Mathf.Abs(path.vectorPath[pathIndex].x - rb.position.x);
        
        if (yDiff >= minJumpHeight && xDiff < minJumpDistance)
        {
            enemy.Jump();
        }
    }

    private void Patrol()
    {
        if (Vector2.Distance(rb.position, target.position) <= nextWaypointDistance) 
        {
            patrolIndex++;
            patrolIndex = patrolIndex % patrolPoints.Count;
            target = patrolPoints[patrolIndex];
        }
    }

    void OnPathComplete(Path p)
    {
        if(!p.error)
        {
            path = p;
            pathIndex = 0;
        }
    }
    public void Attack()
    {
        if (!canAttack) return;
        StartCoroutine(AttackCoroutine());
    }
    public IEnumerator AttackCoroutine()
    {
        canAttack = false;
        canDamagePlayer = true;
        float sign = Mathf.Sign(enemy.input.x);
        Vector2 originalPosition = transform.position;
        Vector2 attackOffset = (Vector2)transform.position + (sign * attackDistance * Vector2.right).normalized * attackDistance;
        Tween myTween = rb.DOMove(attackOffset, attackTime);
        yield return myTween.WaitForCompletion();
        canDamagePlayer = false;
        myTween = rb.DOMove(originalPosition, attackTime / 2f);
        yield return myTween.WaitForCompletion();

    }
    #endregion

}
