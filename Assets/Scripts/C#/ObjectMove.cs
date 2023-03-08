using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectMove : MonoBehaviour
{
    float width, height;

    Canvas canvas;

    private void Awake()
    {
        canvas = transform.root.GetComponentInChildren<Canvas>();
        width = canvas.GetComponent<RectTransform>().rect.width;
        height = canvas.GetComponent<RectTransform>().rect.height;
    }
    private void Update()
    {
        transform.localPosition = new Vector2(GeneralManager.point.x * width, GeneralManager.point.y * height);
    }
}
