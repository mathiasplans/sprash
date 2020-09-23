using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Position : MonoBehaviour {
    public Vector2 coordinates;
    public float power = 1.0f;

    // Start is called before the first frame update
    void Start() {
        coordinates = new Vector2(0.0f, 0.0f);
    }

    // Update is called once per frame
    void Update() {
        if (Input.GetKeyDown("w")) {
            this.coordinates.y += power * Time.deltaTime;
        }

        if (Input.GetKeyDown("a")) {
            this.coordinates.x -= power * Time.deltaTime;
        }

        if (Input.GetKeyDown("s")) {
            this.coordinates.y -= power * Time.deltaTime;
        }

        if (Input.GetKeyDown("d")) {
            this.coordinates.x += power * Time.deltaTime;
        }
    }
}
