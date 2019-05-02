using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreController : MonoBehaviour {

	public Text Points;
	public Text Life;

	int number;

	// Update is called once per frame
	void Update () {
		number = GameObject.Find("Leonardo").GetComponent<LeonardoController>().GetPlayerPoints();
		Points.text = number.ToString ();
		number = GameObject.Find ("Leonardo").GetComponent<LeonardoController> ().GetPlayerLifes ();
		Life.text = number.ToString ();
	}
}
