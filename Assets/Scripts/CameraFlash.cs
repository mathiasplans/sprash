using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFlash : MonoBehaviour {
    [SerializeField] float durationInSeconds;
    private Material flashMaterial;
    private Color flashColor;

    void Start() {
        this.flashMaterial = gameObject.GetComponent<MeshRenderer>().material;
        this.flashColor = this.flashMaterial.GetColor("_Color");
    }

    IEnumerator Fade(float seconds) {
        // Set alpha to max
        this.flashColor.a = 1;
        this.flashMaterial.SetColor("_Color", this.flashColor);

        uint steps = 80;
        for (uint i = 0; i < steps; ++i) {
            this.flashColor.a = 1f - i / (float) steps;

            this.flashMaterial.SetColor("_Color", this.flashColor);
            yield return new WaitForSeconds(seconds / steps);
        }

        this.flashColor.a = 0;
        this.flashMaterial.SetColor("_Color", this.flashColor);
    }

    public void Flash() {
        StartCoroutine(Fade(this.durationInSeconds));
    }
}
