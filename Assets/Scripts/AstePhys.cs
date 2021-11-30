using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AstePhys : MonoBehaviour {
    public Position position;
    public Environment environment;
    public VoroAsteroid handler;

    private Vector3 movedPosition;

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

        GetComponent<Rigidbody>().position = new Vector3(GetComponent<Rigidbody>().position.x, GetComponent<Rigidbody>().position.y, 0);

        if (GetComponent<Rigidbody>().position.magnitude > environment.maxDistance) {
            this.DisintegrateChildren();
        }
    }

    void OnCollisionEnter(Collision collision) {
        this.SplitChildren(collision);
    }
}
