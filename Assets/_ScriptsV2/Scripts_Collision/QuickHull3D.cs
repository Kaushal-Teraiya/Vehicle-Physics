using System.Collections.Generic;
using UnityEngine;

public class QuickHull3D
{
    public static Mesh GenerateHull(Vector3[] points)
    {
        QuickHull hull = new QuickHull();
        hull.Build(points);
        return hull.ToMesh();
    }

    // -------------- INTERNAL QUICKHULL CLASS ------------------

    private class Face
    {
        public int a, b, c;
        public Vector3 normal;

        public Face(int a, int b, int c, Vector3[] pts)
        {
            this.a = a;
            this.b = b;
            this.c = c;
            normal = Vector3.Normalize(
                Vector3.Cross(pts[b] - pts[a], pts[c] - pts[a])
            );
        }

        public bool IsPointAbove(Vector3 p, Vector3[] pts)
        {
            Vector3 point = pts[a];
            return Vector3.Dot(normal, p - point) > 1e-6f;
        }
    }

    private class QuickHull
    {
        List<Face> faces;
        Vector3[] points;

        public void Build(Vector3[] pts)
        {
            points = pts;
            faces = new List<Face>();

            if (pts.Length < 4)
            {
                Debug.LogError("Convex hull needs >= 4 points.");
                return;
            }

            BuildInitialTetrahedron();
            ExpandHull();
        }

        // Create initial tetrahedron
        private void BuildInitialTetrahedron()
        {
            int i0 = 0;
            int i1 = FindFurthestPoint(i0);
            int i2 = FindFurthestPoint(i1);
            int i3 = FindPointWithMaxVolume(i0, i1, i2);

            faces.Add(new Face(i0, i1, i2, points));
            faces.Add(new Face(i0, i2, i3, points));
            faces.Add(new Face(i0, i3, i1, points));
            faces.Add(new Face(i1, i3, i2, points));
        }

        private int FindFurthestPoint(int origin)
        {
            int idx = origin;
            float maxDist = -1f;
            for (int i = 0; i < points.Length; i++)
            {
                float d = (points[i] - points[origin]).sqrMagnitude;
                if (d > maxDist)
                {
                    maxDist = d;
                    idx = i;
                }
            }
            return idx;
        }

        private int FindPointWithMaxVolume(int a, int b, int c)
        {
            int idx = 0;
            float maxVol = -1f;

            for (int i = 0; i < points.Length; i++)
            {
                float vol = Mathf.Abs(Vector3.Dot(
                    Vector3.Cross(points[b] - points[a], points[c] - points[a]),
                    points[i] - points[a]
                ));

                if (vol > maxVol)
                {
                    idx = i;
                    maxVol = vol;
                }
            }

            return idx;
        }

        private void ExpandHull()
        {
            bool expanded = true;

            while (expanded)
            {
                expanded = false;

                for (int i = 0; i < points.Length; i++)
                {
                    if (PointOutsideHull(points[i]))
                    {
                        AddPointToHull(i);
                        expanded = true;
                        break;
                    }
                }
            }
        }

        private bool PointOutsideHull(Vector3 p)
        {
            foreach (var f in faces)
                if (f.IsPointAbove(p, points))
                    return true;
            return false;
        }

        private void AddPointToHull(int pointIndex)
        {
            List<Face> newFaces = new List<Face>();
            List<(int,int)> horizon = new List<(int,int)>();

            // Remove faces visible to the point and collect horizon edges
            foreach (var f in faces)
            {
                if (!f.IsPointAbove(points[pointIndex], points))
                {
                    newFaces.Add(f);
                }
                else
                {
                    CollectHorizonEdges(f, pointIndex, horizon);
                }
            }

            // Add new faces from horizon
            foreach (var e in horizon)
                newFaces.Add(new Face(e.Item1, e.Item2, pointIndex, points));

            faces = newFaces;
        }

        private void CollectHorizonEdges(Face f, int pIdx, List<(int,int)> horizon)
        {
            // For simplicity: treat face edges as horizon
            horizon.Add((f.a, f.b));
            horizon.Add((f.b, f.c));
            horizon.Add((f.c, f.a));
        }

        public Mesh ToMesh()
        {
            Mesh m = new Mesh();
            List<Vector3> verts = new List<Vector3>();
            List<int> tris = new List<int>();

            foreach (var f in faces)
            {
                int start = verts.Count;
                verts.Add(points[f.a]);
                verts.Add(points[f.b]);
                verts.Add(points[f.c]);

                tris.Add(start);
                tris.Add(start + 1);
                tris.Add(start + 2);
            }

            m.SetVertices(verts);
            m.SetTriangles(tris, 0);
            m.RecalculateNormals();
            return m;
        }
    }
}
