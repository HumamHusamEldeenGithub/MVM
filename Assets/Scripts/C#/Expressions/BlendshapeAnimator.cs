using Mvm;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlendShapeAnimator : MonoBehaviour
{
    private SkinnedMeshRenderer m_Renderer;

    private BlendShapes BlendShapes;

    private void Awake()
    {
        m_Renderer = GetComponent<SkinnedMeshRenderer>();
    }

    private void Update()
    {
        if (BlendShapes != null)
        {
            for (int i = 1; i < BlendShapes.BlendShapes_.Count; i++)
            {
                Debug.Log(i.ToString() + BlendShapes.BlendShapes_[i].CategoryName);
                m_Renderer.SetBlendShapeWeight(i - 1, BlendShapes.BlendShapes_[i].Score * 100);
            }
        }
    }

    public void SetBlendShapes(BlendShapes blendShapes)
    {
        BlendShapes = blendShapes;
    }
}
