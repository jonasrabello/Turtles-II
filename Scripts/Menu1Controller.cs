using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu1Controller : MonoBehaviour {

	public GameObject presentantion;
	public GameObject selection;
	public GameObject turtle_1;
	public GameObject turtle_2;
	public GameObject turtleSelection_1;
	public GameObject turtleSelection_2;

	public static int numberPlayers;		// Quantity of players
	float presentantionTimer;				// Control time of presentantion
	float blinkTimer;				        // Control time for blinking
	float showTimer;				        // Control time for displaying and not displaying
	bool selectionActive;			        // Control display for selection screen
	bool enter;						        // Control keyboard Enter is typed
	bool show;						        // Control display for selection option 1 Turtle 2 Turtle

	AudioSource allSoundEffects;

	void Awake(){

		allSoundEffects = GetComponent<AudioSource>();

		selectionActive = false;				// It will start after presentantion

		turtleSelection_2.SetActive (false);    // First shows the selection for 1 Turtle

		numberPlayers = 1;						// Initialize quantity for 1 Turtle

		enter = false;				
		show = true;

		showTimer = 0.0f;
		blinkTimer = 0.0f;
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

		presentantionTimer += Time.deltaTime;		// Control time of presentantion

		if (presentantionTimer >= 4.0f) {
			presentantion.SetActive (false);		// Stops presentantion
			selection.SetActive (true);				// Starts screen for selection
			selectionActive = true;					// Release selection
		}

		if (selectionActive) {

			if (Input.GetKeyDown (KeyCode.UpArrow)) {
				turtleSelection_1.SetActive (true);
				turtleSelection_2.SetActive (false);
				numberPlayers = 1;
			} else if (Input.GetKeyDown (KeyCode.DownArrow)) {
				turtleSelection_1.SetActive (false);
				turtleSelection_2.SetActive (true);
				numberPlayers = 2;
			} else if (Input.GetKeyDown (KeyCode.Return) && enter == false) {
				enter = true;
				allSoundEffects.Play ();
			}
		}

		if (enter) {
		
			blinkTimer += Time.deltaTime;

			if (blinkTimer <= 1.0f) {			// Control time for blinking
				
				showTimer += Time.deltaTime;	// Control time for displaying and not displaying

				if (showTimer >= 0.1f) {
					show = !show;
					showTimer = 0.0f;
					if (numberPlayers == 1) {
						turtle_1.SetActive (show);
					} else {
						turtle_2.SetActive (show);
					}
				}

			} else {				
				SceneManager.LoadScene("Menu_2");
			}			
		}
	}
}
