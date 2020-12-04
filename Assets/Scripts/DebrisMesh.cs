using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class DebrisMesh : MonoBehaviour {
    [Serializable]
    public class DebrisNode {
        [SerializeField] public GameObject meshObject;
        [SerializeField] public DebrisNode[] children;
    }

    [SerializeField] private DebrisNode node;
    public MeshNode root;

    MeshNode TreeConstruct(DebrisNode node) {
        List<MeshNode> kids = new List<MeshNode>();

        // Handle children
        if (node.children.Length != 0) {
            foreach (DebrisNode dn in node.children) {
                kids.Add(TreeConstruct(dn));
            }

            Mesh[] meshes = new Mesh[kids.Count + 1];
            for (int i = 0; i < kids.Count; ++i) {
                meshes[i] = kids[i].GetMesh();
            }

            meshes[kids.Count] = node.meshObject.GetComponent<MeshFilter>().mesh;

            MeshNode intermediary = new MeshNode(meshes, node.meshObject.GetComponent<MeshCollider>());

            foreach (MeshNode kid in kids) {
                intermediary.AddChild(kid);
            }

            return intermediary;
        }

        // Leaf case
        else {
            MeshNode leaf = new MeshNode(
                node.meshObject.GetComponent<MeshFilter>().mesh,
                node.meshObject.transform.position,
                node.meshObject.transform.position,
                node.meshObject.GetComponent<MeshCollider>()
            );

            return leaf;
        }
    }

    public MeshNode GetMeshTree() {
        return TreeConstruct(node);
    }
}
