using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HullDelaunayVoronoi.Voronoi;
using HullDelaunayVoronoi.Delaunay;
using HullDelaunayVoronoi.Hull;
using HullDelaunayVoronoi.Primitives;

public class VoronoiMesh {
    private static bool Filter(Vector3 region, Vertex3 filterable, float scale, float rad) {
        float x = filterable.X / region.x;
        float y = filterable.Y / region.y;
        float z = filterable.Z / region.z;

        float r = scale * rad;

        return x*x + y*y + z*z <= r*r;
    }

    private static bool InBound(Vertex3 v, float scale) {
        if (v.X < -scale || v.X > scale) return false;
        if (v.Y < -scale || v.Y > scale) return false;
        if (v.Z < -scale || v.Z > scale) return false;

        return true;
    }

    public static (List<Mesh> meshes, List<Vector3> centers) Get(int nverts, float scale, int seed, Material mat, float rad, Vector3 size) {
        Vertex3[] vertices = new Vertex3[nverts];

        Random.InitState(seed);
        for (int i = 0; i < nverts; i++) {
            float x = scale * Random.Range(-1.0f, 1.0f);
            float y = scale * Random.Range(-1.0f, 1.0f);
            float z = scale * Random.Range(-1.0f, 1.0f);

            vertices[i] = new Vertex3(x, y, z);
        }

        VoronoiMesh3 voronoi = new VoronoiMesh3();
        voronoi.Generate(vertices);

        return RegionsToMeshes(voronoi, scale, seed, mat, rad, size);
    }

    private static (List<Mesh> meshes, List<Vector3> centers) RegionsToMeshes(VoronoiMesh3 voronoi, float scale, int seed, Material mat, float rad, Vector3 size) {
        List<Mesh> meshes = new List<Mesh>();
        List<Vector3> centers = new List<Vector3>();

        foreach (VoronoiRegion<Vertex3> region in voronoi.Regions) {
            bool draw = true;

            Vertex3 center = region.ArithmeticCenter;
            if (!Filter(size, center, scale, rad))
                continue;

            List<Vertex3> verts = new List<Vertex3>();

            foreach (DelaunayCell<Vertex3> cell in region.Cells) {
                if (!InBound(cell.CircumCenter, scale)) {
                    draw = false;
                    break;
                }

                else {
                    verts.Add(cell.CircumCenter);
                }
            }

            if (!draw)
                continue;

            // If you find the convex hull of the voronoi region it
            // can be used to make a triangle mesh.
            ConvexHull3 hull = new ConvexHull3();
            hull.Generate(verts, false);

            List<Vector3> positions = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<int> indices = new List<int>();

            for (int i = 0; i < hull.Simplexs.Count; i++) {
                for (int j = 0; j < 3; j++) {
                    Vector3 v = new Vector3();
                    v.x = hull.Simplexs[i].Vertices[j].X;
                    v.y = hull.Simplexs[i].Vertices[j].Y;
                    v.z = hull.Simplexs[i].Vertices[j].Z;

                    positions.Add(v);
                }

                Vector3 n = new Vector3();
                n.x = hull.Simplexs[i].Normal[0];
                n.y = hull.Simplexs[i].Normal[1];
                n.z = hull.Simplexs[i].Normal[2];

                if (hull.Simplexs[i].IsNormalFlipped) {
                    indices.Add(i * 3 + 2);
                    indices.Add(i * 3 + 1);
                    indices.Add(i * 3 + 0);
                }

                else {
                    indices.Add(i * 3 + 0);
                    indices.Add(i * 3 + 1);
                    indices.Add(i * 3 + 2);
                }

                normals.Add(n);
                normals.Add(n);
                normals.Add(n);
            }

            Mesh mesh = new Mesh();
            mesh.SetVertices(positions);
            mesh.SetNormals(normals);
            mesh.SetTriangles(indices, 0);

            mesh.RecalculateBounds();
            mesh.RecalculateNormals();

            meshes.Add(mesh);
            centers.Add(new Vector3(center.X, center.Y, center.Z));
        }

        return (meshes, centers);
    }
}
