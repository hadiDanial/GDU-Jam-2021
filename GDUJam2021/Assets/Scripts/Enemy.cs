using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : Entity, IHookable
{
    [Header("Enemy")]
    [SerializeField] private int enemyLayer = 9, heldEnemyLayer = 10;
    [SerializeField] Transform originalParent;

    public Vector2 GetPosition()
    {
        return transform.position;
    }

    //public void Hold(Transform holdPosition)
    //{
    //    transform.position = holdPosition.position;
    //}

    public void Hook(Transform parent)
    {
        rb.isKinematic = true;
        rb.velocity = Vector2.zero;
        transform.SetParent(parent);
        gameObject.layer = heldEnemyLayer;
        currentEntityState = EntityState.Inactive;
    }

    public void SetPosition(Vector2 position)
    {
        transform.position += (Vector3) position;
    }

    public void Throw(Vector2 direction)
    {
        transform.SetParent(originalParent);
        rb.isKinematic = false;
        rb.velocity = direction;
        Invoke("ResetLayer", 0.3f);
        currentEntityState = EntityState.Active;
    }

    public Rigidbody2D GetRB()
    {
        return rb;
    }
    private void ResetLayer()
    {
        gameObject.layer = enemyLayer;
    }

}
