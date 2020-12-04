using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ObjectiveArrow : MonoBehaviour {
    [SerializeField] float distance;

    private Vector2 screenCenter;
    private float magnitude;

    private Color defaultColor;

    // Start is called before the first frame update
    void Start() {
        this.screenCenter = new Vector2(Screen.width, Screen.height) / 2f;
        transform.position = new Vector2(distance, 0f) + this.screenCenter;

        this.defaultColor = gameObject.GetComponent<Image>().color;
    }

    public void PointTo(Vector3 position) {
        Vector2 direction = new Vector2(position.x, position.y).normalized;
        transform.position = direction * Mathf.Min(this.distance, this.magnitude) + this.screenCenter;

        var angle = Mathf.Atan2(-direction.x, direction.y) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    public void Distance(Vector2 screenPoint) {
        this.magnitude = (screenPoint - this.screenCenter).magnitude - 200;
    }
}
