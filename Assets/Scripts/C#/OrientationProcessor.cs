using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class OrientationProcessor
{
    public static List<Vector3> points;

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
                pt *= -1;

                points[i] = pt;
            }
        }
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

            return Angle;
        }
    }

    public static float Z_ANGLE
    {
        get
        {
            var Angle = Vector3.Angle(
                    points[152] - points[10],
                    Vector3.down);

            Angle *= points[152].z > points[10].z ? 1 : -1;

            return Angle;
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
