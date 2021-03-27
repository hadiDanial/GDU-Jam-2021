using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HookCollider : MonoBehaviour
{
    public HookController hookController;
    public bool canCollide = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(canCollide)
        {
            canCollide = false;
            hookController.OnObjectGrabbed(collision);
        }
    }
}
