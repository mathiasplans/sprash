using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Environment : MonoBehaviour {
    [SerializeField] public Vector2 wind;

    [SerializeField] public AsteroidCreatror acreator;
    [SerializeField] public float distance;
    [SerializeField] public float frequency;
    [SerializeField] public Position position;
    [SerializeField] public float maxDistance;


    [SerializeField] public int asteroidPoolSize;
    [SerializeField] public int debrisPoolSize;

    float untilNext = 0.0f;
    float windAngle;
    float fCeil;

    private Queue<VoroAsteroid> asteroidPool;

    // Start is called before the first frame update
    void Start() {
        this.asteroidPool = new Queue<VoroAsteroid>(asteroidPoolSize);

        this.acreator.Initialize();

        for (int i = 0; i < asteroidPoolSize; ++i) {
            this.asteroidPool.Enqueue(this.acreator.Create(this, this.position));
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

    public void SpawnAsteroid() {
        if (asteroidPool.Count == 0)
            return;

        // Get the spawn point
        Vector3 newPos = SpawnPoint();

        // Get an object from the object pool. Set the position
        VoroAsteroid va = asteroidPool.Dequeue();

        // Activate the object
        Vector3 velocity = new Vector3();
        velocity.x = this.wind.x;
        velocity.y = this.wind.y;
        velocity.z = 0.0f;
        va.Enable(newPos, velocity);
    }

    public void ReturnAsteroid(VoroAsteroid asteroid) {
        // Make sure that the object is disabled
        asteroid.gameObject.SetActive(false);

        asteroidPool.Enqueue(asteroid);
    }

    // Update is called once per frame
    void Update() {
        untilNext += Time.deltaTime;
        fCeil = 1.0f / frequency;
        while (untilNext > fCeil) {
            SpawnAsteroid();
            untilNext -= fCeil;
        }
    }
}
