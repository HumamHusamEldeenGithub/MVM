using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MouthExpression
{
    Normal_Closed,
    Normal_Open,

    Smiling_Closed,
    Smiling_Open,

    Upset_Closed,
    Upset_Open

}

public static class OrientationProcessor
{
    [Range(0.1f, 1.0f)]
    public static float AveragingValue = 0.5f;

    public static List<Vector3> points;

    public static float FaceArea;

    private static float maxDistance = 0.05f;

    public static bool isReady
    {
        get { return points != null; }
    }

    public static void SetPoints(Keypoints kpoints)
    {
        if (points == null)
        {
            points = new List<Vector3>();
            for (int i = 0; i < kpoints.Points.Count; i++)
            {
                var kpt = kpoints.Points[i];
                var pt = new Vector3(kpt.X, kpt.Y, kpt.Z);
                pt.x = 2 * pt.x - 1;
                pt.y = 2 * pt.y - 1;
                pt *= -1;

                points.Add(pt);
            }
        }
        else
        {
            for (int i = 0; i < kpoints.Points.Count; i++)
            {
                var kpt = kpoints.Points[i];
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
        FaceArea = Vector3.Distance(points[10], points[152]);
        Debug.Log("Area is " + FaceArea);
    }

    public static Vector3 NosePoint
    {
        get
        {
            return points[1];
        }
    }

    public static float Y_ANGLE
    {
        get
        {
            var Angle = Vector3.Angle(
                    points[454] - points[234],
                    Vector3.left);

            Angle *= points[234].z > points[454].z ? 1 : -1;

            return Angle / FaceArea;
        }
    }

    public static float X_ANGLE
    {
        get
        {
            var Angle = Vector3.Angle(
                    points[152] - points[10],
                    Vector3.down);

            Angle *= points[10].z > points[152].z ? 1 : -1;

            return Angle / FaceArea;
        }
    }

    public static bool Eye_Left_Open
    {
        get
        {
            Debug.Log((points[159] - points[145]).magnitude / FaceArea);
            return (points[159] - points[145]).magnitude / FaceArea > 0.02f;
        }
    }
    public static bool Eye_Right_Open
    {
        get
        {
            Debug.Log((points[386] - points[374]).magnitude / FaceArea);
            return (points[386] - points[374]).magnitude / FaceArea > 0.02f;
        }
    }

    public static bool MouthOpened
    {
        get
        {
            return (points[13] - points[14]).magnitude > 0.0005f;
        }
    }

}
