using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour {
    [SerializeField] string to;
    public void ChangeScene() {
        SceneManager.LoadScene(to);
    }

    IEnumerator After(float seconds) {
        yield return new WaitForSeconds(seconds);
        ChangeScene();
    }

    public void ChangeAfter(float seconds) {
        StartCoroutine(After(seconds));
    }

    void Update() {
        if (Input.GetKey("escape")) {
            Application.Quit();
        }
    }
}
