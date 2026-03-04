using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace FBXEditor.Editor
{
    // This struct holds the flattened mesh data required by the job.
    public struct MeshData
{
    public int vertexOffset;
    public int triangleOffset;
    public float4x4 localToWorldMatrix;
}

[BurstCompile]
public struct RaycastJob : IJobParallelFor
{
    // --- Input Data ---
    [ReadOnly] public NativeArray<Ray> rays;
    [ReadOnly] public NativeArray<float3> vertices;
    [ReadOnly] public NativeArray<int> triangles;
    [ReadOnly] public NativeArray<MeshData> meshData;

    // --- Output Data ---
    // Each element will store the combined index of the mesh and triangle that was hit.
    public NativeArray<int2> results;

    public void Execute(int index)
    {
        Ray ray = rays[index];
        float closestDist = float.MaxValue;
        int2 hitResult = new int2(-1, -1); // x = meshIndex, y = triangleIndex

        for (int meshIndex = 0; meshIndex < meshData.Length; meshIndex++)
        {
            var currentMesh = meshData[meshIndex];
            int triangleEnd = (meshIndex + 1 < meshData.Length) ? meshData[meshIndex + 1].triangleOffset : triangles.Length;

            for (int triBaseIndex = currentMesh.triangleOffset; triBaseIndex < triangleEnd; triBaseIndex += 3)
            {
                int i0 = triangles[triBaseIndex + 0] + currentMesh.vertexOffset;
                int i1 = triangles[triBaseIndex + 1] + currentMesh.vertexOffset;
                int i2 = triangles[triBaseIndex + 2] + currentMesh.vertexOffset;

                float3 v0 = math.transform(currentMesh.localToWorldMatrix, vertices[i0]);
                float3 v1 = math.transform(currentMesh.localToWorldMatrix, vertices[i1]);
                float3 v2 = math.transform(currentMesh.localToWorldMatrix, vertices[i2]);

                if (RayIntersectsTriangleBurst(ray, v0, v1, v2, out float dist))
                {
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        hitResult = new int2(meshIndex, (triBaseIndex - currentMesh.triangleOffset) / 3);
                    }
                }
            }
        }
        results[index] = hitResult;
    }

    /// <summary>
    /// Burst-compatible version of the Möller–Trumbore ray-triangle intersection algorithm.
    /// </summary>
    private bool RayIntersectsTriangleBurst(Ray ray, float3 v0, float3 v1, float3 v2, out float distance)
    {
        distance = 0;
        const float epsilon = 0.0000001f;

        // Convert Ray properties to float3 for consistent math operations
        float3 rayOrigin = ray.origin;
        float3 rayDirection = ray.direction;

        float3 e1 = v1 - v0;
        float3 e2 = v2 - v0;
        float3 h = math.cross(rayDirection, e2);
        float a = math.dot(e1, h);

        if (a > -epsilon && a < epsilon)
            return false;

        float f = 1.0f / a;
        float3 s = rayOrigin - v0;
        float u = f * math.dot(s, h);

        if (u < 0.0f || u > 1.0f)
            return false;

        float3 q = math.cross(s, e1);
        float v = f * math.dot(rayDirection, q);

        if (v < 0.0f || u + v > 1.0f)
            return false;

        float t = f * math.dot(e2, q);

        if (t < epsilon)
            return false;

        distance = t;
        return true;
    }
    }
}