using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsteroidGrid {

    readonly Vector3[] basisvectors = {
        new Vector3( 2,  2,  0),
        new Vector3( 0,  2,  2),
        new Vector3( 2,  0,  2),
        new Vector3(-2,  2,  0),
        new Vector3( 0, -2,  2),
        new Vector3( 2,  0, -2),
        -new Vector3( 2,  2,  0),
        -new Vector3( 0,  2,  2),
        -new Vector3( 2,  0,  2),
        -new Vector3(-2,  2,  0),
        -new Vector3( 0, -2,  2),
        -new Vector3( 2,  0, -2)
    };

    Dictionary<Vector3, AsteroidMesh> slots = new Dictionary<Vector3, AsteroidMesh>();
    Queue<Vector3> nextPos = new Queue<Vector3>();

    int nrOfPieces;
    public AsteroidGrid(int n) {
        // Add the first position to the queue
        this.nextPos.Enqueue(new Vector3(0f, 0f, 0f));

        this.nrOfPieces = n;
    }

    public MeshNode ConstructMeshTree() {
        // Create the grid
        CreateGrid();

        // Do the hierarchical clustering on the grid
        Dictionary<Vector3, MeshNode> slotClusterAssign = new Dictionary<Vector3, MeshNode>();
        int maxClusterSize = 1;

        // Assign the slotClusterAssign
        foreach (Vector3 key in slots.Keys) {
            slotClusterAssign.Add(key, new MeshNode(slots[key].GetMesh(), key, slots[key].GetOffset()));
        }

        // Processing queue
        Queue<List<Vector3>> clusters = new Queue<List<Vector3>>();

        // Add initial clusters to the queue
        System.Random rand = new System.Random();
        List<Vector3> randomOrder = new List<Vector3>(slotClusterAssign.Keys);
        randomOrder.Sort((x, y) => rand.Next(100000));
        foreach (Vector3 key in randomOrder) {
            List<Vector3> cluster = new List<Vector3>();
            cluster.Add(key);
            clusters.Enqueue(cluster);
        }

        while (maxClusterSize != slots.Count && clusters.Count != 0) {
            // Get the processing position
            foreach (Vector3 procPos in clusters.Dequeue()) {
                // Get the node behind the key
                MeshNode procNode = slotClusterAssign[procPos];

                // Get all the positions inside the cluster (this nodes cluster)
                HashSet<Vector3> selfClusterPositions = new HashSet<Vector3>(procNode.GetPositions());

                // Find the neighboring cluster with the lowest size
                int minClusterSize = 1000;
                Vector3 minClusterKey = new Vector3(10000f, 10000f, 10000f);
                foreach (Vector3 n in basisvectors) {
                    // Get a neighbor key
                    Vector3 candidate = procPos + n; ;

                    // Check if the candidate exists
                    if (!slots.ContainsKey(candidate))
                        continue;

                    // Check if the candidate is in the same cluster as current node
                    if (selfClusterPositions.Contains(candidate))
                        continue;

                    // Check the size of the cluster
                    MeshNode neigh = slotClusterAssign[candidate];
                    if (neigh.GetSize() < minClusterSize) {
                        minClusterSize = neigh.GetSize();
                        minClusterKey = candidate;
                    }
                }

                // No eligable neighbours
                if (minClusterSize == 1000)
                    continue;

                // Now we know the cluster we want to pair with
                // Get the node of the cluster
                MeshNode clusterNode = slotClusterAssign[minClusterKey];

                // Create a new cluster where the children are the current cluster and the neighbor cluster
                MeshNode newNode = new MeshNode(clusterNode.GetMesh(), procNode.GetMesh());
                newNode.AddChild(clusterNode);
                newNode.AddChild(procNode);

                // Get all the positions in the new cluster
                List<Vector3> clusterPositions = newNode.GetPositions();

                // Update the cluster assignment
                foreach (Vector3 position in clusterPositions) {
                    slotClusterAssign[position] = newNode;
                }

                // Add the cluster positions to the processing queue
                clusters.Enqueue(clusterPositions);


                // Get the cluster size
                if (newNode.GetSize() > maxClusterSize) {
                    maxClusterSize = newNode.GetSize();

                    // Early exit if task is already finished
                    if (maxClusterSize == slots.Count)
                        break;
                }
            }
        }

        // Return the topmost cluster
        return slotClusterAssign[new Vector3(0f, 0f, 0f)];
    }

    private void CreateGrid() {
        // Grid already exists
        if (slots.Count > 0)
            return;

        int pieces = nrOfPieces;
        while (pieces > 0) {
            // Take a position from the queue
            Vector3 position = nextPos.Dequeue();

            // Skip if position is already filled
            if (slots.ContainsKey(position)) {
                continue;
            }

            // Add the AsteroidMesh to the position
            slots.Add(position, new AsteroidMesh(position));

            // Add the neighbours to the queue
            foreach (Vector3 n in basisvectors) {
                nextPos.Enqueue(position + n);
            }

            // Decrement number of pieces
            pieces -= 1;
        }

        // Empty the queue
        nextPos.Clear();
    }
}