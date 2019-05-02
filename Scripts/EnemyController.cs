using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour {

	public BoxCollider2D boxCollider;
	public EdgeCollider2D frontEdge;
	public EdgeCollider2D backEdge;
	public BoxCollider2D punch;
	public AudioClip enemyExploding;
	public float JumpForce;
	public List<Transform> enemyActions;

	GameObject maxHeight;
	GameObject maxLow;
	GameObject player;

	Animator anim;
	Animator playerAnim;

	AudioSource allSoundEffects;

	Vector3 lastPosition;					// To store the last position of the enemy when he is jumping

	int last_state;
	int actionNumber;						// actionIndex of A.I. Enemies array in Leonardo controller 
	int actionIndex;						// Actual actionIndex in action

	float gap_distance;
	float lastPositionX;
	float forcePositionX;
	float state_0_time;
	float hitTime;
	float killedTime;
	float explode_time;
	float fightingTime;
	float walkingTime;
	float takingOffTime;
	float playerPositionY;
	float positionCalibration;				// To set the precision position y, when it comes back to the floor
	float speed;

	bool faceright;
	bool hit;
	bool setTimeHit;
	bool setTimeKilled;
	bool setTimeExplode;
	bool maxLow_position;
	bool isFighting;
	bool isWalking;
	bool setMove;
	bool checkJump;
	bool setJump;
	bool jump;
	bool setChase;
	bool setActionFalse;					// To set action to false
	float positTime;						// To contol time to start position verification of the player
	float actionTime;						// To control the duration of the first action when there are two

	string attack;

	Rigidbody2D rb2D;

	SpriteRenderer eSpriteRenderer;			// To change the side of the player according to left or right

	BoxCollider2D attack_collider_back;

	// Runs allways first
	void Awake() {

		anim = GetComponent<Animator> ();
		rb2D = GetComponent<Rigidbody2D>();

		GetEnemyActions ();

		maxHeight = GameObject.Find ("MaxHeigthPosition");
		maxLow = GameObject.Find ("MaxLowPosition");

		allSoundEffects = GetComponent<AudioSource>();

		eSpriteRenderer = GetComponent<SpriteRenderer> ();

		player = GameObject.Find ("Leonardo");

		playerAnim = player.GetComponent<Animator> ();

		faceright = true;

		hit = false;

		gap_distance = 2.0f;

		state_0_time = 0.0f;

		speed = 0.08f;

		lastPositionX = transform.position.x;

		positionCalibration = 0.16f;   // Set a calibration position to compare positions when player jumps

		JumpForce = 6.0f;
		forcePositionX = 0.0f;
		fightingTime = 0.0f;
		walkingTime = 0.0f;
		isFighting = true;
		isWalking = false;

		setMove = false;
		setJump = false;
		setChase = false;
		checkJump = true;	

		// Get all the position door that has the tag to break the door
		GameObject[] EnemyPosisition = GameObject.FindGameObjectsWithTag ("Breaking");

		// Check the if the position of the enemy is in the same position of the door was broken
		for (int i = 0; i < EnemyPosisition.Length; i++) {

			if (transform.position == EnemyPosisition [i].transform.position) {
				anim.SetInteger ("state", 0);				// Breaking door
				break;
			} else {			
				anim.SetInteger ("state", 1);				// Walking
			}
		}

	}

	// Use this for initialization
	void Start () {		
	
	}
	
	// Update is called once per frame
	void Update () {

		// Controll of the position between enemy and player
		if ( (player.transform.localPosition.y - transform.localPosition.y ) >= 1.0f) {
			eSpriteRenderer.sortingOrder = 0;

		} else {
			eSpriteRenderer.sortingOrder = -1;
		}

		CheckAction ();

		ActionsControll ();
			
		playerAttackController ();

		if (setChase) {
			ChaseController ();
		}

	}

	void ChaseController() {

		last_state = anim.GetInteger ("state");

		if (last_state == 0) {					// Punching the door

			state_0_time += Time.deltaTime;

			if (state_0_time >= 0.3f) {
				anim.SetInteger ("state", 1);	// Walking
				state_0_time = 0.0f;
			}
		}

		// Controller to enemy can not go to fire
		maxLow_position = false;
		if (player.transform.position.y > maxLow.transform.position.y) { 
			maxLow_position = true;	
		}

		// Chansing the player
		if (last_state != 0 && !hit && maxLow_position) { 

			// Face right or left controller
			if (transform.position.x < lastPositionX && faceright) {
				eSpriteRenderer.flipX = true;
				faceright = false;
			} else if (transform.position.x > lastPositionX && !faceright) {
				eSpriteRenderer.flipX = false;
				faceright = true;
			}
			lastPositionX = transform.position.x;

			if ((Mathf.Abs(player.transform.position.x - transform.position.x) > gap_distance ||
				 Mathf.Abs(player.transform.position.y - transform.position.y) > gap_distance)) {

				// Controller for the enemy does not walking on the wall
				if (player.transform.position.y > maxHeight.transform.position.y) {
					playerPositionY = maxHeight.transform.position.y;
				} else {				
					playerPositionY = player.transform.position.y;
				}

				anim.SetInteger ("state", 1);	// Walking
				transform.position = Vector2.MoveTowards (new Vector2 (transform.position.x, transform.position.y), 
					new Vector2 (player.transform.position.x, playerPositionY), speed);

			} else  {

				if (isFighting) {


					anim.SetInteger ("state", 2);	// Fighting 1

					fightingTime += Time.deltaTime;
					isWalking = false;

					if (fightingTime >= 0.5f) {
						player.SendMessage ("IsHitByEnemy", true);
						walkingTime = 0.0f;
						isWalking = true;
					}
				}

				player.SendMessage("IsHitByEnemy", false);

				if (isWalking) {

					anim.SetInteger ("state", 1);	// Walking

					walkingTime += Time.deltaTime;
					isFighting = false;

					if (walkingTime >= 2.0f) {						
						fightingTime = 0.0f;
						isFighting = true;
					}
				}
			}
		}
	}

	void playerAttackController() {

		float force_x_direction;

		// Set direction of 
		if (eSpriteRenderer.flipX == true) {
			force_x_direction = 5.0f;
		} else {
			force_x_direction = -5.0f;
		}

		if (hit && hitTime == 0.0f && playerAnim.GetInteger ("action") == 6) {  // Attack 1

			anim.SetInteger ("state", 3);	// Hit
			rb2D.gravityScale = 1;
			rb2D.AddForce (new Vector2 (force_x_direction,3.0f), ForceMode2D.Impulse);

			setTimeHit = true;

			player.SendMessage("UpdatePlayerPoints");
			player.SendMessage("SetBlockedkFalse");
			boxCollider.enabled = false;
			frontEdge.enabled = false;
			backEdge.enabled = false;
			punch.enabled = false;
		}

		if (setTimeHit) {

			hitTime  += Time.deltaTime;
			if (hitTime >= 0.2f) {
				anim.SetInteger ("state", 4);	// setTimeKilled
				setTimeKilled = true;
				setTimeHit = false;
			}
		}

		if (setTimeKilled) {

			killedTime += Time.deltaTime;

			if (killedTime >= 0.9f) {				
				anim.SetInteger ("state", 5);	// Explode
				allSoundEffects.PlayOneShot(enemyExploding);
				rb2D.gravityScale = 0;
				rb2D.bodyType = RigidbodyType2D.Static;
				setTimeExplode = true;
				setTimeKilled = false;
			}
		}

		if (setTimeExplode) {

			explode_time += Time.deltaTime;

			if (explode_time > 1.0f) {
				Destroy (gameObject);
				hit = false;
			}
		}

	}

	void OnTriggerEnter2D(Collider2D other) {

		if (other.tag == "Attack") {
			if (!jump && PlayerClose(other)) {
				hit = true;
				setTimeHit = false;
				setTimeKilled = false;
				setTimeExplode = false;
				hitTime = 0.0f;
				killedTime = 0.0f;
				explode_time = 0.0f;
			}
		} 
	}

	public bool PlayerClose(Collider2D other){

		Vector3 playerPosition;				// To get the current position of the player
		Vector3 enemyPosition;				// To get the current position of the enemy

		Transform player = GetComponent<Transform>();

		enemyPosition = transform.position;
		playerPosition = player.position;

		if (playerPosition.y < 0.0f) {
			playerPosition.y *= -1;
		}

		if (enemyPosition.y < 0.0f) {
			enemyPosition.y *= -1;
		}

		if ( (enemyPosition.y - playerPosition.y) > -0.5f && (enemyPosition.y - playerPosition.y) < 0.5f ) {
			return true;
		}

		return false;
	}

	void ActionsControll() {

		if (setJump) {
			last_state = anim.GetInteger ("state");
			// Set direction of
			if (eSpriteRenderer.flipX == true) {
				forcePositionX = -7.0f;
			} else {
				forcePositionX = 7.0f;
			}
			jump = ActiveAction (6, JumpForce);
			setJump = false;
		}
		jump = PhysicsControll (7, jump);

		if (setMove) {

			// Check the enemy has reached the position set move towards CheckAction funcion
			if (transform.position.x >= enemyActions [actionIndex + 1].transform.position.x) {
				setMove = false;
				actionIndex += 1;
			}
		}
	}

	bool ActiveAction (int action, float force) {

		lastPosition = transform.position;
		if (lastPosition.y > maxHeight.transform.position.y) {
			lastPosition.y = maxHeight.transform.position.y - positionCalibration - 0.5f;
		}

		rb2D.gravityScale = 1;											// Set  scale to active the dynamic physics
		anim.SetInteger ("state", action);								// Taking off
		rb2D.AddForce (new Vector2 (forcePositionX, force), ForceMode2D.Impulse);

		positTime = 0.0f;	  	// Set time for when verify last position with the actual position to set jump false
		actionTime = 0.0f;		// When actionTime equal 0.25 set action to Jumping and exit action Taking off
		setActionFalse = false;

		return true;	
	}

	bool PhysicsControll(int second_action, bool action_active)  {

		// Action controller
		if (action_active) {

			actionTime += Time.deltaTime;

			positTime += Time.deltaTime;

			if (actionTime >= 0.6f) {
				anim.SetInteger ("state", second_action);   // Jumping
			}

			if (positTime > 0.5f) { //Time to verify last position with the actual position to set action false
					if (transform.position.y <= (lastPosition.y + positionCalibration)) {				
						setActionFalse = true;			
				}
			}

			if (setActionFalse) {
				rb2D.bodyType = RigidbodyType2D.Static;
				rb2D.gravityScale = 0;
				rb2D.bodyType = RigidbodyType2D.Dynamic;
				action_active = false;
				anim.SetInteger ("state",last_state);
				actionIndex += 1;
			}
		}

		return action_active;
	}

	void GetEnemyActions() {

		if (gameObject.tag != "Enemy") {
			return;
		}

		string nameSearch = GameObject.Find ("Leonardo").GetComponent<LeonardoController> ().GetNameSearch ();

		Transform actionRoot = GameObject.Find(nameSearch).transform;

		actionIndex = 0;
		actionNumber = 0;

		if (actionRoot) {

			foreach (Transform actions in actionRoot) {
				enemyActions.Add (actions);
				actionNumber++;
			}
		}
	}

	void CheckAction () {
	
		if (gameObject.tag != "Enemy") {
			return;
		}

		if (actionIndex > actionNumber-1) {
			return;
		}

		if (enemyActions [actionIndex].name.Contains ("Flip")) {
			eSpriteRenderer.flipX = true;
			actionIndex += 1;
		} else if (enemyActions [actionIndex].name.Contains ("Move")) {
			setMove = true;
			transform.position = Vector3.MoveTowards (transform.position, enemyActions [actionIndex + 1].transform.position, speed);		
		} else if (enemyActions [actionIndex].name.Contains ("Jump") && checkJump) {
			setJump = true;
			checkJump = false;
		}  else if (enemyActions [actionIndex].name.Contains ("Chase") && !setChase) {
			setChase = true;
			actionIndex += 1;
		}	
	}

	void SetChase() {
		setChase = true;		
	}
}