using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Asteroid : MonoBehaviour {
    [SerializeField] public Position position = null;
    [SerializeField] public Environment environment = null;

    Vector2 movementVector;
    Vector3 movedPosition;
    Rigidbody2D rigidbody = null;

    void OnEnable() {
        movementVector = environment.wind;
        movedPosition = position.transform.position;

        // Randomize a bit
        movementVector.x += Random.Range(-environment.maxError, environment.maxError);
        movementVector.y += Random.Range(-environment.maxError, environment.maxError);

        rigidbody = gameObject.GetComponent<Rigidbody2D>();
        rigidbody.velocity = movementVector;
    }

    // Update is called once per frame
    void Update() {
        Vector3 delta = movedPosition - position.transform.position;
        transform.position += delta;
        movedPosition -= delta;

        if (transform.position.magnitude > environment.maxDistance)
            environment.DestroyAsteroid(gameObject);
    }
}
