using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlendshapeAnimator : MonoBehaviour
{
    private SkinnedMeshRenderer m_Renderer;

    private void Awake()
    {
        m_Renderer = GetComponent<SkinnedMeshRenderer>();
    }

    private void Update()
    {
        m_Renderer.SetBlendShapeWeight(0, TrackingReceiver.jawWeight * 100);

        m_Renderer.SetBlendShapeWeight(1, TrackingReceiver.eyeLeftDown * 100);
        m_Renderer.SetBlendShapeWeight(2, TrackingReceiver.eyeLeftUp * 100);
        m_Renderer.SetBlendShapeWeight(3, TrackingReceiver.eyeLeftOut * 100);
        m_Renderer.SetBlendShapeWeight(4, TrackingReceiver.eyeLeftIn * 100);

        m_Renderer.SetBlendShapeWeight(5, TrackingReceiver.eyeRightDown * 100);
        m_Renderer.SetBlendShapeWeight(6, TrackingReceiver.eyeRightUp * 100);
        m_Renderer.SetBlendShapeWeight(7, TrackingReceiver.eyeRightOut * 100);
        m_Renderer.SetBlendShapeWeight(8, TrackingReceiver.eyeRightIn * 100);

        m_Renderer.SetBlendShapeWeight(10, TrackingReceiver.mouthSmile * 100);

    }
}
