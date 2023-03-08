using UnityEngine;
using UnityEngine.UI;

public class CursorMove : MonoBehaviour
{
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
        if (!OrientationProcessor.isReady)
            return;
        transform.localPosition = new Vector2(
            OrientationProcessor.NosePoint.x * width,
            OrientationProcessor.NosePoint.y * height);
        img.color = OrientationProcessor.MouthOpened ? Color.red : Color.white;
    }
}
