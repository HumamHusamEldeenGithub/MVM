using Mvm;
using UnityEngine;

public class AvatarCustomizer : MonoBehaviour
{
    [SerializeField]
    private SkinnedMeshRenderer hair_Renderer;

    [SerializeField]
    private SkinnedMeshRenderer skin_Renderer;

    [SerializeField]
    private GameObject glasses_Slot;

    private Mesh[] hairMeshes;
    private GameObject[] glassesPrefabs;
    private Texture[] hairTextures;
    private Texture[] browsTextures;
    private Texture[] skinImperfectionTextures;
    private Texture[] tattooTextures;

    private Material eyesMaterial;
    private Material skinMaterial;

    private void Awake()
    {
        foreach (Material mat in skin_Renderer.materials)
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

        hairMeshes = Resources.LoadAll<Mesh>("Hair/Mesh");
        glassesPrefabs = Resources.LoadAll<GameObject>("Glasses/Prefabs");
        hairTextures = Resources.LoadAll<Texture>("Hair/Textures");
        browsTextures = Resources.LoadAll<Texture>("Brows");
        skinImperfectionTextures = Resources.LoadAll<Texture>("SkinImperfection");
        tattooTextures = Resources.LoadAll<Texture>("Tattoo");

        Debug.Log(glassesPrefabs.Length);
    }
    public void SetAvatarSettings(AvatarSettings avatarSettings)
    {
        SetHeadStyle(int.Parse(avatarSettings.HeadStyle));
        SetHairStyle(int.Parse(avatarSettings.HairStyle));
        SetGlassesStyle(int.Parse(avatarSettings.Glasses));
        SetBrowsStyle(int.Parse(avatarSettings.EyebrowsStyle));
        SetEyeStyle(int.Parse(avatarSettings.EyeStyle));
        SetNoseStyle(int.Parse(avatarSettings.NoseStyle));
        SetMouthStyle(int.Parse(avatarSettings.MouthStyle));
        SetSkinImperfection(int.Parse(avatarSettings.SkinImperfection));
        SetTattoo(int.Parse(avatarSettings.Tattoo));

        SetHairColor(avatarSettings.HairColor);
        SetBrowsColor(avatarSettings.BrowsColor);
        SetSkinColor(avatarSettings.SkinColor);
        SetEyesColor(avatarSettings.EyeColor);
    }

    protected void SetHeadStyle(int headStyleInd)
    {
        headStyleInd = Mathf.Clamp(headStyleInd, 0, 3);

        for (int i = 1; i < 5; i++)
        {
            int ind = skin_Renderer.sharedMesh.GetBlendShapeIndex("face" + i.ToString());

            if (ind != -1)
                skin_Renderer.SetBlendShapeWeight(ind, i == headStyleInd ? 100 : 0);
        }
    }

    protected void SetHairStyle(int hairStyleInd)
    {
        hair_Renderer.sharedMesh = null;
        skinMaterial.SetTexture("_HairTexture", null);
        if (hairStyleInd == 0)
        {
            return;
        }
        for(int i = 0;i < hairMeshes.Length; i++)
        {
            if (hairMeshes[i].name == hairStyleInd.ToString())
            {
                hair_Renderer.sharedMesh = hairMeshes[i];
                break;
            }
        }
        for (int i = 0; i < hairTextures.Length; i++)
        {
            if (hairTextures[i].name == hairStyleInd.ToString())
            {
                skinMaterial.SetTexture("_HairTexture", hairTextures[i]);
                break;
            }
        }
    }

    protected void SetGlassesStyle(int glassesStyleInd)
    {
        if (glasses_Slot.transform.childCount > 0)
        {
            Destroy(glasses_Slot.transform.GetChild(0).gameObject);
        }

        if (glassesStyleInd == 0)
        {
            return;
        }

        for (int i = 0; i < glassesPrefabs.Length; i++)
        {
            if (glassesPrefabs[i].name == glassesStyleInd.ToString())
            {
                GameObject glasses = Instantiate(glassesPrefabs[i]);
                glasses.transform.parent = glasses_Slot.transform;
                glasses.transform.localPosition = Vector3.zero;
                glasses.transform.localRotation = Quaternion.identity;
                glasses.transform.localScale = Vector3.one;
                break;
            }
        }
    }

    protected void SetBrowsStyle(int browsStyleInd)
    {
        browsStyleInd = Mathf.Clamp(browsStyleInd, 0, browsTextures.Length - 1);

        skinMaterial.SetTexture("_BrowsTexture", null);
        for (int i = 0; i < browsTextures.Length; i++)
        {
            if (browsTextures[i].name == browsStyleInd.ToString())
            {
                skinMaterial.SetTexture("_BrowsTexture", browsTextures[i]);
                break;
            }
        }
    }

    protected void SetEyeStyle(int eyeStyleInd)
    {
        eyeStyleInd = Mathf.Clamp(eyeStyleInd, 0, 3);

        for (int i = 1; i < 5; i++)
        {
            int ind = skin_Renderer.sharedMesh.GetBlendShapeIndex("eyes" + i.ToString());

            if (ind != -1)
                skin_Renderer.SetBlendShapeWeight(ind, i == eyeStyleInd ? 100 : 0);
        }
    }

    protected void SetNoseStyle(int noseStyleInd)
    {
        noseStyleInd = Mathf.Clamp(noseStyleInd, 0, 4);

        for(int i = 1;i < 5; i++)
        {
            int ind = skin_Renderer.sharedMesh.GetBlendShapeIndex("nose" + i.ToString());

            if(ind != -1)
                skin_Renderer.SetBlendShapeWeight(ind, i == noseStyleInd ? 100 : 0);
        }
    }

    protected void SetMouthStyle(int mouthStyleInd)
    {
        mouthStyleInd = Mathf.Clamp(mouthStyleInd, 0, 3);

        for (int i = 1; i < 5; i++)
        {
            int ind = skin_Renderer.sharedMesh.GetBlendShapeIndex("mouth" + i.ToString());

            if (ind != -1)
                skin_Renderer.SetBlendShapeWeight(ind, i == mouthStyleInd ? 100 : 0);
        }
    }

    protected void SetSkinImperfection(int skinImperfectionInd)
    {
        skinImperfectionInd = Mathf.Clamp(skinImperfectionInd, 0, skinImperfectionTextures.Length - 1);

        skinMaterial.SetTexture("_FaceImperfectionTexture", null);
        for (int i = 0; i < skinImperfectionTextures.Length; i++)
        {
            if (skinImperfectionTextures[i].name == skinImperfectionInd.ToString())
            {
                skinMaterial.SetTexture("_FaceImperfectionTexture", skinImperfectionTextures[i]);
                break;
            }
        }
    }

    protected void SetTattoo(int tattooInd)
    {
        tattooInd = Mathf.Clamp(tattooInd, 0, tattooTextures.Length - 1);

        skinMaterial.SetTexture("_FaceTattooTexture", null);
        for (int i = 0; i < tattooTextures.Length; i++)
        {
            if (tattooTextures[i].name == tattooInd.ToString())
            {
                skinMaterial.SetTexture("_FaceTattooTexture", tattooTextures[i]);
                break;
            }
        }
    }

    protected void SetHairColor(string colorHex)
    {
        if (colorHex[0] != '#')
        {
            colorHex = "#" + colorHex;
        }
        Color color;
        if (ColorUtility.TryParseHtmlString(colorHex, out color) && hair_Renderer)
        {
            hair_Renderer.material.color = color;
            skinMaterial.SetColor("_HairColor", color);
        }
    }

    protected void SetBrowsColor(string colorHex)
    {
        if (colorHex[0] != '#')
        {
            colorHex = "#" + colorHex;
        }
        Color color;
        if (ColorUtility.TryParseHtmlString(colorHex, out color) && skinMaterial)
        {
            skinMaterial.SetColor("_BrowsColor", color);
        }
    }

    protected void SetSkinColor(string colorHex)
    {
        if (colorHex[0] != '#')
        {
            colorHex = "#" + colorHex;
        }
        Color color;
        if (ColorUtility.TryParseHtmlString(colorHex, out color) && skinMaterial)
        {
            skinMaterial.SetColor("_SkinColor", color);
        }
    }

    protected void SetEyesColor(string colorHex)
    {
        if (colorHex[0] != '#')
        {
            colorHex = "#" + colorHex;
        }
        Color color;
        if (ColorUtility.TryParseHtmlString(colorHex, out color) && eyesMaterial)
        {
            eyesMaterial.SetColor("_IrisColor", color);
        }
    }
}
