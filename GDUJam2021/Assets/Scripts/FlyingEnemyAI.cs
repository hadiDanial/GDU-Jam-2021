using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingEnemyAI : EnemyAI
{
    [SerializeField] private float verticalOffsetDistance = 1.5f;
    [SerializeField] private float horizontalOffsetDistance = 2f;
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
        //rb.velocity = Vector2.zero;
        enemy.ResetVelocityAndInput();
        //float x = playerController.transform.position.x;
        //float y = playerController.transform.position.y;
        //Vector2 attackOffset = new Vector2(x, y);
        //Vector2[] path = new Vector2[3];
        //path[0] = transform.localPosition;
        //path[1] = attackOffset;
        //path[2] = attackOffset - path[0];
        //Tween myTween = rb.DOLocalPath(path, attackTime, PathType.CubicBezier, PathMode.Sidescroller2D, 10, Color.green);
        //yield return myTween.WaitForCompletion();
        doneAttacking = true;
        timeToAttack = timeBetweenAttacks;
        rb.isKinematic = false;
        yield return new WaitForSeconds(1f);
        canDamagePlayer = false;
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
}
