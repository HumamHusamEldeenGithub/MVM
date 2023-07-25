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

    public void InitializeFace(UserProfile.PeerData userData)
    {
        blendshapeKeys.Clear();
        for (int i = 0; i < m_Renderer.sharedMesh.blendShapeCount; i++)
        {
            blendshapeKeys.Add(m_Renderer.sharedMesh.GetBlendShapeName(i), i);
        }

        if (userData != null)
        {
           
            SetHeadStyle(int.Parse(userData.AvatarSettings[(int)AvatarSettings.HeadStyle]));
            SetHairStyle(int.Parse(userData.AvatarSettings[(int)AvatarSettings.HairStyle]));
            SetBrowsStyle(int.Parse(userData.AvatarSettings[(int)AvatarSettings.EyebrowsStyle]));
            SetEyeStyle(int.Parse(userData.AvatarSettings[(int)AvatarSettings.EyeStyle]));
            SetNoseStyle(int.Parse(userData.AvatarSettings[(int)AvatarSettings.NoseStyle]));
            SetMouthStyle(int.Parse(userData.AvatarSettings[(int)AvatarSettings.MouthStyle]));
            SetSkinImperfection(int.Parse(userData.AvatarSettings[(int)AvatarSettings.SkinImperfection]));
            SetTattoo(int.Parse(userData.AvatarSettings[(int)AvatarSettings.Tattoo]));

            SetHairColor(userData.AvatarSettings[(int)AvatarSettings.HairColor]);
            SetBrowsColor(userData.AvatarSettings[(int)AvatarSettings.BrowsColor]);
            SetSkinColor(userData.AvatarSettings[(int)AvatarSettings.SkinColor]);
            SetEyesColor(userData.AvatarSettings[(int)AvatarSettings.EyeColor]);
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
