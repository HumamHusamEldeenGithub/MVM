using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DynamicPopup : MonoBehaviour
{
    [SerializeField]
    private Button closeButton;

    [SerializeField]
    private TextMeshProUGUI textMeshPro;

    public void Initialize(string textToShow, float secondsToDestroy)
    {
        textMeshPro.text = textToShow;
        StopAllCoroutines();
        StartCoroutine(Hide(secondsToDestroy));
    }



    private IEnumerator Hide(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        gameObject.SetActive(false);
    }
}