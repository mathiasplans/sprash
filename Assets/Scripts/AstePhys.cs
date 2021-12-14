using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AstePhys : MonoBehaviour {
    public Position position;
    public Environment environment;
    public VoroAsteroid handler;

    private Vector3 movedPosition;
    private int grace;

    // It's important to use a grace period
    // because in the Update method we destroy
    // the asteroid when we are out of bounds.
    // But when we start reusing the AstePhys-es,
    // then the Update might happen when we don't
    // have the correct coorcdinates for the rigidbody,
    // in which case one of the child asteroids just
    // gets disintegrated on birth.
    public void Grace() {
        this.grace = 20;
    }

    public void Reset() {
        this.movedPosition = this.position.transform.position;
    }

    void Start() {
        this.movedPosition = new Vector3();
    }

    private void SplitChildren(Collision collision) {
        foreach(Transform child in gameObject.transform) {
            GameObject cobj = child.gameObject;

            // Get the HierHandler
            HierHandler hh = cobj.GetComponent<HierHandler>();
            if (hh != null)
                hh.Collision(collision);
        }
    }

    private void DisintegrateChildren() {
        foreach(Transform child in gameObject.transform) {
            GameObject cobj = child.gameObject;

            // Get the HierHandler
            HierHandler hh = cobj.GetComponent<HierHandler>();
            if (hh != null)
                hh.Disintegrate();
        }
    }

    void Update() {
        Vector3 delta = movedPosition - position.transform.position;
        GetComponent<Rigidbody>().position += delta;
        movedPosition -= delta;

        if (grace > 0)
            grace -= 1;

        GetComponent<Rigidbody>().position = new Vector3(GetComponent<Rigidbody>().position.x, GetComponent<Rigidbody>().position.y, 0);

        if (GetComponent<Rigidbody>().position.magnitude > environment.maxDistance && grace == 0) {
            this.DisintegrateChildren();
        }
    }

    void OnCollisionEnter(Collision collision) {
        this.SplitChildren(collision);
    }
}
