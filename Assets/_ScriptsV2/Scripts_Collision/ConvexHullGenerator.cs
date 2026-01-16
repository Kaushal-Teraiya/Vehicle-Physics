using System.Collections.Generic;
using UnityEngine;

public static class ConvexHullGenerator
{
    public static Mesh GenerateHull(Mesh sourceMesh)
    {
        Vector3[] points = sourceMesh.vertices;
        if (points.Length < 4)
        {
            Debug.LogError("Convex Hull requires at least 4 points.");
            return null;
        }

        // QuickHull main
        List<Vector3> hullPoints = QuickHull(points);

        return BuildMeshFromPoints(hullPoints);
    }

    // -------------------------
    // QUICKHULL IMPLEMENTATION
    // -------------------------
    private static List<Vector3> QuickHull(Vector3[] points)
    {
        List<Vector3> hull = new List<Vector3>();

        // 1. Find extreme points
        Vector3 minX = points[0],
            maxX = points[0];
        foreach (var p in points)
        {
            if (p.x < minX.x)
                minX = p;
            if (p.x > maxX.x)
                maxX = p;
        }

        hull.Add(minX);
        hull.Add(maxX);

        // 2. Split points into two sets
        List<Vector3> leftSet = new List<Vector3>();
        List<Vector3> rightSet = new List<Vector3>();

        foreach (var p in points)
        {
            if (p.Equals(minX) || p.Equals(maxX))
                continue;

            if (PointSide(minX, maxX, p) > 0)
                leftSet.Add(p);
            else
                rightSet.Add(p);
        }

        // 3. Recursively build hull
        BuildHull(minX, maxX, leftSet, hull);
        BuildHull(maxX, minX, rightSet, hull);

        return hull;
    }

    private static float PointSide(Vector3 a, Vector3 b, Vector3 p)
    {
        return Mathf.Sign((b.x - a.x) * (p.y - a.y) - (b.y - a.y) * (p.x - a.x));
    }

    private static void BuildHull(Vector3 a, Vector3 b, List<Vector3> set, List<Vector3> hull)
    {
        if (set.Count == 0)
            return;

        // Find farthest point from line AB
        float dist = float.MinValue;
        Vector3 farthest = Vector3.zero;

        foreach (var p in set)
        {
            float d = DistanceFromLine(a, b, p);
            if (d > dist)
            {
                dist = d;
                farthest = p;
            }
        }

        hull.Add(farthest);

        // Partition points again
        List<Vector3> leftA = new List<Vector3>();
        List<Vector3> leftB = new List<Vector3>();

        foreach (var p in set)
        {
            if (p.Equals(farthest))
                continue;

            if (PointSide(a, farthest, p) > 0)
                leftA.Add(p);

            if (PointSide(farthest, b, p) > 0)
                leftB.Add(p);
        }

        BuildHull(a, farthest, leftA, hull);
        BuildHull(farthest, b, leftB, hull);
    }

    private static float DistanceFromLine(Vector3 a, Vector3 b, Vector3 p)
    {
        return Mathf.Abs((b.x - a.x) * (a.y - p.y) - (a.x - p.x) * (b.y - a.y));
    }

    // -------------------------
    // MESH BUILDER
    // -------------------------
    private static Mesh BuildMeshFromPoints(List<Vector3> hullPoints)
    {
        Mesh hull = new Mesh();
        hull.name = "ConvexHull";

        // Triangulate points into a convex mesh
        // Unity built-in Convex Hull to mesh using QuickHull points
        var vertices = hullPoints.ToArray();
        var triangles = Triangulate(vertices);

        hull.vertices = vertices;
        hull.triangles = triangles;
        hull.RecalculateNormals();
        hull.RecalculateBounds();

        return hull;
    }

    // SUPER SIMPLE fan triangulation
    private static int[] Triangulate(Vector3[] verts)
    {
        List<int> tris = new List<int>();
        for (int i = 1; i < verts.Length - 1; i++)
        {
            tris.Add(0);
            tris.Add(i);
            tris.Add(i + 1);
        }
        return tris.ToArray();
    }
}
