using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Environment : MonoBehaviour {
    [SerializeField] public Vector2 wind;
    [SerializeField] public float maxError;

    [SerializeField] public GameObject generate;
    [SerializeField] public GameObject parent;
    [SerializeField] public float distance;
    [SerializeField] public float frequency;
    [SerializeField] public Position position;
    [SerializeField] public float maxDistance;

    [SerializeField] public int objectPoolSize;

    float untilNext = 0.0f;
    float windAngle;
    float fCeil;

    private Queue<GameObject> objectPool;

    // Start is called before the first frame update
    void Start() {
        objectPool = new Queue<GameObject>(objectPoolSize);
        GameObject temp;

        for (int i = 0; i < objectPoolSize; ++i) {
            temp = Instantiate(generate, new Vector3(0.0f, 0.0f, 0.0f), Quaternion.identity);
            temp.SetActive(false);
            objectPool.Enqueue(temp);
        }
    }

    private Vector3 SpawnPoint() {
        float randomPi = Random.Range(0.0f, Mathf.PI);
        Vector2 onCircle = new Vector2(Mathf.Cos(randomPi), Mathf.Sin(randomPi)) * distance;
        windAngle = Mathf.Atan2(wind.x, wind.y);
        float needToRotate = -windAngle;
        Vector3 rotatedCircle = new Vector2(onCircle.x * Mathf.Cos(needToRotate) - onCircle.y * Mathf.Sin(needToRotate),
                                            onCircle.x * Mathf.Sin(needToRotate) + onCircle.y * Mathf.Cos(needToRotate));
        return -rotatedCircle;
    }

    public void CreateAsteroid() {
        // Get the spawn point
        Vector3 newPos = SpawnPoint();

        // Get an object from the object pool. Set the position
        GameObject n = objectPool.Dequeue();
        n.transform.position = newPos;

        // Asteroid script initialization
        Asteroid a = n.GetComponent<Asteroid>();
        a.environment = this;
        a.transform.parent = parent.transform;
        a.position = position;

        // Activate the object
        n.SetActive(true);
    }

    public void DestroyAsteroid(GameObject asteroid) {
        asteroid.SetActive(false);
        objectPool.Enqueue(asteroid);
    }

    // Update is called once per frame
    void Update() {
        untilNext += Time.deltaTime;
        fCeil = 1.0f / frequency;
        while (untilNext > fCeil) {
            CreateAsteroid();
            untilNext -= fCeil;
        }
    }
}
