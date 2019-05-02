using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LeonardoController : MonoBehaviour {

	public float Speed;
	public float JumpForce;
	public float AttackForce_1;
	public float HitBallForce;
	public AudioClip playerPunch;
	public AudioClip enemyPunch;
	public GameObject Enemy;
	public GameObject EnemyTrigger;
	public GameObject StairBall;
	public List<Transform> EnemyActions;
	public Texture2D texture;				// To get all sprites of lifebar

	GameObject MaxHeight;					// Border o height
	GameObject Door;						// Door of the level 1
	GameObject LifeBar; 					// To control the size of the life bar

	AudioSource allSoundEffects;

	Animator Anim;
	Animator DoorAnim;

	int lastAction;							// To get the last action of the Animation
	int indexLifeBar;						// To control the LifeBarList array
	int playerLifes;						// Number of lifes
	int playerPoints;						// Number of Points
	int enemiesIndex;						// To get the number of enemies to spawn
	int enemiesSpawn;						// To control the enemy that already was spawned
	int numberEnemies;						// Number of enemies that will spawn of trigger collider

	float positTime;						// To contol time to start position verification of the player
	float actionTime;						// To control the duration of the first action when there are two
	float act2ndTime;						// To control the duration of the second action when there are two
	float hitByEnemyTime;					// To control the time when the player gets hit by the enemy
	float spawnEnemyTime;					// To control the inteval to spawn another enemy.

	float horizontalAxis;
	float verticalAxis;
	float safetyPositon;					// To set the player in a safety position out of the fire
	float positionCalibration;				// To set the precision position y, when it comes back to the floor

	bool setActionFalse;					// To set action to false
	bool faceright;
	bool blockDirection;
	bool burned;
	bool jump;
	bool ball;
	bool hitBall;
	bool blocked;
	bool attack1;
	bool enemyDoor;							// To controll the start of Enemy door A1
	bool hitByEnemy;						// Control if the player was hit by the enemy
	bool[] enemiesTriggered;				// Control if the enemies was already triggered

	string enemyDoorName;
	string enemyDoorPosition;
	string nameSearch;

	Sprite[] LifeBarList;

	SpriteRenderer pSpriteRenderer;			// To change the side of the player according to left or right

	BoxCollider2D attackColliderFront;	// To set enable when the player is attacking, but not in other states
	BoxCollider2D attackColliderBack;		// To set enable when the player is attacking, but not in other states

	Rigidbody2D rb2D;

	Vector3 lastPosition;					// To store the last position of the player when it is jumping in anyway

	// Ball stairs controll - begin
	public float [] ballStairTime;			// To control the time to start second ball stair 1 and stair 2
	public bool [] ballStair;				// To controll the start of the balls stair
	public bool [] ball1Stair;				// To controll the start of ball 1 stair
	public bool [] ball2Stair;				// To controll the start of ball 1 stair
	Vector3 newPosition;					// Position of the balls in the stairs
	// Ball stairs controll - end



	void Awake() {
		
		rb2D = GetComponent<Rigidbody2D>();
		Anim = GetComponent<Animator> ();
		pSpriteRenderer = GetComponent<SpriteRenderer> ();
		attackColliderFront = GameObject.Find ("AttackFront").GetComponent<BoxCollider2D> ();
		attackColliderBack = GameObject.Find ("AttackBack").GetComponent<BoxCollider2D> ();
		MaxHeight = GameObject.Find ("MaxHeigthPosition");
		enemiesTriggered = new bool[10];

		allSoundEffects = GetComponent<AudioSource>();

		ballStairTime = new float[2];
		ballStair = new bool[2];
		ball1Stair = new bool[2];
		ball2Stair = new bool[2];

		faceright = true;
		burned = false;
		jump = false;
		ball = false;
		hitBall = false;
		hitByEnemy = false;
		attack1 = false;
		ballStair[0] = false;
		ballStair[1] = false;
		enemyDoor = false;
		blocked = false;

		Speed = 7.0f;
		JumpForce = 11.0f;
		AttackForce_1 = 5.0f;
		HitBallForce = 0.0f;

		LifeBarList = Resources.LoadAll<Sprite>(texture.name);
		LifeBar = GameObject.Find ("LifeBarGame");

		safetyPositon = -4.0f;			// Set safety position in Y to get out of fire
		positionCalibration = 0.16f;   	// Set a calibration position to compare positions when player jumps
		ballStairTime[0] = 8.0f;		// Set time of start stair balls
		ballStairTime[1] = 8.0f;		// Set time of start stair balls
		indexLifeBar = 0;
		playerPoints = 0;
		playerLifes = 2;
		hitByEnemyTime = 0.0f;

		setnumberEnemiesTrigger ();		// Set number of enemies in each trigger enemy
		enemiesIndex = 0;
		enemiesSpawn = 0;
		numberEnemies = 0;
		spawnEnemyTime = 0.0f;
	}

	void Start() {
		Anim.SetInteger ("action", 0);
	}

	// Update is called once per frame
	void Update () {

		if(Input.GetKeyDown(KeyCode.L) ) {		// Load player position at last checkpoint
			LoadPosition ();
		}
			
		// Input
		horizontalAxis = Input.GetAxis("Horizontal");
		verticalAxis   = Input.GetAxis("Vertical");

		// Attacking set collider to hit the Enemy
		if (attack1) {
			if (faceright) {
				attackColliderFront.enabled = true;
			} else {
				attackColliderBack.enabled = true;
			}
		} else {
			attackColliderFront.enabled = false;
			attackColliderBack.enabled = false;
		}

		// Flip sprite
		if (horizontalAxis < 0 && faceright) {
			pSpriteRenderer.flipX = true;
			faceright = false;
		} else if (horizontalAxis > 0 && !faceright) {
			pSpriteRenderer.flipX = false;
			faceright = true;
		}

		// Actions Controll
		ActionsControll();

		// Starts the balls stair 1
		StartBalls();

		//Instantiate the enemies
		enemyInstantiate(enemiesIndex);

		// Instantiate enemy at the door
		if (enemyDoor && DoorAnim.GetCurrentAnimatorStateInfo(0).IsName("Door_Final") ) {	
			Door.SetActive (false);
			GameObject EnemyPosisition = GameObject.Find (enemyDoorPosition);
			GameObject temp_Enemy = Instantiate (Enemy, EnemyPosisition.transform.position, Quaternion.identity) as GameObject;
			temp_Enemy.name = enemyDoorName;
			temp_Enemy.SendMessage ("SetChase");
			enemyDoor = false;
		}

		// Blocked state controll
		if (blocked && ( blockDirection != faceright) ) {
				blocked = false;
		}

		// Hit by the enemy
		if (hitByEnemy && !attack1) {
			Anim.SetInteger ("action", 9);	// Blocked or hit
		}

		// Changing action according to the moviment of the player
		if (!burned && !jump && !attack1 && !hitBall && !blocked && !hitByEnemy) {
			if (horizontalAxis != 0.0f || verticalAxis != 0.0f) {
				Anim.SetInteger ("action", 1);		// Walking
			} else {
				Anim.SetInteger ("action", 0);     	// Standing
			}
		}
	}

	void FixedUpdate() {

		// Controller for the player does not walking on the wall
		if (transform.position.y > MaxHeight.transform.position.y &&
			    (Anim.GetInteger ("action") == 0 || Anim.GetInteger ("action") == 1)) {	 // Standing or Walking		
			rb2D.transform.Translate (horizontalAxis * Speed * Time.deltaTime, -0.01f, 0.0f);
		} else if (!blocked) {
			rb2D.transform.Translate (horizontalAxis * Speed * Time.deltaTime, verticalAxis * Speed * Time.deltaTime, 0.0f);
		}
	}

	public void OnTriggerExit2D(Collider2D other) {

		if (other.tag == "Fire") {			
			burned = false;
		} 
	}

	public void OnCollisionEnter2D(Collision2D collision) {	

		if (collision.gameObject.tag == "Fire") {
			Anim.SetInteger ("action", 2);
			burned = true;
		} else if (collision.gameObject.tag == "Ball" && !jump) {
			if(BallClose(collision)) {
				rb2D.bodyType = RigidbodyType2D.Kinematic;
				ball = true;
			}
		}
	}

	public void OnTriggerEnter2D(Collider2D other) {

		if (other.tag == "Door_Standing") {		
			if (Door) {													// If the gameObject game already exists
				Door = null;
			}
			Door = GameObject.Find (other.gameObject.name);
			if (DoorAnim) {
				DoorAnim = null;
			}				
			DoorAnim = Door.GetComponent<Animator> ();
			DoorAnim.SetBool ("Standing", false);						// Set state of door standing false to not happening again
			enemyDoor = true;
			enemyDoorName = "Enemy" + other.gameObject.name;				// Build the enemy name
			enemyDoorPosition = "EnemyPosisition" + other.gameObject.name;	// Build the enemy position

		} else if (other.tag == "Fire") {								// Active burned state
			Anim.SetInteger ("action", 2);								// Burned state
			burned = true;
			UpdateLifeBar ();											// If burned lost 1 bar of life bar
		  
			// Test collision of the enemy considering in front or in back of him in x axis
		} else if ((other.tag == "FrontEdge" || other.tag == "BackEdge") && !burned && !jump && !attack1 && !hitBall && !hitByEnemy) {
			
			if (EnemyClose (other)) { // Check enemy position in y axis
				
				// Gets the actual sprite of the enemy to know the direction he is poiting
				SpriteRenderer EnemySprite = other.gameObject.GetComponentInParent (typeof(SpriteRenderer)) as SpriteRenderer;

				if ((other.tag == "FrontEdge" && !EnemySprite.flipX && !faceright) ||	// If enemy points right and player left
				    (other.tag == "BackEdge" && EnemySprite.flipX && faceright) || // If enemy points left and player right
				    (other.tag == "FrontEdge" && EnemySprite.flipX && !faceright) || // If enemy points left and player left
				    (other.tag == "BackEdge" && !EnemySprite.flipX && faceright)) {     // If enemy points rigth and player right

					blocked = true;														// player is blocked to continue forward													
					blockDirection = faceright;			
				}
			}

		} else if (other.tag == "Stair") {
			if (other.gameObject.name == "Stair_1" && !ballStair [0] && ballStairTime [0] >= 7.0f) {	// Active balls falling stair 
				SetBalls (0);
			} else if (other.gameObject.name == "Stair_2" && !ballStair [1] && ballStairTime [1] >= 7.0f) {	// Active balls falling stair 
				SetBalls (1);
			}
		} else if (other.tag == "EnemyTrigger") {

			int index = int.Parse (other.gameObject.name.Substring (0, 1));

			if (!enemiesTriggered [index]) {			// Check if the enemies is this collider trigger have alreadu spawned
				enemiesTriggered [index] = true;
				enemiesIndex = index;
				enemiesSpawn = 0;
				spawnEnemyTime = 0.0f;
				numberEnemies = int.Parse (other.gameObject.name.Substring (2, 1));
			}
		} else if (other.gameObject.name.Contains("CheckPoint")) {
			SaveCheckPoint ();
		} 
			

	}

	public bool EnemyClose(Collider2D other){

		Vector3 playerPosition;					// To get the current position of the player
		Vector3 enemyDoorPosition;				// To get the current position of the enemy

		Transform enemy = other.gameObject.GetComponentInParent (typeof(Transform)) as Transform;

		enemyDoorPosition = enemy.position;
		playerPosition = transform.position;

		if (playerPosition.y < 0.0f) {
			playerPosition.y *= -1;
		}

		if (enemyDoorPosition.y < 0.0f) {
			enemyDoorPosition.y *= -1;
		}

		if ( (playerPosition.y - enemyDoorPosition.y) > -1.0f && (playerPosition.y - enemyDoorPosition.y) < 1.0f ) {
			return true;
		}

		return false;
	}

	public bool BallClose(Collision2D collision){

		Vector3 playerPosition;					// To get the current position of the player
		Vector3 ballPosition;					// To get the current position of the ball

		Transform ball = collision.transform;

		ballPosition = ball.position;
		playerPosition = transform.position;

		if (playerPosition.y < 0.0f) {
			playerPosition.y *= -1;
		}

		if (ballPosition.y < 0.0f) {
			ballPosition.y *= -1;
		}

		if ((playerPosition.y - ballPosition.y) > -1.0f &&
			(playerPosition.y - ballPosition.y) < 1.0f) {

			return true;
		}

		return false;
	}

	public void SetBalls(int index) {
		
		string ball1Name = "";
		string ball2Name = "";

		if (index == 0) {
			ball1Name = "Ball1";
			ball2Name = "Ball2";
		} else {
			ball1Name = "Ball3";
			ball2Name = "Ball4";
		}

		ballStair[index] = true;
		ball1Stair[index] = false;
		ball2Stair[index] = false;
		ballStairTime[index] = 0.0f;

		GameObject ball1 = GameObject.Find (ball1Name);
		if (ball1 != null) {
			Destroy (ball1);	// Destroy ball that will be create againg in StartBalls()
		}

		GameObject ball2 = GameObject.Find (ball2Name);
		if (ball2 != null) {
			Destroy (ball2);	// Destroy ball that will be create againg in StartBalls()
		}
	}

	public void StartBalls() {

		string ball1Name = "";
		string ball2Name = "";
		int index = 0;

		if (ballStair [0]) {
			ball1Name = "Ball1";
			ball2Name = "Ball2";
			newPosition = GameObject.Find ("BallPositionStar_1").transform.position;		
		} else if (ballStair [1]) {
			ball1Name = "Ball3";
			ball2Name = "Ball4";
			newPosition = GameObject.Find ("BallPositionStar_2").transform.position;
			index = 1;
		}

		ballStairTime[0] += Time.deltaTime;
		ballStairTime[1] += Time.deltaTime;

		if (ballStair[index]) {
			
			newPosition.z = 0.0f;

			if ( ballStairTime[index] >= 0.0f && !ball1Stair[index] ) {
				GameObject tempball = Instantiate (StairBall, newPosition, Quaternion.identity) as GameObject;
				tempball.name = ball1Name;
				ball1Stair[index] = true;
			}

			if ( ballStairTime[index] >= 1.5f && !ball2Stair[index] ) {

				newPosition.x = newPosition.x - 1.0f;

				GameObject tempball = Instantiate (StairBall, newPosition, Quaternion.identity) as GameObject;
				tempball.name = ball2Name;
				ball2Stair[index] = true;
				ballStair [index] = false;
			}
		}
	}

	public void ActionsControll() {

		if (!hitBall) {

			// jump
			if (Input.GetButtonDown ("Jump") && !jump) {
				jump = ActiveAction (3, JumpForce);
				blocked = false;
			}
			jump = PhysicsControll (4, jump);

			// Attack 1
			if (Input.GetButtonDown ("Fire1") && !attack1) {
				attack1 = ActiveAction (5, AttackForce_1);
				allSoundEffects.PlayOneShot (playerPunch);
				blocked = false;
			}
			attack1 = PhysicsControll (6, attack1);
		}

		// Hit by the ball
		if (ball && !hitBall) {
			hitBall = ActiveAction (7, HitBallForce);
			blocked = false;
			UpdateLifeBar ();
		}
		hitBall = PhysicsControll (8, hitBall);
		ball = hitBall;
	}

	public bool ActiveAction(int action, float force) {

		if(action != 7) {		// Hit the ball
			rb2D.bodyType = RigidbodyType2D.Dynamic;
		}

		lastPosition = transform.position;
		if (lastPosition.y > MaxHeight.transform.position.y) {
			lastPosition.y = MaxHeight.transform.position.y - positionCalibration - 0.5f;
		}

		lastAction = Anim.GetInteger ("action");
		rb2D.gravityScale = 1;											// Set  scale to active the dynamic physics
		Anim.SetInteger ("action", action);								// Taking off
		rb2D.AddForce (new Vector2 (0.0f, force), ForceMode2D.Impulse);

		positTime = 0.5f;		// Set time for when verify last position with the actual position to set jump false
		actionTime = 0.0f;   	// When actionTime equal 0.25 set action to Jumping and exit action Taking off
		act2ndTime = 0.0f;	  	// Controlls the duration of the second action
		setActionFalse = false;

		if (lastAction == 2) {		// Burned
			lastPosition.y = safetyPositon;		// Set return position to a safety position
		}

		return true;	
	}


	public bool PhysicsControll(int second_action, bool action_active)  {
		
		// Action controller
		if (action_active) {

			actionTime += Time.deltaTime;

			act2ndTime += Time.deltaTime;

			positTime -= Time.deltaTime;

			if (actionTime >= 0.25f) {
				Anim.SetInteger ("action", second_action);   // Jumping/Attacking/Hit
			}

			if (second_action == 8 && act2ndTime >= 0.5f) { //Time of the duration of the action				
 				 setActionFalse = true;
			} else if (second_action != 8 && positTime <= 0.0f) { //Time to verify last position with the actual position to set action false
				if (transform.position.y <= (lastPosition.y + positionCalibration)) {				
					setActionFalse = true;			
				}
			}

			if (setActionFalse) {
				rb2D.bodyType = RigidbodyType2D.Static;
				rb2D.gravityScale = 0;
				rb2D.bodyType = RigidbodyType2D.Dynamic;
				action_active = false;
			}
		}

		return action_active;
	}

	public int GetPlayerPoints() {
		return playerPoints;	
	}

	public int GetPlayerLifes() {
		return playerLifes;
	}
		
	public string GetNameSearch() {
		return nameSearch;	
	}

	void  UpdatePlayerPoints() {	
		playerPoints++;
	}

	void SetBlockedkFalse() {
		blocked = false;
		hitByEnemy = false;
	}

	void UpdateLifeBar() {

		indexLifeBar++;

		if (indexLifeBar > 11) {
			indexLifeBar = 0;
			playerLifes -= 1;

			if (playerLifes < 0) {
				playerLifes = 2;
				SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);  // lost all lives the game start again
			}
		}

		LifeBar.GetComponent<SpriteRenderer> ().sprite = LifeBarList [indexLifeBar];
	}

	void IsHitByEnemy(bool isHit) {

		if (isHit) {
			hitByEnemy = true;
			allSoundEffects.PlayOneShot (enemyPunch);
			UpdateLifeBar ();
		} else {

			if (hitByEnemy) {

				hitByEnemyTime += Time.deltaTime;

				if (hitByEnemyTime >= 0.3f) {
					hitByEnemy = false;
					hitByEnemyTime = 0.0f;
				}
			}
		}
	}

	void enemyInstantiate(int index) {

		if (enemiesSpawn >= numberEnemies) {
			return;
		}

		nameSearch = "";

		string enemyName = "";

		spawnEnemyTime += Time.deltaTime;			

		if (spawnEnemyTime >= enemiesSpawn*1.0f) {
				
			nameSearch = "EnemyTrigger" + index.ToString () + enemiesSpawn.ToString () + "action";	// Get the string of enemy's action

			EnemyActions.Clear ();

			Transform actionRoot = GameObject.Find (nameSearch).transform;

			foreach (Transform actions in actionRoot) {
				EnemyActions.Add (actions);
				break;
			}			

			enemyName = "EnemyTrigger" + index.ToString () + enemiesSpawn.ToString ();
			GameObject temp_Enemy = Instantiate (EnemyTrigger, EnemyActions [0].position, Quaternion.identity) as GameObject;
			temp_Enemy.name = enemyName;
			enemiesSpawn++;
		}

	}

	void setnumberEnemiesTrigger() {
		enemiesTriggered [0] = false;
		enemiesTriggered [1] = false;
		enemiesTriggered [2] = false;
		enemiesTriggered [3] = false;
		enemiesTriggered [4] = false;
		enemiesTriggered [5] = false;
		enemiesTriggered [6] = false;
		enemiesTriggered [7] = false;
	}

	void SaveCheckPoint() {
		PlayerPrefs.SetFloat("PlayerPosX", transform.position.x);
		PlayerPrefs.SetFloat("PlayerPosY", transform.position.y);
		PlayerPrefs.SetFloat("PlayerPosZ", transform.position.z);
	}

	void LoadPosition()	{	
		Vector3 playerPosition = new Vector3 (PlayerPrefs.GetFloat ("PlayerPosX"), PlayerPrefs.GetFloat ("PlayerPosY"), PlayerPrefs.GetFloat ("PlayerPosZ"));
		transform.position = playerPosition;

	}
}