using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Hook : MonoBehaviour
{
    [SerializeField] private Transform holdPoint;
    [SerializeField] private IHookable target;
    [SerializeField] private float hookDistance = 3f;
    [SerializeField] private float throwSpeed = 2.5f;
    [SerializeField] private float reelTime = 0.75f, backthowTime = 0.75f;
    [SerializeField] private float swingRadius = 2.5f;
    [SerializeField] private float aimRadius = 0.2f;
    [SerializeField] private LayerMask hookLayer;
    [SerializeField] private HookState state;

    private Vector2 aimDirection;
    private Transform playerTransform;
    private Vector3 defaultHoldPosition;
    private Coroutine reelCoroutine;
    bool toHold = false;

    internal void SetPlayer(Transform transform)
    {
        playerTransform = transform;
    }


    private void Start()
    {
        defaultHoldPosition = holdPoint.position;
    }

    internal void SetAim(Vector2 dir)
    {
        aimDirection = dir;
        //if (state == HookState.Held)
        //    target.SetPosition((Vector2)transform.position + aimDirection.normalized * swingRadius);
    }

    internal void HookObject()
    {
        if (state == HookState.Empty)
        {
            toHold = true;
            GrabObject(reelTime);
        }
        else if(state == HookState.Held || state == HookState.Swinging)
        {
            ThrowHookedObject();
        }
    }

    private void ThrowHookedObject()
    {
        target.Throw(aimDirection * throwSpeed);
        state = HookState.Empty;
    }

    private void GrabObject(float time)
    {
        RaycastHit2D hit = Physics2D.CircleCast(transform.position, aimRadius, aimDirection, hookDistance, hookLayer);
        if (hit)
        {
            target = hit.collider.GetComponent<IHookable>();
            target.Hook(transform);
            state = HookState.Hooked;
            hit.transform.DOMove(holdPoint.position, time);
            Invoke("EndGrab", time);
            Debug.Log("Hit!");
        }
        else Debug.Log("No hits");
    }

   private void EndGrab()
    {
        if(toHold)
        {
            toHold = false;
            state = HookState.Held;
        }

    }
    internal void Swing()
    {
        if (state == HookState.Held)
            StartSwing();
        else if (state == HookState.Swinging)
            EndSwing();
    }

    private void EndSwing()
    {
        throw new NotImplementedException();
    }

    private void StartSwing()
    {
        throw new NotImplementedException();
    }

    internal void Backthrow()
    {
        if(state == HookState.Empty)
        {

        }
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        if(playerTransform!=null)
        Gizmos.DrawWireSphere(playerTransform.position, hookDistance);
    }
}

public enum HookState
{
    Empty, Hooked, Held, Swinging, Backthrow
}

