using Mvm;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BlendShapeAnimator : MonoBehaviour
{
    public SkinnedMeshRenderer male_Renderer;
    public SkinnedMeshRenderer female_Renderer;

    private SkinnedMeshRenderer m_Renderer;

    private List<BlendShape> blendShapes;

    Dictionary<string, int> blendshapeKeys = new Dictionary<string, int>();

    private void Awake()
    {
        InitializeFace(null);
    }

    public void InitializeFace(UserProfile profile)
    {
        if (profile == null)
        {
            m_Renderer = male_Renderer.gameObject.activeSelf ? male_Renderer : female_Renderer;
            female_Renderer.gameObject.SetActive(male_Renderer.gameObject.activeSelf ? false : true);
            male_Renderer.gameObject.SetActive(female_Renderer.gameObject.activeSelf ? false : true);
        }
        else
        {
            switch (profile.userData.UserGender)
            {
                case Gender.Female:
                    m_Renderer = female_Renderer;
                    female_Renderer.gameObject.SetActive(true);
                    male_Renderer.gameObject.SetActive(false);
                    break;
                case Gender.Male:
                    m_Renderer = male_Renderer;
                    male_Renderer.gameObject.SetActive(true);
                    female_Renderer.gameObject.SetActive(false);
                    break;
            }
        }

        blendshapeKeys.Clear();
        for (int i = 0; i < m_Renderer.sharedMesh.blendShapeCount; i++)
        {
            blendshapeKeys.Add(m_Renderer.sharedMesh.GetBlendShapeName(i), i);
        }
    }

    private void Update()
    {
        if (blendShapes != null)
        {
            for (int i = 1; i < blendShapes.Count; i++)
            {
                // TODO : error found Array index (54) is out of bounds (size=52)

                //Debug.Log(i.ToString() + blendShapes[i].CategoryName);
                int ind = 0;
                if (blendshapeKeys.TryGetValue(blendShapes[i].CategoryName, out ind))
                {
                    var curValue = m_Renderer.GetBlendShapeWeight(ind);
                    curValue = Mathf.Lerp(curValue, blendShapes[i].Score * 100, 15 * Time.deltaTime);
                    m_Renderer.SetBlendShapeWeight(ind, curValue);
                }
            }
        }
    }

    public void SetBlendShapes(BlendShapes blendShapes)
    {
        this.blendShapes = blendShapes.BlendShapes_.ToList();
    }
}
