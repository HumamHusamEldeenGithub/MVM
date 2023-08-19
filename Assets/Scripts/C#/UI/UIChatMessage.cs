using ArabicSupport;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIChatMessage : MonoBehaviour
{
    [SerializeField]
    private RectTransform backgroundPanel;

    [SerializeField]
    private TextMeshProUGUI textComponent;

    private RectTransform messageRectTransform;

    public void SetText(string text)
    {
        IEnumerator set()
        {
            textComponent.text = ArabicFixer.Fix(text);

            messageRectTransform = GetComponent<RectTransform>();
            LayoutRebuilder.ForceRebuildLayoutImmediate(backgroundPanel);

            yield return null;

            messageRectTransform.sizeDelta = new Vector2(messageRectTransform.sizeDelta.x, LayoutUtility.GetPreferredHeight(backgroundPanel));
        }
        StartCoroutine(set());
    } 
}
