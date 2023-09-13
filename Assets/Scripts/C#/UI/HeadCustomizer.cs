using Mvm;
using UnityEngine;

public class HeadCustomizer : MonoBehaviour
{
    [SerializeField]
    private AvatarCustomizer maleFace;

    [SerializeField]
    private AvatarCustomizer femaleFace;

    private AvatarCustomizer currentFace;

    public void SetSettings(AvatarSettings settings)
    {
        if(settings.Gender.ToLower() == "male")
        {
            currentFace = maleFace;
            currentFace.gameObject.SetActive(true);
            femaleFace.gameObject.SetActive(false);
        }
        else
        {
            currentFace = femaleFace;
            currentFace.gameObject.SetActive(true);
            maleFace.gameObject.SetActive(false);
        }
        currentFace.SetAvatarSettings(settings);
    }

    public void OnRotate(float value)
    {
        transform.rotation = Quaternion.Euler(0, value, 0);
    }
}
