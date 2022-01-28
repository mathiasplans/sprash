using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class VoroAsteroid : MonoBehaviour {
    public GameObject root;
    public GameObject phys;
    public Environment environment;
    public Position position;
    public int nrOfLeaves;

    private Stack<GameObject> active;
    private Stack<GameObject> deactive;
    private HierHandler rootHandler;

    public void Initialize(Environment environment, Position position) {
        this.environment = environment;
        this.position = position;

        this.active = new Stack<GameObject>(this.nrOfLeaves);
        this.deactive = new Stack<GameObject>(this.nrOfLeaves);

        for (int i = 0; i < this.nrOfLeaves; ++i) {
            GameObject newPhys = Instantiate(this.phys);
            newPhys.SetActive(false);

            AstePhys p = newPhys.GetComponent<AstePhys>();
            p.position = position;
            p.environment = environment;
            p.handler = this;

            this.active.Push(newPhys);
        }

        this.rootHandler = root.GetComponent<HierHandler>();
    }

    private void OnDeath() {
        // Reset all the children. This also disables them
        this.rootHandler.Reset();

        // Swap the pools
        Stack<GameObject> temp = this.active;
        this.active = this.deactive;
        this.deactive = temp;

        // Disable itself
        gameObject.SetActive(false);

        // Return to the environment
        this.environment.ReturnAsteroid(this);
    }

    public void Enable(Vector3 pos, Vector3 velocity, Vector3 angvelocity) {
        gameObject.SetActive(true);

        // Enable the children as well
        this.rootHandler.Enable();

        // Activate the root
        GameObject newPhys = this.active.Pop();
        this.rootHandler.Activate(newPhys, 10f);
        newPhys.transform.localPosition = pos;
        newPhys.GetComponent<Rigidbody>().velocity = velocity;
        newPhys.GetComponent<Rigidbody>().angularVelocity = angvelocity;
    }

    public void Disable() {
        // Reset the children
        this.rootHandler.Reset();

        // Disable this
        gameObject.SetActive(false);
    }

    public GameObject GetPhys() {
        if (this.active.Count > 0)
            return this.active.Pop();

        throw new Exception("The object pool is empty!");
    }

    public void ReturnPhys(GameObject p) {
        if (this.deactive.Count == this.nrOfLeaves)
            throw new Exception("The object pool is full!");

        Rigidbody prb = p.GetComponent<Rigidbody>();
        prb.position = Vector3.zero;
        prb.angularVelocity = Vector3.zero;
        prb.velocity = Vector3.zero;

        prb.transform.localPosition = Vector3.zero;
        prb.transform.eulerAngles = Vector3.zero;

        this.deactive.Push(p);

        if (this.deactive.Count == this.nrOfLeaves)
            this.OnDeath();
    }
}
