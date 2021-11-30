using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HierHandler : MonoBehaviour {
    public bool isLeaf = false;
    public GameObject c1;
    public GameObject c2;
    public VoroAsteroid handler;
    public Position pos;
    public int size;

    private HierHandler c1hh;
    private HierHandler c2hh;

    private Vector3 originalPosition;
    private Vector3 originalEuler;
    private Vector3 originalScale;

    public void Initialize() {
        if (!this.isLeaf) {
            this.c1hh = c1.GetComponent<HierHandler>();
            this.c2hh = c2.GetComponent<HierHandler>();
        }

        this.originalPosition = gameObject.transform.localPosition;
        this.originalEuler = gameObject.transform.localEulerAngles;
        this.originalScale = gameObject.transform.localScale;

        this.pos = this.handler.position;
    }

    private void RestoreTransform() {
        gameObject.transform.localPosition = originalPosition;
        gameObject.transform.localEulerAngles = originalEuler;
        gameObject.transform.localScale = originalScale;
    }

    public void Enable() {
        gameObject.SetActive(true);
        foreach (Transform child in gameObject.transform) {
            GameObject childObj = child.gameObject;

            // If the child has HierHandler, call Enable on it
            HierHandler chh = childObj.GetComponent<HierHandler>();
            if (chh != null)
                chh.Enable();
        }
    }

    public void Reset() {
        RestoreTransform();

        // Disable recursively
        foreach (Transform child in gameObject.transform) {
            GameObject childObj = child.gameObject;

            // If the child has HierHandler, recurse
            HierHandler chh = childObj.GetComponent<HierHandler>();
            if (chh != null)
                chh.Reset();
        }

        // Disable component
        this.enabled = false;

        // Disable self
        gameObject.SetActive(false);
    }

    public void Activate(GameObject phys, float grace) {
        // Grace period
        if (grace > 0f) {
            this.grace = Time.time + grace / 1000f;
            this.invulrn = true;
        }

        // The transform of phys has to match the current transform of this object
        Transform physTransform = phys.transform;
        Rigidbody physRigid = phys.GetComponent<Rigidbody>();

        // Set the mass of the object
        physRigid.mass = (float) this.size;

        // Get the parent
        GameObject parent = gameObject.transform.parent.gameObject;

        // We now need to inject the phys between the parent and this object
        physTransform.parent = parent.transform;
        gameObject.transform.parent = physTransform;

        // Activate the phys
        phys.gameObject.GetComponent<AstePhys>().Reset();
        phys.SetActive(true);

        // Activate this component
        this.enabled = true;
    }

    private (Rigidbody, Rigidbody) Split(float grace) {
        // Get the phys
        GameObject phys = gameObject.transform.parent.gameObject;

        // Get the real parent
        GameObject parent = phys.transform.parent.gameObject;

        // Do the switcharoo
        gameObject.transform.parent = parent.transform;
        phys.transform.parent = null;

        // If we are leaf, return the phys object
        if (this.isLeaf) {
            phys.SetActive(false);
            this.handler.ReturnPhys(phys);

            // Also disable this
            gameObject.SetActive(false);

            return (null, null);
        }

        // Otherwise make children independent
        else {
            Vector3 physPos = phys.transform.localPosition;
            Vector3 physEuler = phys.transform.localEulerAngles;
            Vector3 physVel = phys.GetComponent<Rigidbody>().velocity;
            Vector3 physAngVel = phys.GetComponent<Rigidbody>().angularVelocity;

            // For the first child, we can reuse the phys
            this.c1hh.Activate(phys, grace);

            // For the second child, we need a phys from the master handler
            GameObject secondPhys = this.handler.GetPhys();
            secondPhys.transform.position = Vector3.zero;

            secondPhys.transform.localPosition = physPos;
            secondPhys.transform.localEulerAngles = physEuler;
            secondPhys.GetComponent<Rigidbody>().velocity = physVel;
            secondPhys.GetComponent<Rigidbody>().angularVelocity = physAngVel;

            this.c2hh.Activate(secondPhys, grace);

            // Make transformations
            gameObject.transform.localPosition = Vector3.zero;
            gameObject.transform.localEulerAngles = Vector3.zero;

            phys.transform.localPosition = physPos;
            phys.transform.localEulerAngles = physEuler;
            phys.GetComponent<Rigidbody>().velocity = physVel;
            phys.GetComponent<Rigidbody>().angularVelocity = physAngVel;

            secondPhys.transform.localPosition = physPos;
            secondPhys.transform.localEulerAngles = physEuler;
            secondPhys.GetComponent<Rigidbody>().velocity = physVel;
            secondPhys.GetComponent<Rigidbody>().angularVelocity = physAngVel;

            return (phys.GetComponent<Rigidbody>(), secondPhys.GetComponent<Rigidbody>());
        }
    }

    private float grace = 0f;
    private bool invulrn = true;

    void Update() {
        if (grace <= Time.time) {
            this.invulrn = false;
        }
    }

    public void Disintegrate() {
        (Rigidbody left, Rigidbody right) rigids = Split(0f);

        if (!this.isLeaf) {
            c1hh.Disintegrate();
            c2hh.Disintegrate();
        }
    }

    public void Collision(Collision collision) {
        if (this.invulrn) {
            return;
        }

        // Split the asteroid
        (Rigidbody left, Rigidbody right) rigids = Split(100f);

        if (!this.isLeaf) {
            // Get the center of the split
            Vector3 c = (c1.transform.position + c2.transform.position) / 2.0f;

            // Debug.Log(collision.collider.name);
            rigids.left.AddExplosionForce(0.3f, c, 10.0f);
            rigids.right.AddExplosionForce(0.3f, c, 10.0f);
        }
    }
}
