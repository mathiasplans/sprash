using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsteroidCreatror : MonoBehaviour {
    public GameObject leaf;
    public GameObject branch;
    public Rigidbody rootBody;
    public int NumberOfVertices = 100;
    public float size = 5;
    public int seed = 0;
    public Material material;
    [Range(0.1f, 1.0f)] public float radius = 0.5f;
    [Range(0.1f, 1.0f)] public float xsize = 1.0f;
    [Range(0.1f, 1.0f)] public float ysize = 1.0f;
    [Range(0.1f, 1.0f)] public float zsize = 1.0f;

    private List<Mesh> meshes;
    private List<Vector3> centers;

    private class MeshTree {
        public Mesh? mesh;
        public Vector3? center;

        // Clustering
        public List<MeshTree> children;
        public HashSet<Vector3> centers;
        public Vector3 centroid;

        public MeshTree(Mesh mesh, Vector3 center) {
            this.mesh = mesh;
            this.center = center;

            this.children = new List<MeshTree>();
            this.centers = new HashSet<Vector3>();
            this.centers.Add(center);
            this.centroid = center;
        }

        public MeshTree(List<MeshTree> children) {
            this.children = children;
            this.centers = new HashSet<Vector3>();

            foreach (MeshTree child in this.children)
                this.centers.UnionWith(child.centers);

            this.centroid = new Vector3(0f, 0f, 0f);
            foreach (Vector3 c in this.centers)
                this.centroid += c;

            this.centroid /= this.centers.Count;
        }
    }

    private MeshTree Cluster(List<Mesh> meshes, List<Vector3> centers) {
        HashSet<MeshTree> clusters = new HashSet<MeshTree>();
        Queue<MeshTree> workingorder = new Queue<MeshTree>();

        // Convert to size 1 clusters
        for (int i = 0; i < meshes.Count; ++i) {
            MeshTree newCluster = new MeshTree(meshes[i], centers[i]);
            clusters.Add(newCluster);
            workingorder.Enqueue(newCluster);
        }

        // Work until only one cluster is left
        while (clusters.Count > 1) {
            // Get a cluster to work on
            MeshTree cluster = workingorder.Dequeue();

            // If the cluster is invalid
            if (!clusters.Contains(cluster))
                continue;

            // Remove the neighbour from the valid clusters
            clusters.Remove(cluster);

            // Get another cluster with the closest furthest neighbour
            float linkdist = 10000000000000.0f;
            MeshTree other = cluster;
            foreach (MeshTree neighbour in clusters) {
                // Cluster point with max distance to neighbours centroid
                float maxdist = 0.0f;
                Vector3 maxcluster = new Vector3(0f, 0f, 0f);
                foreach (Vector3 c in cluster.centers) {
                    float dist = Vector3.Distance(c, neighbour.centroid);

                    if (dist > maxdist) {
                        maxdist = dist;
                        maxcluster = c;
                    }
                }

                // Neighbour point with max distance to cluster centroid
                maxdist = 0.0f;
                Vector3 maxneighbour = new Vector3(0f, 0f, 0f);
                foreach (Vector3 c in neighbour.centers) {
                    float dist = Vector3.Distance(c, cluster.centroid);

                    if (dist > maxdist) {
                        maxdist = dist;
                        maxneighbour = c;
                    }
                }

                // Is this closer than other clusters?
                float ldist = Vector3.Distance(maxcluster, maxneighbour);
                if (ldist < linkdist) {
                    linkdist = ldist;
                    other = neighbour;
                }
            }

            if (other == cluster)
                continue;

            // Now we can create a new cluster
            List<MeshTree> pair = new List<MeshTree>();
            pair.Add(cluster);
            pair.Add(other);
            MeshTree newCluster = new MeshTree(pair);

            // Remove the other from valid clusters
            clusters.Remove(other);

            // Add the new cluster
            clusters.Add(newCluster);
            workingorder.Enqueue(newCluster);
        }

        // Return the only cluster remaining
        foreach (MeshTree t in clusters)
            return t;

        return new MeshTree(new List<MeshTree>());
    }

    private static void InitLeaf(GameObject newObj, MeshTree hierarchy) {
        MeshFilter mf = newObj.GetComponent<MeshFilter>();
        mf.mesh = hierarchy.mesh;

        MeshCollider mc = newObj.GetComponent<MeshCollider>();
        mc.enabled = false;
        mc.sharedMesh = null;
        mc.sharedMesh = hierarchy.mesh;
        mc.enabled = true;
    }

    private static void InitBranch(GameObject newObj, List<GameObject> children) {
        HierHandler hh = newObj.GetComponent<HierHandler>();
        hh.c1 = children[0];
        hh.c2 = children[1];
    }

    GameObject CreateObjects(MeshTree hierarchy, Transform parent) {
        GameObject newObj;

        // Leaf
        if (hierarchy.children.Count == 0) {
            newObj = Instantiate(leaf, parent);
            InitLeaf(newObj, hierarchy);
        }

        else {
            newObj = Instantiate(branch, parent);

            List<GameObject> children = new List<GameObject>();

            foreach (MeshTree child in hierarchy.children) {
                children.Add(CreateObjects(child, newObj.transform));
            }

            InitBranch(newObj, children);
        }

        return newObj;
    }

    GameObject CreateObjects(MeshTree hierarchy) {
        GameObject newObj;

        // Leaf
        if (hierarchy.children.Count == 0) {
            newObj = Instantiate(leaf);
            InitLeaf(newObj, hierarchy);
        }

        else {
            newObj = Instantiate(branch);

            List<GameObject> children = new List<GameObject>();

            foreach (MeshTree child in hierarchy.children) {
                children.Add(CreateObjects(child, newObj.transform));
            }

            InitBranch(newObj, children);
        }

        return newObj;
    }

    // Start is called before the first frame update
    void Start() {
        // Get the voronoi cells
        (List<Mesh> meshes, List<Vector3> centers) voronoi = VoronoiMesh.Get(
            NumberOfVertices,
            size,
            seed,
            material,
            radius,
            new Vector3(xsize, ysize, zsize)
        );

        // Cluster them
        MeshTree hierarchy = this.Cluster(voronoi.meshes, voronoi.centers);

        // Now create
        GameObject root = CreateObjects(hierarchy);

        // Rigidbody handling
        Rigidbody rb = root.AddComponent<Rigidbody>();
        rb.angularDrag = rootBody.angularDrag;
        rb.mass = rootBody.mass;
        rb.drag = rootBody.drag;
        rb.useGravity = rootBody.useGravity;
        rb.interpolation = rootBody.interpolation;
        rb.collisionDetectionMode = rootBody.collisionDetectionMode;
        rb.freezeRotation = rootBody.freezeRotation;
        rb.constraints = rootBody.constraints;
    }

    // Update is called once per frame
    void Update() {
    }
}
