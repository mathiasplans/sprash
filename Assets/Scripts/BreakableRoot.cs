using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class BreakableRoot : MonoBehaviour {
    [SerializeField] public Environment environment;
    private Breakable root;
    private Rigidbody rb;
    private Collider col;
    void Start() {
        // Get the required modules
        this.rb = gameObject.GetComponent<Rigidbody>();
        this.col = gameObject.GetComponent<Collider>();

        // Get the Breakable module
        this.root = gameObject.GetComponent<Breakable>();

        // When the root gets destroyed
        Action<Breakable> onDestroy = (Breakable br) => {
            // TODO: generic function that can destroy all kinds of breakables
            environment.DestroyAsteroid(this.root.gameObject);

            // Reset the tree
            this.root.Reset();
        };

        // When the objects in the tree get broken
        Action<Breakable, List<Breakable>> onBreak = (Breakable parent, List<Breakable> children) => {
            // TODO
            // Explosion force
        };

        // When the node gets activated
        Action<Breakable> onActivate = (Breakable br) => {
            // TODO
            // Activate the physics
        };

        // When the node gets deactivated
        Action<Breakable> onDeactivate = (Breakable br) => {
            // TODO
            // Deactivate the physics
        };

        // Establish the tree
        this.root.Adopt(onDestroy, onBreak, onActivate, onDeactivate, null);
    }
}