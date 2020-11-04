using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Position : MonoBehaviour {
    public float power = 1.0f;
    public Vector2 dir;

    bool[] isPressed = new bool[4];

   // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {
        if (Input.GetKeyDown("w"))
            isPressed[0] = true;

        else if (Input.GetKeyUp("w"))
            isPressed[0] = false;

        if (Input.GetKeyDown("a"))
            isPressed[1] = true;

        else if (Input.GetKeyUp("a"))
            isPressed[1] = false;

        if (Input.GetKeyDown("s"))
            isPressed[2] = true;

        else if (Input.GetKeyUp("s"))
            isPressed[2] = false;

        if (Input.GetKeyDown("d"))
            isPressed[3] = true;

        else if (Input.GetKeyUp("d"))
            isPressed[3] = false;

        Vector3 delta = new Vector2();

        if (isPressed[0])
            delta.y += 1;

        if (isPressed[1])
            delta.x -= 1;

        if (isPressed[2])
            delta.y -= 1;

        if (isPressed[3])
            delta.x += 1;

        dir = delta.normalized;

        if (delta.magnitude != 0)
            delta = dir * power * Time.deltaTime;

        transform.position += delta;
    }
}
