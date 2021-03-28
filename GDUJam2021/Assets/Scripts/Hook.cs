﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using DG.Tweening;

[RequireComponent(typeof(SpriteShapeController))]
public class Hook : MonoBehaviour
{
    public GameObject hookHead;
    public Transform restPosition, hitPosition;
    public HookController hookController;
    public bool canCollide = false;

    Coroutine throwHeadCoroutine, reelCoroutine;

    SpriteShapeController controller;
    public Vector2[] splinePositions;
    private LayerMask hookLayer;
    private float aimRadius;
    private Collider2D hit;
    private bool hasHit, canThrow = true;

    private void Awake()
    {
        controller = GetComponent<SpriteShapeController>();
    }

    internal void Setup(LayerMask hookLayer, float aimRadius, HookController hookController)
    {
        this.hookLayer = hookLayer;
        this.aimRadius = aimRadius;
        this.hookController = hookController;
        canCollide = false;
    }
    private void Update()
    {
        if(canCollide && !hasHit)
        {
            hit = Physics2D.OverlapCircle(hitPosition.transform.position, aimRadius, hookLayer);
            if (hit != null)
            {
                hasHit = true;
                hookController.OnObjectGrabbed(hit);
                DOTween.Kill(hookHead.transform);
            }
        }
        if (!canThrow)
            SetSpline();
    }
    public void ActivateSpriteShape(bool val)
    {
        controller.enabled = val;
    }

    [ContextMenu("Set Spline")]
    private void SetSpline()
    {
        if (controller == null) controller = GetComponent<SpriteShapeController>();
        splinePositions = new Vector2[3];
        if (canThrow)
        {
            splinePositions[0] = Vector2.zero;
            splinePositions[2] = Vector2.right * 0.5f;
            splinePositions[1] = Vector2.right * 3f;
        }
        else
        {
            splinePositions[0] = restPosition.localPosition;
            splinePositions[2] = hookHead.transform.localPosition;
            splinePositions[1] = (splinePositions[0] + splinePositions[2]) / 2f;
        }
        if (Vector2.Distance(splinePositions[0], splinePositions[1]) < 0.25f ||
            Vector2.Distance(splinePositions[1], splinePositions[2]) < 0.25f)
        {
            ActivateSpriteShape(false);
            return;
        }
        else
        {
            ActivateSpriteShape(true);
        }

        for (int i = 0; i < 3; i++)
        {
            controller.spline.SetPosition(i, splinePositions[i]);
            controller.spline.SetTangentMode(i, ShapeTangentMode.Continuous);
        }
        Vector2 tangent = (splinePositions[2] - splinePositions[0])/2f;
        controller.spline.SetLeftTangent(1, -tangent);
        controller.spline.SetRightTangent(1, tangent);

        controller.RefreshSpriteShape();

        //OldSetSpline();
    }

    private void OldSetSpline()
    {
        controller.spline.SetPosition(0, splinePositions[0]);
        controller.spline.SetPosition(1, splinePositions[1]);

        controller.spline.SetSpriteIndex(0, 1);
        controller.spline.SetSpriteIndex(1, 0);
        for (int i = 2; i < splinePositions.Length - 2; i++)
        {
            controller.spline.SetPosition(i, splinePositions[i]);

        }

        int index = splinePositions.Length - 2;
        controller.spline.SetPosition(index, splinePositions[index]);

        controller.spline.SetPosition(index, splinePositions[index] - (splinePositions[index - 1] - splinePositions[index]).normalized * 0.5f);
        controller.spline.SetSpriteIndex(index, 2);

        controller.RefreshSpriteShape();
    }

    internal void ThrowHead(Vector2 aimDirection, float hookDistance, Hook hook, float currentTime, Transform returnTarget = null)
    {
        if (!canThrow) return;
        hookController.playerController.audioSource.PlayOneShot(hookController.playerController.hookSound);

        if (throwHeadCoroutine != null)
        {
            StopCoroutine(throwHeadCoroutine);
        }
        throwHeadCoroutine = StartCoroutine(ThrowHeadCoroutine(aimDirection * hookDistance, currentTime, returnTarget));
    }

    IEnumerator ThrowHeadCoroutine(Vector2 aimDirection, float time, Transform returnPos)
    {
        canThrow = false;
        ActivateSpriteShape(true);
        //Tween myTween = hookHeadRB.DOMove((Vector2)transform.position + aimDirection, time);
        Tween myTween = hookHead.transform.DOMove((Vector2)transform.position + aimDirection, time);
        canCollide = true;
        hasHit = false;
        yield return myTween.WaitForCompletion();
        if(!hasHit)
        {
            hit = Physics2D.OverlapCircle(hitPosition.transform.position, aimRadius, hookLayer);
        }
        canCollide = false;
        if (hit != null && !hasHit)
        {
            hookController.OnObjectGrabbed(hit);
        }
        if (reelCoroutine != null)
        {
            StopCoroutine(reelCoroutine);
        }
        reelCoroutine = StartCoroutine(ReelCoroutine(hit, time / 2f, returnPos));
    }

    IEnumerator ReelCoroutine(Collider2D hit, float time, Transform returnPos)
    {
        Transform p;
        if (returnPos == null) p = restPosition;
        else p = returnPos;
        if (hookController.playerController.isGrounded)
        {
            p.transform.position = new Vector3(p.transform.position.x, hookController.transform.position.y);
        }
        Tween myTween = hookHead.transform.DOMove(p.position, time);
        //if (hit != null)
        //    hit.transform.DOMove(hitPosition.position, time / 2f);
        yield return myTween.WaitForCompletion();
        if (restPosition != returnPos)
        {
            hookController.OnBackreelFinished();
            StartCoroutine(ReelCoroutine(hit, time, restPosition));
        }
        else
        {
            hookHead.transform.position = restPosition.position;
            canThrow = true;
            ActivateSpriteShape(false);
            hookController.OnReelFinished();
        }
    }

    private void OnDrawGizmos()
    {
        if (hookHead != null && hitPosition != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(hitPosition.transform.position, aimRadius);

        }
    }
}

