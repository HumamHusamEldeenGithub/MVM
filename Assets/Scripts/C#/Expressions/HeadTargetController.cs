using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadTargetController : MonoBehaviour
{
    [SerializeField]
    private float topPos;

    [SerializeField]
    private float rightPos;

    [SerializeField]
    private OrientationProcessor orientationProcessor;

    private void Update()
    {
        if (!orientationProcessor.isReady)
        {
            return;
        }


        var xangle = 1 - ((orientationProcessor.X_ANGLE + 60) / 120);
        var yangle = 1 - ((orientationProcessor.Y_ANGLE + 60) / 120);

        transform.localPosition = Vector3.Lerp(transform.localPosition, new Vector3(Mathf.Lerp(-rightPos, rightPos, Mathf.Clamp01(yangle)),
            transform.localPosition.y, Mathf.Lerp(-topPos, topPos, Mathf.Clamp01(xangle))), Time.deltaTime * 25f);
    }
}
