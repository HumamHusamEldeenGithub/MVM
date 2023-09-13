using Mvm;
using System;
using System.Collections.Generic;
using UnityEngine;

public class FaceAnimator : MonoBehaviour
{

    [Header("Enable | Disable options")]
    [SerializeField] private bool enableeyeWide;
    [SerializeField] private bool enableDimple;
    [SerializeField] private bool enablecheekSquint;
    [SerializeField] private bool enablenose;
    [Tooltip("Enabling this option will open and close eyes together(you can not blink!)")]
    [SerializeField] private bool enableSimultaneouslyeyesOpenClose;
    [Tooltip("Enabling this option will frown your mouth together(you can not frown one side)")]
    [SerializeField] private bool enableSimultaneouslyFrown;

    [Header("Methods")]
    [Tooltip("How to deal with each blend weights")]
    [SerializeField] private int eyeWideMethod;
    [SerializeField] private int mouthCloseMethod = 1;
    [SerializeField] private int mouthSmileFrownMethod;

    [SerializeField]
    private Transform headBone;

    class LocalBlendShape
    {
        public int index;
        public float weight;
    }

    private SkinnedMeshRenderer m_Renderer;
    private AvatarCustomizer avatarCustomizer;

    private List<string> blendShapeNames = new List<string>()
    {
        "browDownLeft",
        "browDownRight",
        "browInnerUp",
        "browOuterUpLeft",
        "browOuterUpRight",
        "cheekPuff",
        "cheekSquintLeft",
        "cheekSquintRight",
        "eyeBlinkLeft",
        "eyeBlinkRight",
        "eyeLookDownLeft",
        "eyeLookDownRight",
        "eyeLookInLeft",
        "eyeLookInRight",
        "eyeLookOutLeft",
        "eyeLookOutRight",
        "eyeLookUpLeft",
        "eyeLookUpRight",
        "eyeSquintLeft",
        "eyeSquintRight",
        "eyeWideLeft",
        "eyeWideRight",
        "jawForward",
        "jawLeft",
        "jawOpen",
        "jawRight",
        "mouthClose",
        "mouthDimpleLeft",
        "mouthDimpleRight",
        "mouthFrownLeft",
        "mouthFrownRight",
        "mouthFunnel",
        "mouthLeft",
        "mouthLowerDownLeft",
        "mouthLowerDownRight",
        "mouthPressLeft",
        "mouthPressRight",
        "mouthPucker",
        "mouthRight",
        "mouthRollLower",
        "mouthRollUpper",
        "mouthShrugLower",
        "mouthShrugUpper",
        "mouthSmileLeft",
        "mouthSmileRight",
        "mouthStretchLeft",
        "mouthStretchRight",
        "mouthUpperUpLeft",
        "mouthUpperUpRight",
        "noseSneerLeft",
        "noseSneerRight",
        "tongueOut"
    };

    private Dictionary<string, LocalBlendShape> blendshapeKeys = new Dictionary<string, LocalBlendShape>();

    #region MonoBehaviour

    private void Awake()
    {
        avatarCustomizer = GetComponent<AvatarCustomizer>();
        m_Renderer = GetComponentInChildren<SkinnedMeshRenderer>();
    }

    private void Update()
    {
        UpdateBlendShape();
        //UpdateBlendShapes();
    }

    #endregion

    public void InitializeFace(UserProfile.PeerData userData)
    {
        foreach (var name in blendShapeNames)
        {
            blendshapeKeys.Add(name, new LocalBlendShape()
            {
                weight = 0,
                index = m_Renderer.sharedMesh.GetBlendShapeIndex(name)
            });
        }

        if (userData != null)
        {
            avatarCustomizer.SetAvatarSettings(userData.AvatarSettings);
        }
    }

    #region BlendShapes

    public void SetBlendShapes(BlendShapes blendShapes)
    {
        foreach(var bs in blendShapes.BlendShapes_)
        {
            try
            {
                if (bs.CategoryName == "_neutral") continue;
                blendshapeKeys[bs.CategoryName].weight = bs.Score * 100;
            }
            catch(Exception e)
            {
                Debug.LogWarning(e);
            }
        }
    }

    public void UpdateBlendShape()
    {
        // Apply deformation weights

        // Apply eye values
        Updateeyes();

        // Apply mouth smile and frown values
        UpdateMouthSmileFrownWeights();

        // mouths
        mouthDirection();


        UpdateBlendShapeWeight(blendshapeKeys["mouthLeft"]);
        UpdateBlendShapeWeight(blendshapeKeys["mouthRight"]);

        UpdateBlendShapeWeight(blendshapeKeys["mouthStretchLeft"]);
        UpdateBlendShapeWeight(blendshapeKeys["mouthStretchRight"]);

        UpdateBlendShapeWeight(blendshapeKeys["mouthLowerDownRight"]);
        UpdateBlendShapeWeight(blendshapeKeys["mouthLowerDownLeft"]);

        UpdateBlendShapeWeight(blendshapeKeys["mouthPressLeft"]);
        UpdateBlendShapeWeight(blendshapeKeys["mouthPressRight"]);

        if (mouthCloseMethod == 1)
        {
            blendshapeKeys["mouthClose"].weight *= 4;
        }
        else if (mouthCloseMethod == 2)
        {
            if (blendshapeKeys["mouthClose"].weight > 80)
                blendshapeKeys["mouthClose"].weight = 100;
            if (blendshapeKeys["mouthClose"].weight > 1)
            {
                blendshapeKeys["mouthClose"].weight = MappingEffect(blendshapeKeys["mouthClose"].weight, 80, 120, 0);
            }
        }
        UpdateBlendShapeWeight(blendshapeKeys["mouthClose"]);
        UpdateBlendShapeWeight(blendshapeKeys["mouthPucker"]);

        UpdateBlendShapeWeight(blendshapeKeys["mouthShrugUpper"]);
        UpdateBlendShapeWeight(blendshapeKeys["jawOpen"]);
        UpdateBlendShapeWeight(blendshapeKeys["jawLeft"]);
        UpdateBlendShapeWeight(blendshapeKeys["jawRight"]);

        UpdateBlendShapeWeight(blendshapeKeys["browDownLeft"]);
        UpdateBlendShapeWeight(blendshapeKeys["browOuterUpLeft"]);
        UpdateBlendShapeWeight(blendshapeKeys["browDownRight"]);
        UpdateBlendShapeWeight(blendshapeKeys["browOuterUpRight"]);

        if (enablecheekSquint)
        {
            UpdateBlendShapeWeight(blendshapeKeys["cheekSquintRight"]);
            UpdateBlendShapeWeight(blendshapeKeys["cheekSquintLeft"]);
        }

        UpdateBlendShapeWeight(blendshapeKeys["mouthRollLower"]);
        UpdateBlendShapeWeight(blendshapeKeys["mouthRollUpper"]);

        if (enablenose)
        {
            UpdateBlendShapeWeight(blendshapeKeys["noseSneerLeft"]);
            UpdateBlendShapeWeight(blendshapeKeys["noseSneerRight"]);
        }


    }

    private void Updateeyes()
    {
        if (enableSimultaneouslyeyesOpenClose)
        {
            blendshapeKeys["eyeBlinkLeft"].weight = (blendshapeKeys["eyeBlinkLeft"].weight + blendshapeKeys["eyeBlinkRight"].weight) / 2.0f;
            blendshapeKeys["eyeBlinkRight"].weight = blendshapeKeys["eyeBlinkLeft"].weight;
        }

        UpdateBlendShapeWeight(blendshapeKeys["eyeBlinkLeft"]);
        UpdateBlendShapeWeight(blendshapeKeys["eyeBlinkRight"]);
        UpdateBlendShapeWeight(blendshapeKeys["eyeSquintLeft"]);
        UpdateBlendShapeWeight(blendshapeKeys["eyeSquintRight"]);

        UpdateBlendShapeWeight(blendshapeKeys["eyeLookInLeft"]);
        UpdateBlendShapeWeight(blendshapeKeys["eyeLookOutLeft"]);
        UpdateBlendShapeWeight(blendshapeKeys["eyeLookUpLeft"]);
        UpdateBlendShapeWeight(blendshapeKeys["eyeLookDownLeft"]);

        UpdateBlendShapeWeight(blendshapeKeys["eyeLookInRight"]);
        UpdateBlendShapeWeight(blendshapeKeys["eyeLookOutRight"]);
        UpdateBlendShapeWeight(blendshapeKeys["eyeLookUpRight"]);
        UpdateBlendShapeWeight(blendshapeKeys["eyeLookDownRight"]);

        if (enableeyeWide)
        {
            if (eyeWideMethod == 1)
            {
                blendshapeKeys["eyeWideLeft"].weight = MappingEffect(blendshapeKeys["eyeWideLeft"].weight - 80, 20, 100, -40);
                blendshapeKeys["eyeWideRight"].weight = MappingEffect(blendshapeKeys["eyeWideRight"].weight - 80, 20, 100, -40);
            }
            else if (eyeWideMethod == 2)
            {
                if (blendshapeKeys["eyeWideLeft"].weight < 90)
                {
                    blendshapeKeys["eyeWideLeft"].weight = 0;
                }

                if (blendshapeKeys["eyeWideRight"].weight < 90)
                {
                    blendshapeKeys["eyeWideRight"].weight = 0;
                }
            }


            UpdateBlendShapeWeight(blendshapeKeys["eyeWideLeft"]);
            UpdateBlendShapeWeight(blendshapeKeys["eyeWideRight"]);
        }
    }

    private void mouthDirection()
    {
        UpdateBlendShapeWeight(blendshapeKeys["mouthLowerDownLeft"]);
        UpdateBlendShapeWeight(blendshapeKeys["mouthLowerDownRight"]);
        UpdateBlendShapeWeight(blendshapeKeys["mouthUpperUpLeft"]);
        UpdateBlendShapeWeight(blendshapeKeys["mouthUpperUpRight"]);

    }

    private void UpdateMouthSmileFrownWeights()
    {
        if (mouthSmileFrownMethod == 1)
        {
            blendshapeKeys["mouthSmileRight"].weight = MappingEffect(blendshapeKeys["mouthSmileRight"].weight, 60, 100, 0);
            blendshapeKeys["mouthSmileLeft"].weight = MappingEffect(blendshapeKeys["mouthSmileLeft"].weight, 60, 100, 0);
            blendshapeKeys["mouthDimpleLeft"].weight = MappingEffect(blendshapeKeys["mouthDimpleLeft"].weight, 60, 100, 0);
            blendshapeKeys["mouthDimpleRight"].weight = MappingEffect(blendshapeKeys["mouthDimpleRight"].weight, 60, 100, 0);

        }

        if (enableSimultaneouslyFrown)
        {
            blendshapeKeys["mouthFrownRight"].weight = (blendshapeKeys["mouthFrownLeft"].weight + blendshapeKeys["mouthFrownRight"].weight) / 2.0f;
            blendshapeKeys["mouthFrownLeft"].weight = blendshapeKeys["mouthFrownRight"].weight;
        }

        UpdateBlendShapeWeight(blendshapeKeys["mouthSmileRight"]);
        UpdateBlendShapeWeight(blendshapeKeys["mouthSmileLeft"]);
        if (enableDimple)
        {
            UpdateBlendShapeWeight(blendshapeKeys["mouthDimpleLeft"]);
            UpdateBlendShapeWeight(blendshapeKeys["mouthDimpleRight"]);
        }

        UpdateBlendShapeWeight(blendshapeKeys["mouthFrownRight"]);
        UpdateBlendShapeWeight(blendshapeKeys["mouthFrownLeft"]);
    }

    private void UpdateBlendShapeWeight(LocalBlendShape blendShape)
    {
        UpdateBlendShapeWeight(blendShape.index, blendShape.weight);
    }
    private void UpdateBlendShapeWeight(int blendNum, float blendWeight)
    {
        if (blendNum != -1)
        {

            var curValue = m_Renderer.GetBlendShapeWeight(blendNum);
            curValue = Mathf.Lerp(curValue, Mathf.Clamp(blendWeight, 0, 100), 15 * Time.deltaTime);
            m_Renderer.SetBlendShapeWeight(blendNum, Mathf.Clamp(curValue, 0, 100));
        }
    }

    //change the value between 0 upto effectOrder
    private float MappingEffect(float value, float maxValue, float effectOrder, float offset)
    {
        return (value / maxValue) * effectOrder + offset;
    }

    #endregion
}
