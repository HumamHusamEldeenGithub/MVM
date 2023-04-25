using System.Collections.Generic;
using UnityEngine;

public class FaceAnimator : MonoBehaviour
{
    [SerializeField] Transform bone;

    [SerializeField] Transform eye_Left;
    [SerializeField] Transform eye_Right;

    [SerializeField]
    Dictionary<MouthExpression, Sprite> mouthExpressions = new Dictionary<MouthExpression, Sprite>();

    private Quaternion originalRotation;

    private void Awake()
    {
        originalRotation = bone.rotation;
    }

    private void Update()
    {
        if (!OrientationProcessor.isReady)
            return;

        bone.rotation = Quaternion.Lerp(bone.rotation, originalRotation * Quaternion.Euler(
            OrientationProcessor.X_ANGLE,
            OrientationProcessor.Y_ANGLE,
            0), 0.75f);

        eye_Right.transform.localScale = OrientationProcessor.Eye_Right_Open ? Vector3.one : Vector3.Lerp(eye_Right.localScale, new Vector3(1, 1, 0.1f), 0.8f);
        eye_Left.transform.localScale = OrientationProcessor.Eye_Left_Open ? Vector3.one : Vector3.Lerp(eye_Left.localScale, new Vector3(1, 1, 0.1f), 0.8f);

    }
}
