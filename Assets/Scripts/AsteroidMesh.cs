using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class AsteroidMesh {
    Vector3[] initialVertices = {
        // (+Z)
        new Vector3( 1,  1,  1),
        new Vector3( 1, -1,  1),
        new Vector3( 0,  0,  2),

        new Vector3( 1, -1,  1),
        new Vector3(-1, -1,  1),
        new Vector3( 0,  0,  2),

        new Vector3(-1, -1,  1),
        new Vector3(-1,  1,  1),
        new Vector3( 0,  0,  2),

        new Vector3(-1,  1,  1),
        new Vector3( 1,  1,  1),
        new Vector3( 0,  0,  2)
    };

    Vector3[] vertices = new Vector3[12 * 6];
    int[] triangles = new int[12 * 6];
    Mesh mesh;

    [SerializeField] public int seed = 0;

    Vector3 offset;

    public AsteroidMesh(Vector3 position) {
        this.mesh = new Mesh();

        this.offset = position;

        CreateVertices();
        CreateMesh();
    }

    public Vector3 GetOffset() {
        return this.offset;
    }

    public Mesh GetMesh() {
        return mesh;
    }

    private void CreateVertices() {
        int facesize = initialVertices.Length;

        for (int i = 0; i < facesize; ++i) {
            Vector3 v = initialVertices[i];

            vertices[i] = v;
            vertices[i + facesize] = new Vector3(v.z, v.x, v.y);
            vertices[i + facesize * 2] = new Vector3(v.y, v.z, v.x);

            int negindex = facesize - i - 1;

            vertices[negindex + facesize * 3] = -v;
            vertices[negindex + facesize * 4] = -vertices[i + facesize];
            vertices[negindex + facesize * 5] = -vertices[i + facesize * 2];
        }

        for (int i = 0; i < triangles.Length; ++i) {
            triangles[triangles.Length - i - 1] = i;
            // triangles[i] = i;
        }

        // Transform vertices a bit
        for (int i = 0; i < facesize * 6; ++i) {
            // Make it bumpy
            Vector3 v = vertices[i];

            // Apply offset for the seed
            v = v + offset;
            System.Random r = new System.Random((int)(v.x + 10 * v.y + 100 * v.z) + seed);

            vertices[i].x += ((float)r.Next(-5, 5)) / 10.0f;
            vertices[i].y += ((float)r.Next(-5, 5)) / 10.0f;
            vertices[i].z += ((float)r.Next(-5, 5)) / 10.0f;

            // Bias towards center
            float bias = ((float)r.Next(10, 100)) / 10.0f / 8.0f;
            vertices[i] += -v.normalized * bias;

            // Apply the offset to the vertex
            vertices[i] += offset;
        }
    }

    private void CreateMesh() {
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }
}
