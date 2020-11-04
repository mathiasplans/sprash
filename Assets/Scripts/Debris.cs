using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Debris : MonoBehaviour
{
    [SerializeField] public Position position = null;
    [SerializeField] public Environment environment = null;
    [SerializeField] public GameObject gameObjectToCreate = null;

    Vector3 movementVector;
    Vector3 movedPosition;
    Rigidbody rigidbody = null;

    [SerializeField] public int seed = 0;

    GameObject root;
    GameObject parent;
    GameObject[] children;

    public MeshNode meshTree;

    int childrenDestroyed = 0;

    int nrOfCells;
    int breaks;

    Vector3 offset;

    int gracePeriod = 0;
    [SerializeField] bool broken;
    [SerializeField] Vector3 centroid;

    private void RecursiveDisable() {
        if (children != null && children.Length != 0) {
            foreach (GameObject c in children) {
                c.GetComponent<MeshRenderer>().enabled = true;
                c.GetComponent<MeshCollider>().enabled = true;
                c.GetComponent<SphereCollider>().enabled = false;
                c.GetComponent<Debris>().RecursiveDisable();
                c.SetActive(false);
            }
        }
    }

    private void OnEnable() {
        gameObject.GetComponent<MeshRenderer>().enabled = true;
        gameObject.GetComponent<MeshCollider>().enabled = true;
        gameObject.GetComponent<SphereCollider>().enabled = false;
        gracePeriod = 10;
        broken = false;

        // Disable children
        RecursiveDisable();
    }

    void InitChildren() {
        List<MeshNode> kids = meshTree.GetChildren();

        if (kids.Count == 0)
            return;

        children = new GameObject[kids.Count];

        for (int i = 0; i < kids.Count; ++i) {
            children[i] = Instantiate(gameObjectToCreate, new Vector3(0f, 0f, 0f), Quaternion.identity, gameObject.transform);
            children[i].SetActive(false);
            Debris childDebris = children[i].GetComponent<Debris>();
            childDebris.meshTree = kids[i];
            childDebris.root = root;
            childDebris.gameObjectToCreate = gameObjectToCreate;
            childDebris.Init();
        }
    }

    private void InitRoot() {
        // Get a MeshTree
        // TODO: Outside Debris
        
        breaks = meshTree.GetTreeSize();
        root = gameObject;
    }

    private void InitChild() {
        parent = gameObject.transform.parent.gameObject;
        Debris parentDebris = parent.GetComponent<Debris>();
        environment = parentDebris.environment;
        seed = parentDebris.seed;
        position = parentDebris.position;
    }

    private void OnDisable() {
        movedPosition = new Vector3(-10000, -10000, -10000);
        gameObject.GetComponent<Rigidbody>().velocity = movementVector;
        broken = false;
    }

    public void RootDestroy() {
        childrenDestroyed++;
        if (childrenDestroyed == breaks) {
            childrenDestroyed = 0;
            environment.DestroyAsteroid(gameObject);
        }
    }

    public void Init() {
        // Root object
        if (gameObject.transform.parent == null)
            InitRoot();

        // Child object
        else
            InitChild();

        centroid = meshTree.GetCentroid();

        InitChildren();


        // Set Mesh
        gameObject.GetComponent<MeshFilter>().mesh = meshTree.GetMesh();
        gameObject.GetComponent<MeshCollider>().sharedMesh = meshTree.GetMesh();

        // Movement
        movementVector = new Vector3(environment.wind.x, environment.wind.y, 0);
        movedPosition = position.transform.position;

        // Randomize a bit
        movementVector.x += Random.Range(-environment.maxError, environment.maxError);
        movementVector.y += Random.Range(-environment.maxError, environment.maxError);

        rigidbody = gameObject.GetComponent<Rigidbody>();
        rigidbody.velocity = movementVector;

        gameObject.GetComponent<Renderer>().material.SetFloat("_Seed", Random.Range(-10000, 10000));
    }

    private void FixedUpdate() {
        rigidbody.velocity = new Vector3(rigidbody.velocity.x, rigidbody.velocity.y, 0);
    }

    // Update is called once per frame
    void Update() {
        if (movedPosition.x == -10000)
            movedPosition = position.transform.position;

        else {
            Vector3 delta = movedPosition - position.transform.position;
            rigidbody.position += delta;
            movedPosition -= delta;
        }

        rigidbody.position = new Vector3(rigidbody.position.x, rigidbody.position.y, 0);

        // Sync rigidBody and Transform
        //transform.position = rigidbody.position;
        
        if (broken) {
            rigidbody.angularVelocity = new Vector3(0f, 0f, 0f);
        }
        
        Quaternion rot = Quaternion.Euler(0, 0, rigidbody.rotation.eulerAngles.z);
        rigidbody.rotation = rot;

        if (gracePeriod != 0)
            gracePeriod -= 1;

        if (gracePeriod == 0)
            gameObject.GetComponent<SphereCollider>().enabled = false;

        if (rigidbody.position.magnitude > environment.maxDistance)
            Break();
    }

    void Break() {
        if (gracePeriod != 0)
            return;

        // Ensure that this function is called only once
        if (broken)
            return;

        broken = true;

        root.GetComponent<Debris>().RootDestroy();

        if (children != null && children.Length != 0) {
            float[] weights = new float[children.Length];

            for (int i = 0; i < children.Length; ++i) {
                GameObject kid = children[i];
                Debris chdeb = kid.GetComponent<Debris>();
                chdeb.movedPosition = movedPosition;
                chdeb.transform.position = gameObject.transform.position;
                kid.SetActive(true);

                weights[i] = chdeb.meshTree.GetSize();
            }

            float totalWeight = 0f;
            foreach (float w in weights) {
                totalWeight += w;
            }

            Vector3 center = new Vector3(0f, 0f, 0f);

            for (int i = 0; i < weights.Length; ++i) {
                weights[i] /= totalWeight;
                weights[i] = 1f - weights[i];

                center += weights[i] * children[i].GetComponent<Debris>().meshTree.GetCentroid();
            }

            center += gameObject.transform.position;
            center = new Vector3(center.x, center.y, 0);

            gameObject.GetComponent<ParticleSystem>().Play();

            gameObject.GetComponent<SphereCollider>().center = center;
            gameObject.GetComponent<SphereCollider>().enabled = true;
            gracePeriod = 4;
        }

        gameObject.GetComponent<MeshRenderer>().enabled = false;
        gameObject.GetComponent<MeshCollider>().enabled = false;
    }

    void OnCollisionEnter(Collision collision) {
        Break();
    }
}
