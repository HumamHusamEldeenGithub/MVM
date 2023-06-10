using Mono.Cecil.Cil;
using UnityEngine;
using UnityEngine.UI;

public class CursorMove : MonoBehaviour
{
    [SerializeField]
    OrientationProcessor orProcessor;
    float width, height;

    Canvas canvas;
    Image img;

    private void Awake()
    {
        img = GetComponent<Image>();
        canvas = transform.root.GetComponentInChildren<Canvas>();
        width = canvas.GetComponent<RectTransform>().rect.width;
        height = canvas.GetComponent<RectTransform>().rect.height;
    }

    private void Update()
    {
        if (!orProcessor?.isReady == false)
            return;

        transform.localPosition = new Vector2(
            orProcessor.NosePoint.x * width,
            orProcessor.NosePoint.y * height);
        img.color = orProcessor.MouthOpened ? Color.red : Color.white;
    }
}
