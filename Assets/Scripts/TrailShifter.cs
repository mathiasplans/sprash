using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrailShifter : MonoBehaviour {
    public void SetSimulationSpace(AntiPosition p) {
        ParticleSystem ps = gameObject.GetComponent<ParticleSystem>();
        var main = ps.main;
        main.simulationSpace = ParticleSystemSimulationSpace.Custom;
        main.customSimulationSpace = p.transform;
    }
}
