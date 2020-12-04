using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Environment : MonoBehaviour {
    [SerializeField] public Vector2 wind;
    [SerializeField] public float maxError;

    [SerializeField] public GameObject asteroidPrefab;
    [SerializeField] public GameObject debrisPrefab;
    [SerializeField] public GameObject parent;
    [SerializeField] public GameObject asteroidEndParticle;
    [SerializeField] public GameObject debrisEndParticle;
    [SerializeField] public float distance;
    [SerializeField] public float frequency;
    [SerializeField] public Position position;
    [SerializeField] public float maxDistance;

    [SerializeField] public int asteroidPoolSize;
    [SerializeField] public int debrisPoolSize;

    float untilNext = 0.0f;
    float windAngle;
    float fCeil;

    private Queue<GameObject> asteroidPool;
    private Queue<GameObject> debrisPool;

    // Start is called before the first frame update
    void Start() {
        asteroidPool = new Queue<GameObject>(asteroidPoolSize);
        GameObject temp;

        System.Random r = new System.Random();

        int nrOfCells = 10;
        AsteroidGrid ag = new AsteroidGrid(nrOfCells);
        MeshNode asteroidMeshTree = ag.ConstructMeshTree();

        for (int i = 0; i < asteroidPoolSize; ++i) {
            temp = Instantiate(asteroidPrefab, new Vector3(0.0f, 0.0f, 0.0f), Quaternion.identity);
            temp.SetActive(false);

            Debris a = temp.GetComponent<Debris>();
            a.environment = this;
            a.position = position;
            a.seed = r.Next(1, 10000);
            a.meshTree = asteroidMeshTree;
            a.gameObjectToCreate = asteroidPrefab;
            a.explosionParticleToCreate = asteroidEndParticle;

            a.Init();

            asteroidPool.Enqueue(temp);
        }

        debrisPool = new Queue<GameObject>(debrisPoolSize);

        for (int i = 0; i < asteroidPoolSize; ++i) {
            temp = Instantiate(debrisPrefab, new Vector3(0.0f, 0.0f, 0.0f), Quaternion.identity);
            temp.SetActive(false);

            Movable d = temp.GetComponent<Movable>();
            d.environment = this;
            d.position = position;

            d.Init();

            temp.transform.rotation = debrisPrefab.transform.rotation;

            debrisPool.Enqueue(temp);
        }
    }

    private Vector3 SpawnPoint() {
        float trim = Mathf.PI / 8.0f;
        float randomPi = Random.Range(trim, Mathf.PI - trim);
        Vector2 onCircle = new Vector2(Mathf.Cos(randomPi), Mathf.Sin(randomPi)) * distance;
        Vector2 windMovement = (wind - 6 * position.dir).normalized;
        windAngle = Mathf.Atan2(windMovement.x, windMovement.y);
        float needToRotate = -windAngle;
        Vector3 rotatedCircle = new Vector2(onCircle.x * Mathf.Cos(needToRotate) - onCircle.y * Mathf.Sin(needToRotate),
                                            onCircle.x * Mathf.Sin(needToRotate) + onCircle.y * Mathf.Cos(needToRotate));
        return -rotatedCircle;
    }

    public void CreateAsteroid() {
        if (asteroidPool.Count == 0)
            return;

        // Get the spawn point
        Vector3 newPos = SpawnPoint();

        // Get an object from the object pool. Set the position
        GameObject n = asteroidPool.Dequeue();
        n.transform.position = newPos;

        // Asteroid script initialization
        Debris a = n.GetComponent<Debris>();
        a.position = position;

        // Activate the object
        n.SetActive(true);
    }

    public void CreteDebris() {
        if (debrisPool.Count == 0)
            return;

        Vector3 newPos = SpawnPoint();

        // Get an object from the object pool. Set the position
        GameObject n = debrisPool.Dequeue();
        n.transform.position = newPos;
        n.transform.eulerAngles = new Vector3(0, 90, 0);
        n.GetComponent<Rigidbody>().velocity = new Vector3(wind.x, wind.y, 0);

        // Activate the object
        n.SetActive(true);
    }

    public void DestroyAsteroid(GameObject asteroid) {
        asteroid.SetActive(false);
        asteroidPool.Enqueue(asteroid);
    }

    public void DestroyDebris(GameObject debris) {
        debris.SetActive(false);
        debrisPool.Enqueue(debris);
    }

    // Update is called once per frame
    void Update() {
        untilNext += Time.deltaTime;
        fCeil = 1.0f / frequency;
        while (untilNext > fCeil) {
            CreateAsteroid();
            CreteDebris();
            untilNext -= fCeil;
        }
    }
}
