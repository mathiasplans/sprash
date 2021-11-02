using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HierHandler : MonoBehaviour {
    Rigidbody rootBody;

    public GameObject c1;
    public GameObject c2;

    (Vector3, Vector3) Split() {
        Destroy(rootBody);

        Rigidbody rb1 = c1.AddComponent<Rigidbody>();
        rb1.angularDrag = rootBody.angularDrag;
        rb1.mass = rootBody.mass;
        rb1.drag = rootBody.drag;
        rb1.useGravity = rootBody.useGravity;
        rb1.interpolation = rootBody.interpolation;
        rb1.collisionDetectionMode = rootBody.collisionDetectionMode;
        rb1.freezeRotation = rootBody.freezeRotation;
        rb1.constraints = rootBody.constraints;

        Rigidbody rb2 = c2.AddComponent<Rigidbody>();
        rb2.angularDrag = rootBody.angularDrag;
        rb2.mass = rootBody.mass;
        rb2.drag = rootBody.drag;
        rb2.useGravity = rootBody.useGravity;
        rb2.interpolation = rootBody.interpolation;
        rb2.collisionDetectionMode = rootBody.collisionDetectionMode;
        rb2.freezeRotation = rootBody.freezeRotation;
        rb2.constraints = rootBody.constraints;

        return (c1.transform.position, c2.transform.position);
    }

    // Start is called before the first frame update
    void Start() {
        this.rootBody = gameObject.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update() {

    }

    void OnCollisionEnter(Collision collision) {
        // Split the asteroid
        (Vector3 a, Vector3 b) centroids = Split();

        // Get the center of the split
        Vector3 c = (centroids.a + centroids.b) / 2.0f;

        // Debug.Log(collision.collider.name);
        collision.rigidbody.AddExplosionForce(1.0f, c, 10.0f);
    }
}
