using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class DebrisMesh : MonoBehaviour {
    [Serializable]
    public class DebrisNode {
        [SerializeField] public GameObject meshObject;
        [SerializeField] public DebrisNode[] children;
        //[SerializeField] public DebrisNode child1;
        //[SerializeField] public DebrisNode child2;
    }

    [SerializeField] private DebrisNode node;

    // Start is called before the first frame update
    void Start() {
        
    }

    // Update is called once per frame
    void Update() {
        
    }
}
