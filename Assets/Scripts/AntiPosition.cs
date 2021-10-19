using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AntiPosition : MonoBehaviour {
    [SerializeField] public Position position;

    // Update is called once per frame
    void Update() {
        transform.position = -position.transform.position;
    }
}
