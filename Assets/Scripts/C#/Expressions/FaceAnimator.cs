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
    private OrientationProcessor orProcessor;

    private void Awake()
    {
        originalRotation = bone.rotation;

        orProcessor = GetComponent<OrientationProcessor>();
    }

    private void Update()
    {
        if (!orProcessor.isReady)
            return;

        bone.rotation = Quaternion.Lerp(bone.rotation, originalRotation * Quaternion.Euler(
            orProcessor.X_ANGLE,
            orProcessor.Y_ANGLE,
            0), 0.75f);

        eye_Right.transform.localScale = orProcessor.Eye_Right_Open ? Vector3.one : Vector3.Lerp(eye_Right.localScale, new Vector3(1, 1, 0.1f), 0.8f);
        eye_Left.transform.localScale = orProcessor.Eye_Left_Open ? Vector3.one : Vector3.Lerp(eye_Left.localScale, new Vector3(1, 1, 0.1f), 0.8f);

    }
}
