using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Asteroid : MonoBehaviour {
    [SerializeField] public Position position = null;
    [SerializeField] public Environment environment = null;

    Vector3 movementVector;
    Vector3 movedPosition;
    Rigidbody rigidbody = null;

    [SerializeField] public int seed = 0;

    GameObject root;
    GameObject parent;
    GameObject child1;
    GameObject child2;

    public MeshNode meshTree;

    int childrenDestroyed = 0;

    int nrOfCells;
    int breaks;

    Vector3 offset;

    int gracePeriod = 0;
    bool broken;

    private void RecursiveDisable() {
        if (child1 != null && child2 != null) {
            child1.GetComponent<Asteroid>().RecursiveDisable();
            child2.GetComponent<Asteroid>().RecursiveDisable();
            child1.SetActive(false);
            child2.SetActive(false);
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

        // Child 1
        child1 = Instantiate(gameObject, new Vector3(0.0f, 0.0f, 0.0f), Quaternion.identity, gameObject.transform);
        child1.SetActive(false);
        Asteroid childAsteroid1 = child1.GetComponent<Asteroid>();
        childAsteroid1.meshTree = kids[0];
        childAsteroid1.root = root;
        childAsteroid1.Init();

        // Child 2
        child2 = Instantiate(gameObject, new Vector3(0.0f, 0.0f, 0.0f), Quaternion.identity, gameObject.transform);
        child2.SetActive(false);
        Asteroid childAsteroid2 = child2.GetComponent<Asteroid>();
        childAsteroid2.meshTree = kids[1];
        childAsteroid2.root = root;
        childAsteroid2.Init();
    }

    private void InitRoot() {
        // Get a MeshTree
        nrOfCells = 6;
        AsteroidGrid ag = new AsteroidGrid(nrOfCells);
        meshTree = ag.ConstructMeshTree();
        breaks = meshTree.GetTreeSize();
        root = gameObject;
    }

    private void InitChild() {
        parent = gameObject.transform.parent.gameObject;
        Asteroid parentAsteroid = parent.GetComponent<Asteroid>();
        environment = parentAsteroid.environment;
        seed = parentAsteroid.seed;
        position = parentAsteroid.position;
    }

    private void OnDisable() {
        Vector3 defaultVec = new Vector3(0, 0, 0);
        movedPosition = new Vector3(-10000, -10000, -10000);
        gameObject.transform.position = defaultVec;
        gameObject.GetComponent<Rigidbody>().position = defaultVec;
        gameObject.GetComponent<Rigidbody>().velocity = movementVector;
        broken = false;
    }

    public void RootDestroy() {
        childrenDestroyed++;
        if (childrenDestroyed == breaks) {
            childrenDestroyed = 0;
            environment.DestroyAsteroid(gameObject);
            Debug.Log("Asteroid is completly destroyed.");
        }
    }

    public void Init() {
        // Root object
        if (gameObject.transform.parent == null)
            InitRoot();

        // Child object
        else
            InitChild();

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
        if (broken) {
            rigidbody.position = new Vector3(0f, 0f, 0f);
        }

        else if (movedPosition.x == -10000)
            movedPosition = position.transform.position;

        else {
            Vector3 delta = movedPosition - position.transform.position;
            rigidbody.position += delta;
            movedPosition -= delta;
        }

        rigidbody.position = new Vector3(rigidbody.position.x, rigidbody.position.y, 0);

        // Sync rigidBody and Transform
        transform.position = rigidbody.position;

        Quaternion rot = Quaternion.Euler(0, 0, rigidbody.rotation.eulerAngles.z);
        rigidbody.rotation = rot;

        if (gracePeriod != 0)
            gracePeriod -= 1;

        if (gracePeriod == 0)
            gameObject.GetComponent<SphereCollider>().enabled = false;

        if (rigidbody.position.magnitude > environment.maxDistance) {
            Debug.Log("Break due max distance. " + rigidbody.position.magnitude);
            Break();
        }
    }

    void Break() {
        if (gracePeriod != 0)
            return;

        // Ensure that this function is called only once
        if (broken) {
            Debug.Log("Is already broken!");
            return;
        }

        broken = true;

        root.GetComponent<Asteroid>().RootDestroy();

        if (child1 != null && child2 != null) {
            Asteroid ch1ast = child1.GetComponent<Asteroid>();
            ch1ast.movedPosition = movedPosition;
            child1.transform.position = gameObject.transform.position + ch1ast.meshTree.GetCentroid();
            child1.transform.rotation = gameObject.transform.rotation;
            child1.SetActive(true);

            Asteroid ch2ast = child2.GetComponent<Asteroid>();
            ch2ast.movedPosition = movedPosition;
            child2.transform.position = gameObject.transform.position;
            child2.transform.rotation = gameObject.transform.rotation;
            child2.SetActive(true);

            // Create a collider between the fragments
            // Get the weights of the children
            float ch1w = ch1ast.meshTree.GetSize();
            float ch2w = ch2ast.meshTree.GetSize();
            float totalw = ch1w + ch2w;
            ch1w /= totalw;
            ch2w /= totalw;

            // Note that we use ch2w for ch1ast, because heavier objects should move less
            Vector3 center = ch2w * ch1ast.meshTree.GetCentroid() + ch1w * ch2ast.meshTree.GetCentroid();
            center = new Vector3(center.x, center.y, 0);
            
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
