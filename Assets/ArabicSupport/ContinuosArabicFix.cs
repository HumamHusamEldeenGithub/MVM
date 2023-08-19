using ArabicSupport;
using TMPro;
using UnityEngine;

public class ContinuosArabicFix : MonoBehaviour
{
    TextMeshProUGUI textComponent;
    void Awake()
    {
        textComponent = GetComponent<TextMeshProUGUI>();
    }

    void LateUpdate()
    {
        textComponent.text = ArabicFixer.Fix(textComponent.text);
    }
}
