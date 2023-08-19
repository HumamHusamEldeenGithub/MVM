using ArabicSupport;
using TMPro;
using UnityEngine;

public class ContinuosInputFieldArabicFixer : MonoBehaviour
{
    private TMP_InputField inputFieldComponent;

    [SerializeField]
    private TextMeshProUGUI textComponent;
    void Awake()
    {
        inputFieldComponent = GetComponent<TMP_InputField>();
        inputFieldComponent.onValueChanged.AddListener(Fix);
    }

    void Fix(string s)
    {
        textComponent.text = ArabicFixer.Fix(s);
    }
}
