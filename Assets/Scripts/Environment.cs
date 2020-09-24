using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Environment : MonoBehaviour {
    [SerializeField] public Vector2 wind;
    [SerializeField] public float maxError;

    [SerializeField] public GameObject generate;
    [SerializeField] public GameObject parent;
    [SerializeField] public float distance;
    [SerializeField] public float frequency;
    [SerializeField] public Position position;
    [SerializeField] public float maxDistance;

    float untilNext = 0.0f;
    float windAngle;
    float fCeil;

    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {
        untilNext += Time.deltaTime;
        fCeil = 1.0f / frequency;
        while (untilNext > fCeil) {
            float randomPi = Random.Range(0.0f, Mathf.PI);
            Vector2 onCircle = new Vector2(Mathf.Cos(randomPi), Mathf.Sin(randomPi)) * distance;
            windAngle = Mathf.Atan2(wind.x, wind.y);
            float needToRotate = -windAngle;
            Vector3 rotatedCircle = new Vector2(onCircle.x * Mathf.Cos(needToRotate) - onCircle.y * Mathf.Sin(needToRotate),
                                                onCircle.x * Mathf.Sin(needToRotate) + onCircle.y * Mathf.Cos(needToRotate));
            Vector3 newPosition = -rotatedCircle;
            GameObject n = Instantiate(generate, newPosition, Quaternion.identity);
            Asteroid a = n.GetComponent<Asteroid>();
            a.environment = this;
            a.transform.parent = parent.transform;
            a.position = position;

            untilNext -= fCeil;
        }
    }
}
