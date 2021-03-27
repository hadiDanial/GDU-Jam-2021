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

        //controller.spline.Clear();

        controller.spline.SetPosition(0, splinePositions[0]);
        controller.spline.SetPosition(1, splinePositions[1]);
        //controller.spline.SetTangentMode(0, ShapeTangentMode.Linear);
        //controller.spline.SetTangentMode(1, ShapeTangentMode.Broken);
        controller.spline.SetSpriteIndex(0, 1);
        controller.spline.SetSpriteIndex(1, 0);
        for (int i = 2; i < splinePositions.Length - 2; i++)
        {
            //controller.spline.SetRightTangent(i - 1, (splinePositions[i] - splinePositions[i - 1])/2f);
            controller.spline.SetPosition(i, splinePositions[i]);
            //controller.spline.SetTangentMode(i, ShapeTangentMode.Continuous);
            //controller.spline.SetLeftTangent(i, splinePositions[i + 1] - splinePositions[i]);
        }

        int lastTwo = splinePositions.Length - 2;
        controller.spline.SetPosition(lastTwo, splinePositions[lastTwo]);
        //controller.spline.SetTangentMode(lastTwo, ShapeTangentMode.Broken);
        controller.spline.SetPosition(lastTwo, splinePositions[lastTwo] - (splinePositions[lastTwo-1] - splinePositions[lastTwo]).normalized*0.5f);
        controller.spline.SetSpriteIndex(lastTwo, 2);
        lastTwo++;
        //controller.spline.SetTangentMode(lastTwo, ShapeTangentMode.Linear);

        controller.RefreshSpriteShape();
    }
}
