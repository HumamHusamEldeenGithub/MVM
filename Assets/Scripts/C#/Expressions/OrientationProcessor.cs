using Mvm;
using System.Collections.Generic;
using UnityEngine;

public class OrientationProcessor : MonoBehaviour
{
    [Range(0.1f, 1.0f)]
    private float AveragingValue = 0.9f;

    private List<Vector3> points;

    private float FaceArea;

    private float maxDistance = 0.05f;

    public bool isReady
    {
        get { return points != null; }
    }

    public void SetPoints(List<Keypoint> kpoints)
    {
        if (points == null)
        {
            points = new List<Vector3>();
            for (int i = 0; i < kpoints.Count; i++)
            {
                var kpt = kpoints[i];
                var pt = new Vector3(kpt.X, kpt.Y, kpt.Z);
                pt.x = 2 * pt.x - 1;
                pt.y = 2 * pt.y - 1;
                pt *= -1;

                points.Add(pt);
            }
        }
        else
        {
            for (int i = 0; i < kpoints.Count; i++)
            {
                var kpt = kpoints[i];
                var pt = new Vector3(kpt.X, kpt.Y, kpt.Z);
                pt.x = 2 * pt.x - 1;
                pt.y = 2 * pt.y - 1;
                pt.z = 2 * pt.z - 1;
                pt *= -1;

                float dis = (points[i] - pt).magnitude;

                float distance = Vector3.Distance(points[i], pt);

                // If the distance is greater than the maximum distance, clamp the newPosition to be within the maximum distance from the originalPosition
                if (distance > maxDistance)
                {
                    pt = points[i] + (pt - points[i]).normalized * maxDistance;
                }

                // Set the position of the object to the new position

                points[i] = Vector3.Lerp(points[i], pt, AveragingValue);
            }
        }
        FaceArea = Vector3.Distance(points[3], points[1]);
        //Debug.Log("Area is " + FaceArea);
    }

    public float Y_ANGLE
    {
        get
        {
            var Angle = Vector3.Angle(
                    points[2] - points[0],
                    Vector3.left);

            Angle *= points[0].z > points[2].z ? 1 : -1;

            return Angle;
        }
    }

    public float X_ANGLE
    {
        get
        {
            var Angle = Vector3.Angle(
                    points[1] - points[3],
                    Vector3.down);

            Angle *= points[3].z > points[1].z ? 1 : -1;

            return Angle;
        }
    }

}
