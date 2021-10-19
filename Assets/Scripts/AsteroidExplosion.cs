using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsteroidExplosion : MonoBehaviour {
    public Position position;
    private Rigidbody rb;
    private Vector3 movedPosition;
    public GameObject root;
    private bool inited = false;

    public void Init() {
        return;
    }

    void Start() {
        rb = gameObject.GetComponent<Rigidbody>();
        movedPosition = new Vector3(-10000f, -10000f, -10000f);
    }

    void Update() {
        if (!inited)
            return;

        if (movedPosition.x == -10000)
            movedPosition = position.transform.position;

        else {
            Vector3 delta = movedPosition - position.transform.position;
            rb.position += delta;
            movedPosition -= delta;
        }

        rb.position = new Vector3(rb.position.x, rb.position.y, 0);
    }

    private void OnDisable() {
        movedPosition = new Vector3(-10000, -10000, -10000);
    }

    public void Play() {
        // Make so that the circle emission is orthogonal to camera direction
        transform.eulerAngles = new Vector3(0f, 0f, 0f);

        // Start the particle system
        gameObject.GetComponent<ParticleSystem>().Play();
    }

    public void OnParticleSystemStopped() {
        // Whan particle system ends, destroy the root
        root.GetComponent<Debris>().RootDestroy();
        gameObject.SetActive(false);
    }
}
