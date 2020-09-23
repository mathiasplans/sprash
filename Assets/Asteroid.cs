using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Asteroid : MonoBehaviour {
    [SerializeField] public Position position;

    // Update is called once per frame
    void Update()
    {
        transform.Position = position.coordinates;
    }
}
