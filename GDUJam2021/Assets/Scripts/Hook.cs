using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Hook : MonoBehaviour
{
    [SerializeField] private GameObject arrow;
    [SerializeField] private Transform holdPoint, backthrowTarget;
    [SerializeField] private IHookable targetHookable;
    [SerializeField] private float hookDistance = 3f;
    [SerializeField] private float throwSpeed = 2.5f;
    [SerializeField] private float reelTime = 0.75f, backthowTime = 0.75f;
    [SerializeField] private float swingRadius = 2.5f, swingSpeed = 10;
    [SerializeField] private float aimRadius = 0.2f;
    [SerializeField] private LayerMask hookLayer;
    [SerializeField] private HookState state;

    private Vector2 aimDirection;
    private Transform playerTransform, targetTransform;
    private Vector3 defaultHoldPosition;
    private Coroutine reelCoroutine;
    bool toHold = false, forcePosition = false;
    private float swingPercent;


    private void Start()
    {
        defaultHoldPosition = holdPoint.position;
    }

    internal void SetPlayer(Transform transform)
    {
        playerTransform = transform;
    }

    internal void SetAim(Vector2 dir, bool useArrow = true)
    {
        aimDirection = dir.normalized;
        if(useArrow)
        {
            arrow.SetActive(true);
            arrow.transform.right = aimDirection;
        }
        else
        {
            arrow.SetActive(false);
        }
        backthrowTarget.localPosition = -aimDirection;
        //if (state == HookState.Held)
        //    target.SetPosition((Vector2)transform.position + aimDirection.normalized * swingRadius);
    }

    internal void HookObject()
    {
        forcePosition = false;
        if (state == HookState.Empty)
        {
            toHold = true;
            GrabObject(reelTime, holdPoint, false);
        }
        else 
            ThrowHookedObject();       
    }

    private void ThrowHookedObject()
    {
        if (state == HookState.Held)
        {
            targetHookable.Throw(aimDirection * throwSpeed);
        }
        else if (state == HookState.Swinging)
        {

        }
            state = HookState.Empty;
    }

    private void GrabObject(float time, Transform targetT, bool throwObject = false)
    {
        RaycastHit2D hit = Physics2D.CircleCast(transform.position, aimRadius, aimDirection, hookDistance, hookLayer);
        if (hit)
        {
            targetHookable = hit.collider.GetComponent<IHookable>();
            targetHookable.Hook(transform);
            targetTransform = hit.transform;
            state = HookState.Hooked;
            targetTransform.DOLocalMove(targetT.localPosition, time);
            if (throwObject)
                Invoke("Throw", time - 0.1f);
            else
                Invoke("EndGrab", time-0.1f);
            Debug.Log("Hit!");
        }
        else Debug.Log("No hits");
    }

   private void EndGrab()
    {
        if(toHold)
        {
            toHold = false;
            forcePosition = true;
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
    private void StartSwing()
    {
        swingPercent = 0;
    }

    private void EndSwing()
    {
        swingPercent = 0;
    }


    internal void Backthrow()
    {
        if(state == HookState.Empty)
        {

        }
    }


    private void OnDrawGizmos()
    {
        if (playerTransform != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(playerTransform.position, hookDistance);

        }
        if (backthrowTarget != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(backthrowTarget.position, 0.25f);
        }

        }
    }

public enum HookState
{
    Empty, Hooked, Held, Swinging, Backthrow
}

