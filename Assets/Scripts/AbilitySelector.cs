using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AbilitySelector : MonoBehaviour {
    [SerializeField] public float colorReductionFactor = 5f;
    private bool selected = false;
    private Color initialColor;

    public void Start() {
        this.initialColor = gameObject.GetComponent<Image>().color;

        // By default, deselected
        gameObject.GetComponent<Image>().color = this.initialColor / colorReductionFactor;
    }

    IEnumerator Fade(Color from, Color to) {
        uint steps = 80;
        for (int i = 0; i < steps; ++i) {
            gameObject.GetComponent<Image>().color = Color.Lerp(from, to, i / (float) steps);
            yield return new WaitForSeconds(0.0025f);
        }

        // Make sure that the color is set to 'to'
        gameObject.GetComponent<Image>().color = to;

        yield return null;
    }

    public void Select() {
        if (this.selected)
            return;

        this.selected = true;
        StartCoroutine(Fade(this.initialColor / colorReductionFactor, this.initialColor));
    }

    public void Deselect() {
        if (this.selected == false)
            return;

        this.selected = false;
        StartCoroutine(Fade(this.initialColor, this.initialColor / colorReductionFactor));
    }
}
