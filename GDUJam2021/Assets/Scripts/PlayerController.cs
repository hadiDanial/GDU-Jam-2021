using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Hook))]
public class PlayerController : Entity
{
    public Hook hook;


    public delegate void SetHookDirection(Vector2 dir);
    public static event SetHookDirection OnSetHook;

    internal override void Awake()
    {
        base.Awake();
        hook = GetComponent<Hook>();
        hook.SetPlayer(transform);
    }

    public void OnMovement(InputValue value)
    {
        if (!IsActive())
            return;
        input = value.Get<Vector2>();
        movementVector = useGravity ? new Vector2(input.x, 0).normalized : input;
    }
    public void OnHorizontal(InputValue value)
    {
        if (!IsActive())
            return;
        input.x = value.Get<float>();
        movementVector = useGravity ? new Vector2(input.x, 0).normalized : input;
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
    public void OnOnBackthrow()
    {
        hook.Backthrow();
    }

    public void OnMouseAim(InputValue val)
    {
        Vector2 aim = val.Get<Vector2>();
        if (aim != Vector2.zero)
            hook.SetAim(aim);
    }
    public void OnGamepadAim(InputValue val)
    {
        //cursor.SetGamepad(val.Get<Vector2>());
        Vector2 aim = val.Get<Vector2>();
        if (aim != Vector2.zero)
            hook.SetAim(aim);
    }


}
