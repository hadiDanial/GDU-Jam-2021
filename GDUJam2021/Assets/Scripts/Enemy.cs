using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EnemyAI))]
public class Enemy : Entity, IHookable
{
    [Header("Enemy Layers")]
    [SerializeField] private int enemyLayer = 9;
    [SerializeField] private int heldEnemyLayer = 10;
    [SerializeField] private Transform originalParent = null;
    [SerializeField] bool isHeld = false;

    private EnemyAI enemyAI;

    internal override void Awake()
    {
        base.Awake();
        enemyAI = GetComponent<EnemyAI>();
    }

    internal void Update()
    {
        transform.rotation = Quaternion.identity;
        
    }

    #region HOOK_BEHAVIOR
    public Vector2 GetPosition()
    {
        return transform.position;
    }
    public void Hook(Transform parent)
    {
        rb.isKinematic = true;
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0;
        transform.SetParent(parent);
        gameObject.layer = heldEnemyLayer;
        transform.localPosition = Vector2.zero;
        spriteRenderer.color = heldColor;
        currentEntityState = EntityState.Inactive;
        enemyAI.PauseAI();
        CancelInvoke();
    }
    public void Hold()
    {
        isHeld = true;
    }
    public void SetPosition(Vector2 position)
    {
        transform.position += (Vector3) position;
    }
    public void Throw(Vector2 direction)
    {
        
        //transform.position = transform.localToWorldMatrix * transform.localPosition;
        transform.SetParent(originalParent);
        rb.isKinematic = false;
        rb.velocity = direction;
        Invoke("ResetLayer", 1f);
    }

    public Rigidbody2D GetRB()
    {
        return rb;
    }
    private void ResetLayer()
    {
        gameObject.layer = enemyLayer;
        isHeld = false;
        spriteRenderer.color = normalColor;
        currentEntityState = EntityState.Active;

    }
    #endregion

    internal override void OnCollisionEnter2D(Collision2D collision)
    {
        base.OnCollisionEnter2D(collision);
        GameObject other = collision.gameObject;
        Entity entity = other.GetComponent<Entity>();
        if (!isHeld) return;
        if ((enemyMask & 1 << other.layer) != 0)
        {
            if (entity != null)
            {
                entity.Damage(1);
            }
            Damage(1);
        }
    }

    private void OnDestroy()
    {
        if (isHeld)
            FindObjectOfType<HookController>().Clear();
    }
}
