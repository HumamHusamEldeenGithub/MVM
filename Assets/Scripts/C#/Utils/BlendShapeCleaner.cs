using Mvm;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine.Rendering;

public static class BlendShapeCleaner
{
    private static List<float> previousBlendShapes = new List<float>();
    public static List<BlendShape> CleanBlendShapes(BlendShapes blendShapes)
    {
        var result = blendShapes.BlendShapes_.ToList();

        if (previousBlendShapes.Count > 0)
            SmoothBlendShapeValues(result, 0.5f);

        LimitExtremeValues(result);

        LimitContradictingBlendShapes(result);

        UpdatePreviousValues(result);

        return result;
    }

    private static void SmoothBlendShapeValues(List<BlendShape> blendshapes, float smoothingFactor)
    {
        UnityEngine.Debug.Log(blendshapes.Count);
        for (int i = 0; i < blendshapes.Count; i++)
        {
            var blendshape = blendshapes[i];
            float currentValue = blendshape.Score;

            blendshape.Score = (smoothingFactor * currentValue) + ((1 - smoothingFactor) * previousBlendShapes[i]);
        }
    }

    private static void LimitExtremeValues(List<BlendShape> BlendShapes)
    {
        // Limit extreme values of BlendShape scores to ensure realistic deformations
        // This can be done by clamping or thresholding the values to a specific range
        const float minScore = 0.0f;
        const float maxScore = 1.0f;

        foreach (var BlendShape in BlendShapes)
        {
            BlendShape.Score = Math.Clamp(BlendShape.Score, minScore, maxScore);
        }
    }

    private static void LimitContradictingBlendShapes(List<BlendShape> blendshapes)
    {
        // Identify and limit contradicting blendshapes that may produce unnatural deformations when active together
        // This step involves analyzing the blendshapes and their relationships to prevent conflicting effects

        LimitContradiction(blendshapes, "eyeBlinkLeft", "eyeWideLeft");
        LimitContradiction(blendshapes, "browDownLeft", "browUpLeft");
        LimitContradiction(blendshapes, "mouthClose", "mouthOpen");
        LimitContradiction(blendshapes, "mouthSmileLeft", "mouthFrownLeft");
        LimitContradiction(blendshapes, "mouthSmileRight", "mouthFrownRight");
        LimitContradiction(blendshapes, "mouthStretchLeft", "mouthShrugUpper");
        LimitContradiction(blendshapes, "mouthStretchRight", "mouthShrugUpper");
        LimitContradiction(blendshapes, "mouthLowerDownLeft", "mouthUpperUpLeft");
        LimitContradiction(blendshapes, "mouthLowerDownRight", "mouthUpperUpRight");
        LimitContradiction(blendshapes, "browInnerUp", "browOuterUpLeft");
        LimitContradiction(blendshapes, "browInnerUp", "browOuterUpRight");
        LimitContradiction(blendshapes, "browOuterUpLeft", "browOuterUpRight");

        // Add more cases for other blendshape combinations that may produce conflicting deformations
        // LimitContradiction(blendshapes, "Blendshape1", "Blendshape2");
        // LimitContradiction(blendshapes, "Blendshape3", "Blendshape4");
    }


    private static void LimitContradiction(List<BlendShape> BlendShapes, string BlendShape1, string BlendShape2)
    {
        var BlendShape1Obj = BlendShapes.Find(BlendShape => BlendShape.CategoryName == BlendShape1);
        var BlendShape2Obj = BlendShapes.Find(BlendShape => BlendShape.CategoryName == BlendShape2);

        if (BlendShape1Obj != null && BlendShape2Obj != null)
        {
            // If both BlendShapes are active, limit the effect of one to avoid contradicting deformations
            if (BlendShape1Obj.Score > 0 && BlendShape2Obj.Score > 0)
            {
                const float limitValue = 0.5f;
                BlendShape1Obj.Score = Math.Min(BlendShape1Obj.Score, limitValue);
                BlendShape2Obj.Score = Math.Min(BlendShape2Obj.Score, limitValue);
            }
        }
    }

    private static void UpdatePreviousValues(List<BlendShape> blendShapes)
    {
        for(int i = 0;i < blendShapes.Count; i++)
        {
            if (previousBlendShapes.Count < blendShapes.Count)
            {
                previousBlendShapes.Add(blendShapes[i].Score);
            }
            else
            {
                previousBlendShapes[i] = blendShapes[i].Score;
            }
        }
    }
}