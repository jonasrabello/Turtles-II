using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPurpleController : MonoBehaviour {

	public BoxCollider2D boxCollider;
	public EdgeCollider2D frontEdge;
	public EdgeCollider2D backEdge;
	public BoxCollider2D punch;
	public AudioClip enemyExploding;

	GameObject max_height;
	GameObject max_low;
	GameObject player;

	Animator anim;
	Animator player_anim;

	AudioSource allSoundEffects;

	int last_state;

	float gap_distance;
	float last_position;
	float state_0_time;
	float hit_time;
	float killed_time;
	float explode_time;
	float fightingTime;
	float walkingTime;
	float player_position_y;

	bool faceright;
	bool hit;
	bool set_time_hit;
	bool set_time_killed;
	bool set_time_explode;
	bool burned;
	bool max_low_position;
	bool isFighting;
	bool isWalking;

	string attack;

	Rigidbody2D rb2D;

	SpriteRenderer eSpriteRenderer;			// To change the side of the player according to left or right

	BoxCollider2D attack_collider_back;

	// Runs allways first
	void Awake() {

		anim = GetComponent<Animator> ();
		rb2D = GetComponent<Rigidbody2D>();

		max_height = GameObject.Find ("MaxHeigthPosition");
		max_low = GameObject.Find ("MaxLowPosition");

		allSoundEffects = GetComponent<AudioSource>();

		GameObject EnemyPosisition = GameObject.Find ("EnemyPosisitionDoor_1");

		if (transform.position == EnemyPosisition.transform.position) {

			anim.SetInteger ("state", 0);				// Breaking door
		}
		else {			
			anim.SetInteger ("state", 1);				// Walking
		}

		faceright = true;

		hit = false;

		eSpriteRenderer = GetComponent<SpriteRenderer> ();

		gap_distance = 2.0f;

		state_0_time = 0.0f;

		last_position = transform.position.x;

		player = GameObject.Find ("Leonardo");

		player_anim = player.GetComponent<Animator> ();

		fightingTime = 0.0f;
		walkingTime = 0.0f;
		isFighting = true;
		isWalking = false;
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

		playerAttackController ();

		ChaseController ();
	}

	public void ChaseController() {

		last_state = anim.GetInteger ("state");

		if (last_state == 0) {					// Punching the door

			state_0_time += Time.deltaTime;

			if (state_0_time >= 0.3f) {
				anim.SetInteger ("state", 1);	// Walking
				state_0_time = 0.0f;
			}
		}

		// Controller to enemy can not go to fire
		max_low_position = false;
		if (player.transform.position.y > max_low.transform.position.y) {
			max_low_position = true;
		}

		// Chansing the player
		if (last_state != 0 && !hit && max_low_position) { 	

			// Face right or left controller
			if (transform.position.x < last_position && faceright) {
				eSpriteRenderer.flipX = true;
				faceright = false;
			} else if (transform.position.x > last_position && !faceright) {
				eSpriteRenderer.flipX = false;
				faceright = true;
			}
			last_position = transform.position.x;

			if ((Mathf.Abs(player.transform.position.x - transform.position.x) > gap_distance ||
				 Mathf.Abs(player.transform.position.y - transform.position.y) > gap_distance)) {

				// Controller for the enemy does not walking on the wall
				if (player.transform.position.y > max_height.transform.position.y) {
					player_position_y = max_height.transform.position.y;
				} else {				
					player_position_y = player.transform.position.y;
				}

				anim.SetInteger ("state", 1);	// Walking
				transform.position = Vector2.MoveTowards (new Vector2 (transform.position.x, transform.position.y), 
					new Vector2 (player.transform.position.x, player_position_y), 0.08f);

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

	public void playerAttackController() {

		float force_x_direction;

		// Set direction of 
		if (eSpriteRenderer.flipX == true) {
			force_x_direction = 5.0f;
		} else {
			force_x_direction = -5.0f;
		}

		if (hit && hit_time == 0.0f && player_anim.GetInteger ("action") == 6) {  // Attack 1

			anim.SetInteger ("state", 3);	// Hit
			rb2D.gravityScale = 1;
			rb2D.AddForce (new Vector2 (force_x_direction,3.0f), ForceMode2D.Impulse);

			set_time_hit = true;

			player.SendMessage("UpdatePlayerPoints");
			boxCollider.enabled = false;
			frontEdge.enabled = false;
			backEdge.enabled = false;
			punch.enabled = false;
		}

		if (set_time_hit) {

			hit_time  += Time.deltaTime;
			if (hit_time >= 0.1f) {
				anim.SetInteger ("state", 4);	// set_time_killed
				set_time_killed = true;
				set_time_hit = false;
			}
		}

		if (set_time_killed) {

			killed_time += Time.deltaTime;

			if (killed_time >= 0.9f) {				
				anim.SetInteger ("state", 5);	// Explode
				allSoundEffects.PlayOneShot(enemyExploding);
				rb2D.gravityScale = 0;
				rb2D.bodyType = RigidbodyType2D.Static;
				set_time_explode = true;
				set_time_killed = false;
			}
		}

		if (set_time_explode) {

			explode_time += Time.deltaTime;

			if (explode_time > 1.0f) {
				Destroy (gameObject);
				hit = false;
			}
		}

	}

	public void OnTriggerEnter2D(Collider2D other) {

		if (other.tag == "Attack") {
			hit = true;
			set_time_hit = false;
			set_time_killed = false;
			set_time_explode = false;
			hit_time = 0.0f;
			killed_time = 0.0f;
			explode_time = 0.0f;			 
		} 
	}
}