using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;


public class HookController : MonoBehaviour
{
    [SerializeField] private Hook hook;
    [SerializeField] private SpriteRenderer hookHeadRenderer = null;
    [SerializeField] private GameObject hookGameObject = null, hookPos = null;
    [SerializeField] protected Transform holdPoint, backthrowTarget;
    [SerializeField] private IHookable targetHookable;
    [SerializeField] private float hookDistance = 3f;
    [SerializeField] private float throwSpeed = 2.5f;
    [SerializeField] private float reelTime = 0.75f, backthowTime = 0.75f;
    [SerializeField] private float swingRadius = 2.5f, swingSpeed = 10;
    [SerializeField] private float aimRadius = 0.3f;
    [SerializeField] private LayerMask hookLayer;
    [SerializeField] private HookState state;

    internal PlayerController playerController;
    private Vector2 aimDirection;
    private Transform playerTransform, targetTransform;
    private Vector3 defaultHoldPosition;
    private float swingPercent, currentTime;
    private RaycastHit2D hit;
    private Transform targetT;
    internal bool throwObject;
    private bool lockBackthrowTarget;
    float hitPosX, hitPosY;
    private void Start()
    {
        hook.Setup(hookLayer, aimRadius, this);
        hitPosX = hookPos.transform.localPosition.x;
        hitPosY = hookPos.transform.localPosition.y;
        defaultHoldPosition = holdPoint.position;
        playerController = GetComponent<PlayerController>();
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
            hookGameObject.SetActive(true);
            hookGameObject.transform.rotation = MathP.RotationFromDirection(-aimDirection);
            if (aimDirection.x < 0)
            {
                hookHeadRenderer.flipY = true;
                hookPos.transform.localPosition = new Vector2(hitPosX, -hitPosY);
            }
            else if (aimDirection.x > 0)
            {
                hookHeadRenderer.flipY = false;
                hookPos.transform.localPosition = new Vector2(hitPosX, hitPosY);
            }
        }
        else
        {
            hookGameObject.SetActive(false);
        }
        if (playerController.isGrounded)
            aimDirection = new Vector2(aimDirection.x, Mathf.Clamp(aimDirection.y, 0, 2));
        if (!lockBackthrowTarget)
            backthrowTarget.localPosition = -aimDirection * hookDistance;
    }

    /// <summary>
    /// Hook an object or throw it
    /// </summary>
    internal void HookObject()
    {
        if (state == HookState.Empty)
        {
            targetT = holdPoint;
            currentTime = reelTime;
            throwObject = false;
            GrabObject();
        }
        else 
            ThrowHookedObject(aimDirection);       
    }

    /// <summary>
    /// Try to grab object
    /// </summary>
    private void GrabObject()
    {
        hook.ThrowHead(aimDirection, hookDistance, hook, currentTime, targetT);
    }

    /// <summary>
    /// What to do when the object is grabbed
    /// </summary>
    /// <param name="collision"></param>
    internal void OnObjectGrabbed(Collider2D hookable)
    {
        targetHookable = hookable.GetComponent<IHookable>();
        targetHookable.Hook(hookPos.transform);
        targetTransform = hookable.transform;
        state = HookState.Hooked;
    }
    
    /// <summary>
    /// Throw object
    /// </summary>
    private void ThrowHookedObject(Vector2 direction)
    {
        if (targetHookable == null)
        {
            Clear();
            return;
        }
        else if (state == HookState.Held || throwObject)
        {
            if (targetHookable != null)
            {
                targetHookable.Throw(direction * throwSpeed);
                playerController.audioSource.PlayOneShot(playerController.throwSound);
            }
            Clear();
            if (throwObject) throwObject = false;
            lockBackthrowTarget = false;
        }
        else if (state == HookState.Swinging)
        {

        }
        state = HookState.Empty;
    }

    internal void Clear(bool throwObj = false)
    {
        if (targetHookable != null && throwObj) targetHookable.Throw(Vector2.zero);
        targetHookable = null;
        targetTransform = null;
        state = HookState.Empty;
    }

    internal void OnReelFinished()
    {
        if (targetHookable != null)
            targetHookable.Hold();
        EndGrab();
    }

    private void EndGrab()
    {
        if(targetHookable != null)
        {
            state = HookState.Held;
        }
        else
        {
            throwObject = false;
            lockBackthrowTarget = false;
        }

    }

    internal void OnBackreelFinished()
    {
        if (targetHookable != null)
        {
            targetHookable.Hold();
            if (throwObject)
            {
                ThrowHookedObject(-aimDirection);
            }
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
            lockBackthrowTarget = true;
            targetT = backthrowTarget;
            currentTime = backthowTime;
            throwObject = true;
            
            GrabObject();
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

