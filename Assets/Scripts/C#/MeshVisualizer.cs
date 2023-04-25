using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MeshVisualizer : MonoBehaviour
{

    SkinnedMeshRenderer mesh;
    public float selectionRadius = 0.02f;
    public Color selectionColor = Color.red;
    private int[] selectedVertexIndex = { -1, -1, -1 };
    private Camera cm;

    private void Awake()
    {
        cm = Camera.main;
    }
    private void OnDrawGizmos()
    {

        if (mesh == null)
        {
            mesh = GetComponentInChildren<SkinnedMeshRenderer>();
        }

        Gizmos.color = Color.red;
        Handles.color = Color.green;
        for (int i = 0; i < mesh.sharedMesh.vertexCount; i++)
        {
            if (i != selectedVertexIndex[0] && i != selectedVertexIndex[1] && i != selectedVertexIndex[2])
                continue;
            var vertex = mesh.sharedMesh.vertices[i];
            var worldPos = mesh.transform.TransformPoint(vertex);
            Handles.color = Color.green;
            Handles.Label(mesh.transform.TransformPoint(vertex), i.ToString());

            Gizmos.color = selectionColor;
            Gizmos.DrawSphere(worldPos, selectionRadius);
        }
    }
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = cm.ScreenPointToRay(Input.mousePosition);
            RaycastHit[] hits = Physics.RaycastAll(ray);
            float minDistance = Mathf.Infinity;
            int closestVertexIndex = -1;

            for (int i = 0; i < hits.Length; i++)
            {
                RaycastHit hit = hits[i];
                if (hit.collider == GetComponent<Collider>())
                {
                    MeshCollider meshCollider = hit.collider as MeshCollider;
                    if (meshCollider == null) continue;

                    int[] triangles = mesh.sharedMesh.triangles;
                    Vector3[] vertices = mesh.sharedMesh.vertices;

                    for (int j = 0; j < triangles.Length; j += 3)
                    {
                        Vector3 p1 = transform.TransformPoint(vertices[triangles[j]]);
                        Vector3 p2 = transform.TransformPoint(vertices[triangles[j + 1]]);
                        Vector3 p3 = transform.TransformPoint(vertices[triangles[j + 2]]);

                        float distance = DistanceToTriangle(ray.origin, ray.direction, p1, p2, p3);
                        if (distance < minDistance)
                        {
                            minDistance = distance;
                            closestVertexIndex = triangles[j];
                            selectedVertexIndex[0] = triangles[j];

                            selectedVertexIndex[1] = triangles[j+1];
                            selectedVertexIndex[2] = triangles[j+2];
                        }
                    }
                }
            }

        }
    }
    float DistanceToTriangle(Vector3 origin, Vector3 direction, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        Vector3 e1 = p2 - p1;
        Vector3 e2 = p3 - p1;
        Vector3 normal = Vector3.Cross(e1, e2).normalized;
        float d = -Vector3.Dot(normal, p1);
        float denominator = Vector3.Dot(normal, direction);
        if (Mathf.Approximately(denominator, 0f)) return Mathf.Infinity;
        float t = -(Vector3.Dot(normal, origin) + d) / denominator;
        if (t < 0f) return Mathf.Infinity;
        Vector3 intersection = origin + t * direction;
        Vector3 v1 = p2 - p1;
        Vector3 v2 = p3 - p2;
        Vector3 v3 = p1 - p3;
        Vector3 i1 = intersection - p1;
        Vector3 i2 = intersection - p2;
        Vector3 i3 = intersection - p3;
        Vector3 c1 = Vector3.Cross(v1, i1);
        Vector3 c2 = Vector3.Cross(v2, i2);
        Vector3 c3 = Vector3.Cross(v3, i3);
        if (Vector3.Dot(c1, c2) >= 0f && Vector3.Dot(c2, c3) >= 0f && Vector3.Dot(c3, c1) >= 0f)
        {
            return Vector3.Distance(intersection, origin);
        }
        else
        {
            return Mathf.Infinity;
        }
    }
}
