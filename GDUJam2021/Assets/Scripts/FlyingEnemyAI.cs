using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingEnemyAI : EnemyAI
{
    [Header("Flying")]
    [SerializeField] private float verticalOffsetDistance = 1.5f;
    [SerializeField] private float horizontalOffsetDistance = 2f;
    [SerializeField] private float attackEndDistance = 3f;
    internal override void CalculatePath()
    {
        if (seeker.IsDone())
        {
            if(target == playerController.transform)
            {
                Vector2 offset = Vector2.up * verticalOffsetDistance - 
                                 Vector2.right * playerController.sign * horizontalOffsetDistance;
                seeker.StartPath(transform.position, (Vector2)target.position + offset, OnPathComplete);
            }
            else
                seeker.StartPath(transform.position, target.position, OnPathComplete);
        }
    }

    internal override IEnumerator AttackCoroutine()
    {
        doneAttacking = false;
        canAttack = false;
        canDamagePlayer = true;
        rb.isKinematic = true;
        rb.mass = 1;
        enemy.ResetVelocityAndInput();
        float x = playerController.transform.position.x;
        float y = playerController.transform.position.y;
        Vector2 attackOffset = new Vector2(x, y);
        Vector2[] path = new Vector2[3];
        path[0] = transform.localPosition;
        path[1] = attackOffset;
        path[2] = new Vector2(transform.position.x + enemy.sign * attackEndDistance + attackOffset.x, transform.position.y);
        Tween myTween = rb.DOLocalPath(path, attackTime, PathType.CatmullRom, PathMode.Sidescroller2D, 10, Color.green);
        yield return myTween.WaitForCompletion();
        canDamagePlayer = false;
        timeToAttack = timeBetweenAttacks;
        enemy.col.enabled = true;
        rb.isKinematic = false;
        rb.mass = enemy.mass;
        yield return new WaitForSeconds(pauseAfterAttackTime);
        doneAttacking = true;
        canAttack = true;
    }
    internal override void EnemyMovement()
    {
        if (doneAttacking)
        {
            enemy.input = ((Vector2)path.vectorPath[pathIndex] - rb.position).normalized;
            enemy.SetMovementVector();
        }
    }
    internal override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        Gizmos.color = Color.green;
        if(enemy!=null)
        Gizmos.DrawLine(transform.position, (Vector2)transform.position + Vector2.right * enemy.sign * horizontalOffsetDistance);
    }
    internal override void OnCollisionEnter2D(Collision2D collision)
    {
        base.OnCollisionEnter2D(collision);
        if(collision.gameObject.GetComponent<PlayerController>())
        {
            enemy.col.enabled = false;
        }
    }
}
