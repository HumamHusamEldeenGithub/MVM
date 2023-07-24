using Mvm;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FaceAnimator : MonoBehaviour
{
    private SkinnedMeshRenderer m_Renderer;
    private Material eyesMaterial;
    private Material skinMaterial;
    private Material hairMaterial;

    private List<BlendShape> blendShapes;
    Dictionary<string, int> blendshapeKeys = new Dictionary<string, int>();

    #region MonoBehaviour

    private void Awake()
    {
        m_Renderer = GetComponent<SkinnedMeshRenderer>();
        foreach(Material mat in m_Renderer.materials)
        {
            if (mat.name.ToLower().Contains("eye"))
            {
                eyesMaterial = mat;
            }
            else if (mat.name.ToLower().Contains("skin"))
            {
                skinMaterial = mat;
            }
        }
    }

    private void Update()
    {
        UpdateBlendShapes();
    }

    #endregion

    public void InitializeFace(UserProfile profile)
    {
        blendshapeKeys.Clear();
        for (int i = 0; i < m_Renderer.sharedMesh.blendShapeCount; i++)
        {
            blendshapeKeys.Add(m_Renderer.sharedMesh.GetBlendShapeName(i), i);
        }

        if (profile != null)
        {
            SetHeadStyle(profile.userData.UserFeatures.HeadStyle);
            SetHairStyle(profile.userData.UserFeatures.HairStyle);
            SetBrowsStyle(profile.userData.UserFeatures.EyebrowsStyle);
            SetEyeStyle(profile.userData.UserFeatures.EyeStyle);
            SetNoseStyle(profile.userData.UserFeatures.NoseStyle);
            SetMouthStyle(profile.userData.UserFeatures.MouthStyle);
            SetSkinImperfection(profile.userData.UserFeatures.SkinImperfection);
            SetTattoo(profile.userData.UserFeatures.Tattoo);

            SetHairColor(profile.userData.UserFeatures.HairColor);
            SetBrowsColor(profile.userData.UserFeatures.BrowsColor);
            SetSkinColor(profile.userData.UserFeatures.SkinColor);
            SetEyesColor(profile.userData.UserFeatures.EyeColor);
        }
    }

    #region Facial Features

    protected void SetHeadStyle(int headStyleInd)
    {
        // TODO
    }

    protected void SetHairStyle(int hairStyleInd)
    {
        // TODO
    }

    protected void SetBrowsStyle(int browsStyleInd)
    {
        // TODO _BrowsTexture
    }

    protected void SetEyeStyle(int eyeStyleInd)
    {
        // TODO 
    }
    
    protected void SetNoseStyle(int noseStyleInd)
    {
        // TODO 
    }

    protected void SetMouthStyle(int mouthStyleInd)
    {
        // TODO
    }

    protected void SetSkinImperfection(int skinImperfectionInd)
    {
        // TODO _FaceImperfectionTexture
    }

    protected void SetTattoo(int tattooInd)
    {
        // TODO _FaceTattooTexture
    }

    protected void SetHairColor(string colorHex)
    {
        Color color;
        if (ColorUtility.TryParseHtmlString(colorHex, out color))
        {
            hairMaterial.color = color;
        }
    }

    protected void SetBrowsColor(string colorHex)
    {
        Color color;
        if (ColorUtility.TryParseHtmlString(colorHex, out color))
        {
            skinMaterial.SetColor("_BrowsColor", color);
        }
    }

    protected void SetSkinColor(string colorHex)
    {
        Color color;
        if (ColorUtility.TryParseHtmlString(colorHex, out color))
        {
            skinMaterial.SetColor("_SkinColor", color);
        }
    }

    protected void SetEyesColor(string colorHex)
    {
        Color color;
        if(ColorUtility.TryParseHtmlString(colorHex, out color))
        {
            eyesMaterial.SetColor("_IrisColor", color);
        }
    }

    #endregion

    #region BlendShapes

    public void SetBlendShapes(BlendShapes blendShapes)
    {
        this.blendShapes = blendShapes.BlendShapes_.ToList();
    }

    private void UpdateBlendShapes()
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

    #endregion
}
