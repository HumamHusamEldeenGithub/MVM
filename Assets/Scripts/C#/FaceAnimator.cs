using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceAnimator : MonoBehaviour
{

    [SerializeField] Transform bone;

    private Quaternion originalRotation;


    private void Awake()
    {
        originalRotation = bone.rotation;
    }
    private void Update()
    {
        if (OrientationProcessor.isReady)
        {
            bone.rotation = originalRotation * Quaternion.Euler(0,
                OrientationProcessor.Y_ANGLE,
                OrientationProcessor.Z_ANGLE);
        }
    }
}
