using Mvm;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlendShapeAnimator : MonoBehaviour
{
    private SkinnedMeshRenderer m_Renderer;

    private List<BlendShape> blendShapes;

    /*
    1   - browDownLeft
    2   - browDownRight 
    3	- browInnerUp 
    4	- browOuterUpLeft
    5	- browOuterUpRight 
    6   - cheekPuff
    7   - cheekSquintLeft
    8	- cheekSquintRight 
    9	- eyeBlinkLeft
    10	- eyeBlinkRight 
    11	- eyeLookDownLeft
    12	- eyeLookDownRight 
    13	- eyeLookInLeft
    14	- eyeLookInRight 
    15	- eyeLookOutLeft
    16	- eyeLookOutRight 
    17	- eyeLookUpLeft
    18	- eyeLookUpRight 
    19	- eyeSquintLeft
    20	- eyeSquintRight 
    21	- eyeWideLeft
    22	- eyeWideRight 
    23	- jawForward
    24	- jawLeft
    25	- jawOpen 
    26	- jawRight
    27	- mouthClose 
    28	- mouthDimpleLeft
    29	- mouthDimpleRight 
    30	- mouthFrownLeft
    31	- mouthFrownRight 
    32	- mouthFunnel 
    33	- mouthLeft
    34	- mouthLowerDownLeft
    35	- mouthLowerDownRight 
    36	- mouthPressLeft
    37	- mouthPressRight 
    38	- mouthPucker 
    39	- mouthRight 
    40	- mouthRollLower
    41	- mouthRollUpper 
    42	- mouthShrugLower
    43 - mouthShrugUpper 
    44	- mouthSmileLeft
    45	- mouthSmileRight 
    46	- mouthStretchLeft
    47	- mouthStretchRight 
    48	- mouthUpperUpLeft
    49	- mouthUpperUpRight 
    50	- noseSneerLeft
    51	- noseSneerRight 
    52	- tongueOut
     */

    private void Awake()
    {
        m_Renderer = GetComponent<SkinnedMeshRenderer>();
    }

    private void Update()
    {
        if (blendShapes != null)
        {
            for (int i = 1; i < blendShapes.Count; i++)
            {
                // TODO : error found Array index (54) is out of bounds (size=52)

                //Debug.Log(i.ToString() + blendShapes[i].CategoryName);
                var curValue = m_Renderer.GetBlendShapeWeight(i-1);
                curValue = Mathf.Lerp(curValue, blendShapes[i].Score * 100, 15 * Time.deltaTime);
                m_Renderer.SetBlendShapeWeight(i - 1, curValue);
            }
        }
    }

    public void SetBlendShapes(BlendShapes blendShapes)
    {
        this.blendShapes = BlendShapeCleaner.CleanBlendShapes(blendShapes);
    }
}
