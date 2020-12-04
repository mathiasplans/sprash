using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movable : MonoBehaviour {
    // Start is called before the first frame update
    [SerializeField] public Position position = null;
    [SerializeField] public Environment environment = null;

    Vector3 movementVector;
    Vector3 movedPosition;

    Rigidbody rigidbody;

    bool isInited = false;

    public void Start() {
        gameObject.GetComponent<MeshCollider>().name = "PanelMesh";
    }

    public void OnEnable() {
        movedPosition = new Vector3(-10000, -10000, -10000);
        Vector3 newAng = new Vector3(Random.Range(-1, 1), Random.Range(-1, 1), Random.Range(-1, 1));
        gameObject.GetComponent<Rigidbody>().angularVelocity = newAng;
    }

    public void Init() {
        // Movement
        movementVector = new Vector3(environment.wind.x, environment.wind.y, 0);
        movedPosition = position.transform.position;

        // Randomize a bit
        movementVector.x += Random.Range(-environment.maxError, environment.maxError);
        movementVector.y += Random.Range(-environment.maxError, environment.maxError);

        rigidbody = gameObject.GetComponent<Rigidbody>();
        rigidbody.velocity = movementVector;

        gameObject.GetComponent<Renderer>().material.SetFloat("_Seed", Random.Range(-10000, 10000));

        isInited = true;
    }

    // Update is called once per frame
    void Update() {
        if (!isInited)
            return;

        if (movedPosition.x == -10000)
            movedPosition = position.transform.position;

        else {
            Vector3 delta = movedPosition - position.transform.position;
            rigidbody.position += delta;
            movedPosition -= delta;
        }

        rigidbody.position = new Vector3(rigidbody.position.x, rigidbody.position.y, 0);

        if (rigidbody.position.magnitude > environment.maxDistance)
            Break();
    }

    void OnCollisionEnter(Collision collision) {
        Break();
    }

    void Break() {
        environment.DestroyDebris(gameObject);
    }
}
