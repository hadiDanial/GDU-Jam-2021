using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(HookController))]
public class PlayerController : Entity
{
    public AudioClip hookSound, throwSound;
    public HookController hook;

    Vector2 prevAim = Vector2.right;

    internal override void Awake()
    {
        base.Awake();
        hook = GetComponent<HookController>();
        hook.SetPlayer(transform);
    }

    public void OnMovement(InputValue value)
    {
        if (!IsActive())
            return;
        input = value.Get<Vector2>();
        SetMovementVector();
    }


    public void OnHorizontal(InputValue value)
    {
        if (!IsActive())
            return;
        input.x = value.Get<float>();
        SetMovementVector();
    }



    public void OnVertical(InputValue value)
    {
        if (!IsActive())
            return;
        input.y = value.Get<float>();
        movementVector = useGravity ? new Vector2(input.x, 0).normalized : input;
    }
    public void OnJump()
    {
        if (!IsActive())
            return;
        Jump();
    }
    public void OnHook()
    {
        hook.HookObject();
    }
    public void OnSwing()
    {
        hook.Swing();
    }
    public void OnBackthrow()
    {
        hook.Backthrow();
    }

    public void OnMouseAim(InputValue val)
    {
        Vector2 aim = Camera.main.ScreenToWorldPoint(val.Get<Vector2>()) - transform.position;
        SetAim(aim);
    }


    public void OnGamepadAim(InputValue val)
    {
        //cursor.SetGamepad(val.Get<Vector2>());
        Vector2 aim = val.Get<Vector2>();
        SetAim(aim);
    }

    private void SetAim(Vector2 aim)
    {
        if (aim == Vector2.zero)
        {
            aim = prevAim;
        }
        hook.SetAim(aim);
        RotateGFX(aim);
        prevAim = aim;
    }
}
