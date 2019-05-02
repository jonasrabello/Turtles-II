using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu2Controller : MonoBehaviour {

	public GameObject chooseSprite;
	public GameObject playerSprite;
	public AudioClip itemSelected;
	public Texture2D texture;		// To get all sprites of menu 2

	GameObject leonardo;
	GameObject michelangelo;
	GameObject donatello;
	GameObject raphael;
	GameObject chooseA;
	GameObject chooseB;
	GameObject chooseC;
	GameObject chooseD;	

	Vector3 leonardoPosition;
	Vector3 michelangeloPosition;
	Vector3 donatelloPosition;
	Vector3 raphaelPosition;
	Vector3 chooseAPosition;
	Vector3 chooseBPosition;
	Vector3 chooseCPosition;
	Vector3 chooseDPosition;



	AudioSource allSoundEffects;

	float blinkTimer;				// Control time for blinking
	float showTimer;				// Control time for displaying and not displaying

	int numberPlayers;				// Quantity of players
	int option_1;
	int option_2;
	int option;

	bool enter;
	bool right;
	bool left;
	bool up;
	bool down;
	bool setInitialOpt;				// To initialize option once in 2 Turtles choice
	bool show;						// Control display for selection option 1 Turtle 2 Turtle
	bool setMenu2Turtle;			// Control display for second selection

	Sprite[] Menu2List;

	void Awake() {

		allSoundEffects = GetComponent<AudioSource>();

		numberPlayers = Menu1Controller.numberPlayers;

		Menu2List = Resources.LoadAll<Sprite>(texture.name);

		chooseAPosition = GameObject.Find ("ChooseAPosition").GetComponent<Transform>().position;
		chooseBPosition = GameObject.Find ("ChooseBPosition").GetComponent<Transform>().position;
		chooseCPosition = GameObject.Find ("ChooseCPosition").GetComponent<Transform>().position;
		chooseDPosition = GameObject.Find ("ChooseDPosition").GetComponent<Transform>().position;

		leonardoPosition = GameObject.Find ("LeonardoPosition").GetComponent<Transform> ().position;
		michelangeloPosition = GameObject.Find ("MichelangeloPosition").GetComponent<Transform> ().position;
		donatelloPosition = GameObject.Find ("DonatelloPosition").GetComponent<Transform> ().position;
		raphaelPosition = GameObject.Find ("RaphaelPosition").GetComponent<Transform> ().position;

		InitializeChoices (4); // Initialize choose for 1 player

		playerSprite.GetComponent<SpriteRenderer>().sprite = Menu2List[0];	 // Initialize with Leonardo activated
		leonardo = Instantiate (playerSprite, leonardoPosition, Quaternion.identity) as GameObject;
		playerSprite.GetComponent<SpriteRenderer>().sprite = Menu2List[7];	 // Initialize with Michelangelo deactivated
		michelangelo = Instantiate (playerSprite, michelangeloPosition, Quaternion.identity) as GameObject;
		playerSprite.GetComponent<SpriteRenderer>().sprite = Menu2List[8];	 // Initialize with Donatello deactivated
		donatello = Instantiate (playerSprite, donatelloPosition, Quaternion.identity) as GameObject;
		playerSprite.GetComponent<SpriteRenderer>().sprite = Menu2List[9];	 // Initialize with Raphael deactivated
		raphael = Instantiate (playerSprite, raphaelPosition, Quaternion.identity) as GameObject;

		SetInitialOption (chooseA);

		option_1 = 0;
		option_2 = 0;

		setInitialOpt = true;		
		setMenu2Turtle = false;
	}

	// Use this for initialization
	void Start () {

	}

	// Update is called once per frame
	void Update () {

		if (option_1 == 0) {

			option = selectPlayer ();
			if (enter) {
				option_1 = option;
			}
		} 

		if (option_1 != 0 && setMenu2Turtle == false) {

			PlayerSetBlinking (option_1);

			if (blinkTimer > 1.0f) {
				if (numberPlayers == 2) {
					setMenu2Turtle = true;
				} else {
					SceneManager.LoadScene ("Level_1");
				}
			}
		}

		if (setMenu2Turtle) {
			if (setInitialOpt) {
				InitializeChoices (5);		// Set choices for 2 player
				switch (option_1) {
				case 1:
					SetInitialOption (chooseB);
					break;
				case 2:
					SetInitialOption (chooseA);
					break;
				case 3:
					SetInitialOption (chooseA);
					break;
				case 4:
					SetInitialOption (chooseA);
					break;
				}
				setInitialOpt = false;
			}
			option = selectPlayer ();
			if (enter) {
				option_2 = option;
			}				
		}		

		if (option_2 != 0) {
			PlayerSetBlinking (option_2);
			if (blinkTimer > 1.0f) {
				SceneManager.LoadScene ("Level_1");
			}
		}
	}

	int selectPlayer() {

		if (Input.GetKeyDown (KeyCode.UpArrow)) {			
			up = true;
			down = false;
		} else if (Input.GetKeyDown (KeyCode.DownArrow)) {
			up = false;
			down = true;
		} else if (Input.GetKeyDown (KeyCode.LeftArrow)) {
			right = false;
			left = true;
		} else if (Input.GetKeyDown (KeyCode.RightArrow)) {
			right = true;
			left = false;
		} else if (Input.GetKeyDown (KeyCode.Return) && enter == false) {
			enter = true;
			allSoundEffects.PlayOneShot (itemSelected, 1.0f);
		}

		if (!enter) {
			if (up && left) {
				if (option_1 != 1) {					// if option 1 has not been choose
					option = 1;
					SetActiveOption (chooseA);
					SetActivePlayer (leonardo, 0);
				}
			} else if (up && right) {					
				if (option_1 != 2) {					// if option 1 has not been choose
					option = 2;
					SetActiveOption (chooseB);
					SetActivePlayer (michelangelo, 1);
				} 
			} else if (down && left) {
				if (option_1 != 3) {					// if option 1 has not been choose
					option = 3;
					SetActiveOption (chooseC);
					SetActivePlayer (donatello, 2);
				}
			} else if (down && right) {
				if (option_1 != 4) {					// if option 1 has not been choose
					option = 4;
					SetActiveOption (chooseD);
					SetActivePlayer (raphael, 3);
				}
			}
		}

		return option;
	}

	void SetActiveOption(GameObject choose) {
		chooseA.SetActive (false);
		chooseB.SetActive (false);
		chooseC.SetActive (false);
		chooseD.SetActive (false);
		choose.SetActive (true);
	}

	void SetActivePlayer(GameObject player, int playerIndex) {		
		if (option_1 != 1) {					// if option 1 has not been choose
			leonardo.GetComponent<SpriteRenderer> ().sprite = Menu2List [6];			//Deactivated
		}
		if (option_1 != 2) {					// if option 1 has not been choose
			michelangelo.GetComponent<SpriteRenderer> ().sprite = Menu2List [7]; 		//Deactivated
		}
		if (option_1 != 3) {					// if option 1 has not been choose
			donatello.GetComponent<SpriteRenderer> ().sprite = Menu2List [8];			//Deactivated
		}
		if (option_1 != 4) {					// if option 1 has not been choose
			raphael.GetComponent<SpriteRenderer> ().sprite = Menu2List [9];				//Deactivated		
		}
		player.GetComponent<SpriteRenderer> ().sprite = Menu2List [playerIndex];	//Activated
	}

	void SetInitialOption(GameObject choose) {
		chooseA.SetActive (false);
		chooseB.SetActive (false);
		chooseC.SetActive (false);
		chooseD.SetActive (false);
		choose.SetActive (true);
		up = true;
		right = false;
		down = false;
		left = true;
		enter = false;
		show = false;
		showTimer = 0.0f;
		blinkTimer = 0.0f;
		option = 0;
	}

	void PlayerSetBlinking (int option) {

		blinkTimer += Time.deltaTime;

		if (blinkTimer <= 1.0f) {			// Control time for blinking

			showTimer += Time.deltaTime;	// Control time for displaying and not displaying

			if (showTimer >= 0.1f) {				

				show = !show;
				showTimer = 0.0f;

				switch (option) {
				case 1:
					leonardo.SetActive (show);
					break;
				case 2:
					michelangelo.SetActive (show);
					break;
				case 3:
					donatello.SetActive (show);
					break;
				case 4:
					raphael.SetActive (show);
					break;
				}
			}
		} else {
			switch (option) {
			case 1:
				leonardo.SetActive (true);
				break;
			case 2:
				michelangelo.SetActive (true);
				break;
			case 3:
				donatello.SetActive (true);
				break;
			case 4:
				raphael.SetActive (true);
				break;
			}
		}
	}

	void InitializeChoices(int index) {
		
		chooseA = Instantiate (chooseSprite, chooseAPosition, Quaternion.identity) as GameObject;
		chooseA.GetComponent<SpriteRenderer> ().sprite = Menu2List [index];
		chooseB = Instantiate (chooseSprite, chooseBPosition, Quaternion.identity) as GameObject;
		chooseB.GetComponent<SpriteRenderer> ().sprite = Menu2List [index];
		chooseC = Instantiate (chooseSprite, chooseCPosition, Quaternion.identity) as GameObject;
		chooseC.GetComponent<SpriteRenderer> ().sprite = Menu2List [index];
		chooseD = Instantiate (chooseSprite, chooseDPosition, Quaternion.identity) as GameObject;
		chooseD.GetComponent<SpriteRenderer> ().sprite = Menu2List [index];
	}


}