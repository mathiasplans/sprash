using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshNode {
    private Mesh mesh;
    private List<MeshNode> children;
    private Vector3 position;
    private Vector3 centroid;
    public MeshCollider collider = null;

    private int size = 1;

    public MeshNode(Mesh mesh, Vector3 position, Vector3 centroid) {
        this.mesh = mesh;
        this.position = position;
        this.children = new List<MeshNode>();
        this.centroid = centroid;
    }

    public MeshNode(Mesh mesh, Vector3 position, Vector3 centroid, MeshCollider collider) {
        this.mesh = mesh;
        this.position = position;
        this.children = new List<MeshNode>();
        this.centroid = centroid;
        this.collider = collider;
    }

    private struct Triangle {
        Vector3 v1, v2, v3;

        public Triangle(Vector3 v1, Vector3 v2, Vector3 v3) {
            this.v1 = v1;
            this.v2 = v2;
            this.v3 = v3;
        }

        public override int GetHashCode() {
            return this.v1.GetHashCode() ^ (2 * this.v2).GetHashCode() ^ (3 * this.v3).GetHashCode();
        }
    }

    public MeshNode(Mesh[] meshes, MeshCollider collider) {
        // CombineInstance[] combine = new CombineInstance[meshes.Length];

        // for (int i = 0; i < meshes.Length; ++i) {
        //     combine[i].mesh = meshes[i];
        // }

        // this.mesh = new Mesh();
        // this.mesh.CombineMeshes(combine, false);
        // this.mesh.RecalculateNormals();

        this.mesh = meshes[meshes.Length - 1];

        this.centroid = new Vector3(0, 0, 0);
        this.children = new List<MeshNode>();
        this.collider = collider;
    }

    public MeshNode(Mesh mesh1, Mesh mesh2) {

        // Construct a new mesh from mesh1 and mesh2
        Vector3[] vertices1 = mesh1.vertices;
        Vector3[] vertices2 = mesh2.vertices;

        // Create a set of triangles in vertices1
        HashSet<Triangle> allTriangles = new HashSet<Triangle>();

        for (int i = 0; i < vertices1.Length; i += 3) {
            allTriangles.Add(new Triangle(vertices1[i], vertices1[i + 1], vertices1[i + 2]));
        }

        // Now create an array of vertices that is the union of vertices1 and vertices2
        // Order has to be maintained!
        List<Vector3> vertices = new List<Vector3>();

        for (int i = 0; i < vertices1.Length; ++i) {
            vertices.Add(vertices1[i]);
        }

        for (int i = 0; i < vertices2.Length; i += 3) {
            Vector3 v1 = vertices2[i], v2 = vertices2[i + 1], v3 = vertices2[i + 2];
            Triangle t = new Triangle(v1, v2, v3);
            if (!allTriangles.Contains(t)) {
                vertices.Add(v1);
                vertices.Add(v2);
                vertices.Add(v3);
            }
        }

        // Amount of vertices
        int vertexCount = vertices.Count;

        // Triangle order is from back to front
        int[] triangles = new int[vertexCount];
        for (int i = 0; i < vertexCount; ++i) {
            triangles[i] = vertexCount - i - 1;
        }

        this.mesh = new Mesh();
        this.mesh.vertices = vertices.ToArray();
        this.mesh.triangles = triangles;
        this.mesh.RecalculateNormals();
        this.centroid = new Vector3(0, 0, 0);

        this.children = new List<MeshNode>();
    }

    public void AddChild(MeshNode newMesh) {
        this.children.Add(newMesh);
        this.centroid *= this.size;
        this.size += newMesh.size;
        this.centroid += newMesh.centroid;
        this.centroid /= this.size;
    }

    public Mesh GetMesh() {
        return this.mesh;
    }

    public List<MeshNode> GetChildren() {
        return this.children;
    }

    public List<MeshNode> GetItems() {
        List<MeshNode> items = new List<MeshNode>();
        items.Add(this);

        // Children
        foreach (MeshNode child in children) {
            items.AddRange(child.GetItems());
        }

        return items;
    }

    public List<Vector3> GetPositions() {
        List<Vector3> pos = new List<Vector3>();

        // Leaf
        if (this.children.Count == 0) {
            pos.Add(this.position);
        }

        else {
            // Add all the childrens positions
            foreach (MeshNode child in children) {
                pos.AddRange(child.GetPositions());
            }
        }

        return pos;
    }

    public int GetSize() {
        return this.size;
    }
    public int GetTreeSize() {
        int s = 1;
        foreach (MeshNode c in children) {
            s += c.GetTreeSize();
        }

        return s;
    }
    public Vector3 GetCentroid() {
        return this.centroid;
    }
}