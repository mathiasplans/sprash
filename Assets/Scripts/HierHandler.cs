using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HierHandler : MonoBehaviour {
    public bool isLeaf = false;
    public GameObject c1;
    public GameObject c2;
    public VoroAsteroid handler;

    private HierHandler c1hh;
    private HierHandler c2hh;

    private Vector3 originalPosition;
    private Vector3 originalEuler;
    private Vector3 originalScale;

    // Start is called before the first frame update
    void Start() {

    }

    public void Initialize() {
        if (!this.isLeaf) {
            this.c1hh = c1.GetComponent<HierHandler>();
            this.c2hh = c2.GetComponent<HierHandler>();
        }

        this.originalPosition = gameObject.transform.localPosition;
        this.originalEuler = gameObject.transform.localEulerAngles;
        this.originalScale = gameObject.transform.localScale;
    }

    private void RestoreTransform() {
        gameObject.transform.localPosition = this.originalPosition;
        gameObject.transform.localEulerAngles = this.originalEuler;
        gameObject.transform.localScale = this.originalScale;
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
        // Restore the transform
        this.RestoreTransform();

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

    public void Activate(GameObject phys) {
        // The transform of phys has to match the current transform of this object
        Transform ownTransform = gameObject.transform;
        Transform physTransform = phys.transform;

        physTransform.localPosition = ownTransform.localPosition;
        physTransform.localEulerAngles = ownTransform.localEulerAngles;
        physTransform.localScale = ownTransform.localScale;

        // Get the parent
        GameObject parent = gameObject.transform.parent.gameObject;

        // We now need to inject the phys between the parent and this object
        physTransform.parent = parent.transform;
        gameObject.transform.parent = physTransform;

        // Grace period
        this.grace = 10;

        // Activate the phys
        phys.SetActive(true);

        // Activate this component
        this.enabled = true;
    }

    private (Rigidbody, Rigidbody) Split() {
        // Get the phys
        GameObject phys = gameObject.transform.parent.gameObject;

        // Get the real parent
        GameObject parent = phys.transform.parent.gameObject;

        // Do the switcharoo
        gameObject.transform.parent = parent.transform;
        phys.transform.parent = null;

        // Restore original transform
        this.RestoreTransform();

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
            // For the first child, we can reuse the phys
            this.c1hh.Activate(phys);

            // For the second child, we need a phys from the master handler
            GameObject secondPhys = this.handler.GetPhys();
            this.c2hh.Activate(secondPhys);

            return (phys.GetComponent<Rigidbody>(), secondPhys.GetComponent<Rigidbody>());
        }
    }

    private int grace = 0;

    void Update() {
        if (grace > 0)
            grace -= 1;
    }

    public void Collision(Collision collision) {
        if (grace > 0)
            return;

        // Split the asteroid
        (Rigidbody left, Rigidbody right) rigids = Split();

        if (!this.isLeaf) {
            // Get the center of the split
            Vector3 c = (c1.transform.position + c2.transform.position) / 2.0f;

            // Debug.Log(collision.collider.name);
            rigids.left.AddExplosionForce(0.1f, c, 10.0f);
            rigids.right.AddExplosionForce(0.1f, c, 10.0f);
        }
    }
}
