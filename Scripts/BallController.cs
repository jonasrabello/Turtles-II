using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallController : MonoBehaviour {

	public GameObject ball;

	float direction;  // up or down

	// Use this for initialization
	void Start () {

		direction = 0.1f;
	}
	
	// Update is called once per frame
	void Update () {

		if (gameObject.transform.localPosition.y < -12.0f) {	// Ball is out out game
			Destroy (gameObject);
		} else {

			if (ball.transform.localPosition.y <= 0.1f || ball.transform.localPosition.y >= 0.4f) {
			
				direction *= -1;		// ball falling
			}

			ball.transform.Translate (new Vector3 (0, direction, 0));		// Ball movement
			transform.Translate (new Vector3 (0.05f, -0.05f, 0));				// Shadow movement

		}
	}
}
