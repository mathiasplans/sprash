using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Objective : MonoBehaviour {
    [SerializeField] public Transform positionTransform;
    [SerializeField] public float minTravelDistance;
    [SerializeField] public float maxTravelDistance;
    [SerializeField] public float minDiscoveryDistance;
    [SerializeField] public float maxDiscoveryDistance;
    [SerializeField] public float timeout;
    [SerializeField] public float objectiveRadius;

    private Vector3 travelled;
    private Vector3 lastPosition;
    private float travelledDistance;

    uint dataPoints = 0;
    private Vector3 averageDirection;

    private float neededTravel;
    private float neededDiscovery;
    private bool discovered = false;

    private Vector3 objectivePosition;

    private void AddToAverage(Vector3 movement) {
        this.averageDirection *= this.dataPoints;
        this.averageDirection += movement;
        this.dataPoints += 1;
        this.averageDirection /= this.dataPoints;
    }

    IEnumerator Timeout(float seconds) {
        yield return new WaitForSeconds(seconds);
        this.travelledDistance += this.neededTravel * 2f;
    }

    private float NextFloat(System.Random rng, float min, float max) {
        double range = (double) max - (double) min;
        double sample = rng.NextDouble();
        double scaled = (sample * range) + min;
        return (float) scaled;
    }

    // Start is called before the first frame update
    void Start() {
        this.travelled = new Vector3(0f, 0f, 0f);
        this.lastPosition = new Vector3(0f, 0f, 0f);
        this.averageDirection = new Vector3(0f, 0f, 0f);
        this.travelledDistance = 0f;

        System.Random r = new System.Random();
        this.neededTravel = this.NextFloat(r, this.minTravelDistance, this.maxTravelDistance);
        this.neededDiscovery = this.NextFloat(r, this.minDiscoveryDistance, this.maxDiscoveryDistance);

        // Timeout coroutine
        StartCoroutine(Timeout(this.timeout));
    }

    // Update is called once per frame
    void Update() {
        Vector3 delta = this.positionTransform.position - lastPosition;
        this.lastPosition = this.positionTransform.position;

        if (delta.magnitude == 0f)
            return;

        travelled += delta;
        travelledDistance += delta.magnitude;

        this.AddToAverage(delta);

        // Going toward objective
        if (this.discovered) {
            // Move the objective
            this.objectivePosition -= delta;
        }

        // Discovery
        else if (this.neededTravel < travelledDistance) {
            // Get the position of the objective from 0, 0, 0
            this.objectivePosition = this.averageDirection.normalized * this.neededDiscovery;
            this.discovered = true;
        }
    }

    public bool IsDiscovered() {
        return this.discovered;
    }

    public Vector3 GetPosition() {
        return this.objectivePosition;
    }

    public bool IsAchievable() {
        if (!this.discovered)
            return false;

        if (this.objectivePosition != null)
            return this.objectiveRadius >= this.objectivePosition.magnitude;

        return false;
    }
}
