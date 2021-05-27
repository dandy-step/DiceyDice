using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkyCyl : MonoBehaviour {
    public float mAmountToRotate = 0.1f;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        transform.Rotate(Vector3.up, mAmountToRotate);
	}
}
