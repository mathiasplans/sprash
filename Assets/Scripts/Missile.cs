using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Missile : MonoBehaviour {
    [SerializeField] public float speed;
    [SerializeField] public float turningSpeed = 0.1f;
    [SerializeField] public GameObject trailer;
    [SerializeField] public float timeout;
    [SerializeField] public float explosionForce;
    public Action<GameObject> onDestroy;
    public float maxDistance;

    public Position position;

    private Vector3 target;
    private Vector3 movement;
    private bool seeking = false;

    private Vector3 movedPosition;

    public void Start() {
        gameObject.GetComponent<CapsuleCollider>().name = "MissileCollider";
    }

    public void Seek(Vector3 tar) {
        this.target = tar;
    }

    public void Activate() {
        this.seeking = true;

        // Timeout
        StartCoroutine(Timeout(this.timeout));
    }

    public Vector3 GetPosition() {
        return transform.position;
    }

    IEnumerator DestroyAfterTime(float particleLifetime) {
        // Wait until particle effects have expired
        yield return new WaitForSeconds(particleLifetime);

        // Reactivate collider and renderer
        gameObject.GetComponent<CapsuleCollider>().enabled = true;
        gameObject.GetComponent<MeshRenderer>().enabled = true;

        // Deactivate
        this.onDestroy(gameObject);
    }

    IEnumerator Timeout(float seconds) {
        yield return new WaitForSeconds(seconds);

        this.Destruct();
    }

    public void Destruct() {
        if (this.seeking == false)
            return;

        // Stop timeout
        StopCoroutine("Timeout");

        // Stand still
        gameObject.GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);

        // Stop seeking
        this.seeking = false;

        // Activate explosion
        gameObject.GetComponent<SphereCollider>().enabled = true;
        gameObject.GetComponent<ParticleSystem>().Play();

        // Disable rocket
        gameObject.GetComponent<CapsuleCollider>().enabled = false;
        gameObject.GetComponent<MeshRenderer>().enabled = false;

        // Stop the trail
        this.trailer.GetComponent<ParticleSystem>().Stop();

        // Wait until trail has expired
        StartCoroutine(DestroyAfterTime(5f));
    }

    public void OnParticleSystemStopped() {
        // Explosion has ended
        gameObject.GetComponent<SphereCollider>().enabled = false;
    }

    void OnDisable() {
        this.movedPosition = new Vector3(-10000, -10000, -10000);
    }

    void OnEnable() {
        // Start the trail
        this.trailer.GetComponent<ParticleSystem>().Play();

        // Get the direction to the target
        Vector3 targetDir = this.target - transform.position;
        targetDir.z = 0;

        // Correct rotation
        gameObject.GetComponent<Rigidbody>().rotation = Quaternion.LookRotation(Vector3.RotateTowards(
            transform.forward,
            new Vector3(targetDir.x, targetDir.y, 0f).normalized,
            7f,
            0.0F
        ));
    }

    void Update() {
        // Fix the position
        if (movedPosition.x == -10000)
            movedPosition = position.transform.position;

        else {
            Vector3 delta = movedPosition - position.transform.position;
            gameObject.GetComponent<Rigidbody>().position += delta;
            // gameObject.GetComponent<TrailRenderer>().AddPosition(delta);
            movedPosition -= delta;
        }

        if (this.seeking) {
            // Get the current position
            Vector3 pos = gameObject.GetComponent<Rigidbody>().position;

            // Get the direction to the target
            Vector3 targetDir = this.target - pos;
            targetDir.z = 0;
            targetDir.Normalize();

            // Add to the current movemen vector
            this.movement = Vector3.Lerp(this.movement, targetDir, this.turningSpeed).normalized;

            // Move the missile
            gameObject.GetComponent<Rigidbody>().velocity = this.speed * this.movement;

            // Get the angle of the movement vector
            gameObject.GetComponent<Rigidbody>().rotation = Quaternion.LookRotation(Vector3.RotateTowards(
                transform.forward,
                new Vector3(this.movement.x, this.movement.y, 0f),
                6.5f,
                0.0F
            ));

            // If we are too far from the center, self-destruct
            if (gameObject.GetComponent<Rigidbody>().position.magnitude > this.maxDistance)
                this.Destruct();
        }
    }

    void OnCollisionEnter(Collision collision) {
        Debug.Log(collision.collider.name);
        // Only destruct when hitting an asteroid or panel
        if (collision.collider.name == "AsteroidMesh" || collision.collider.name == "Panel") {
            collision.rigidbody.AddExplosionForce(this.explosionForce, transform.position, 1.5f * transform.localScale.x * gameObject.GetComponent<SphereCollider>().radius);
            this.Destruct();
        }

        else
            Physics.IgnoreCollision(collision.collider, gameObject.GetComponent<CapsuleCollider>());
    }
}