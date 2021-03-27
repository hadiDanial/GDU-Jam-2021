using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

[RequireComponent(typeof(SpriteShapeController))]
public class Hook : MonoBehaviour
{
    SpriteShapeController controller;

    public Vector2[] splinePositions = { Vector2.zero, Vector2.right * 0.5f, Vector2.right * 3f, Vector2.right * 3.5f, Vector2.right * 4f };
    private void Awake()
    {
        controller = GetComponent<SpriteShapeController>();
        SetSpline();

    }

    public void ActivateSpriteShape(bool val)
    {
        controller.enabled = val;
    }

    [ContextMenu("Set Spline")]
    private void SetSpline()
    {
        if(controller == null) controller = GetComponent<SpriteShapeController>();

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

        controller.spline.SetPosition(index, splinePositions[index] - (splinePositions[index-1] - splinePositions[index]).normalized*0.5f);
        controller.spline.SetSpriteIndex(index, 2);

        controller.RefreshSpriteShape();
    }
}
