using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class Breakable : MonoBehaviour {
    private Vector3 defaultPosition;
    private Quaternion defaultRotation;
    private bool defaultRoot;

    // Is this game object the root object, or is it attached to something?
    private bool root = false;

    //// Childcare
    private List<Breakable> children;
    private uint nrOfDecendants;
    private uint destroyedChildren;

    //// Action callbacks
    // Called when all the decendants have deceased
    // onDestroy has to be defined by the parent/instantiator
    // onChildDestroy has a common definition
    private Action<Breakable> onDestroy;
    private Action<Breakable> onChildDestroy;
    
    // Called when the objects are breaking
    // onBreak is defined by the instantiator
    private Action<Breakable, List<Breakable>> onBreak;

    // Called when the breakable node activates/deactivates
    // onActivate and onDeactivate are defined by instantiator
    private Action<Breakable> onActivate;
    private Action<Breakable> onDeactivate;

    void Start() {
        this.defaultPosition = transform.position;
        this.defaultRotation = transform.rotation;

        //// Determine if this is a root object
        // Get the parent
        GameObject parent = transform.parent.gameObject;

        // If parent exist, get the Breakable module
        if (parent != null) {
            Breakable parentBreakable = parent.GetComponent<Breakable>();
            
            // If the module exists, set the root to false
            this.defaultRoot = parentBreakable == null;
        }

        this.root = this.defaultRoot;

        //// Actions
        // On child destroy
        this.onChildDestroy = (Breakable br) => {
            this.destroyedChildren += 1;

            // All the children have been destroyed. This means
            // that this node can be destroyed as well
            if (this.destroyedChildren == this.children.Count) {
                this.onDestroy(this);
                this.destroyedChildren = 0;
            }
        };
    }

    public uint Adopt(Action<Breakable> onDestroy, Action<Breakable, List<Breakable>> onBreak,
                      Action<Breakable> onActivate, Action<Breakable> onDeactivate,
                      Action<Breakable> onAdopt) {
        this.nrOfDecendants = 0;

        //// Set the actions
        this.onDestroy = onDestroy;
        this.onBreak = onBreak;
        this.onActivate = onActivate;
        this.onDeactivate = onDeactivate;
        
        // Call on adopt
        if (onAdopt != null)
            onAdopt(this);

        //// Adopt the children
        // Get the children
        foreach (Transform childTransform in transform) {
            // Increment the number of decendants
            this.nrOfDecendants += 1;

            // If the child exists, get the Breakable module
            Breakable childBreakable = childTransform.gameObject.GetComponent<Breakable>();
            
            // If the module exists, adopt it with Adopt function
            if (childBreakable != null) {
                this.children.Add(childBreakable);
                this.nrOfDecendants += childBreakable.Adopt(
                    this.onChildDestroy,
                    this.onBreak,
                    this.onActivate,
                    this.onDeactivate,
                    onAdopt
                );
            }
        }

        return this.nrOfDecendants;
    }

    public void Reset() {
        transform.position = this.defaultPosition;
        transform.rotation = this.defaultRotation;

        this.root = this.defaultRoot;
        this.destroyedChildren = 0;

        foreach (Breakable child in this.children) {
            child.Reset();
        }
    }

    public void Activate() {
        // Call activate callback
        if (this.onActivate != null)
            this.onActivate(this);
    }

    public void Deactivate() {
        // Call deactivate callback
        if (this.onDeactivate != null)
            this.onDeactivate(this);
    }

    /**
     * Breaks the gameobject away from the parent.
     * @params breaks - how many breaks does this object have to do
     */
    public void Break(uint breaks) {
        foreach (Breakable br in this.children) {
            // Set the root variable to true
            br.root = true;
            br.Activate();
        }
        
        // Set self root to false
        this.root = false;
        this.Deactivate();

        // Call onBreak callback
        if (this.onBreak != null)
            this.onBreak(this, this.children);
    }
}