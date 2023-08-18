using System.Collections;
using TMPro;
using UnityEditor.Overlays;
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
        Debug.Log("Ya 5ra");
        IEnumerator set()
        {
            textComponent.text = text;

            messageRectTransform = GetComponent<RectTransform>();
            LayoutRebuilder.ForceRebuildLayoutImmediate(backgroundPanel);

            yield return null;

            Debug.LogError(LayoutUtility.GetPreferredHeight(backgroundPanel));

            messageRectTransform.sizeDelta = new Vector2(messageRectTransform.sizeDelta.x, LayoutUtility.GetPreferredHeight(backgroundPanel));
        }
        StartCoroutine(set());
    } 
}
