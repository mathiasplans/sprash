using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AstePhys : MonoBehaviour {
    void OnCollisionEnter(Collision collision) {
        foreach(Transform child in gameObject.transform) {
            GameObject cobj = child.gameObject;

            // Get the HierHandler
            HierHandler hh = cobj.GetComponent<HierHandler>();
            if (hh != null)
                hh.Collision(collision);
        }
    }
}
