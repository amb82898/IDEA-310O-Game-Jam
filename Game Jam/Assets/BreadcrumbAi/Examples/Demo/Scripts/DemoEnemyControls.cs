using UnityEngine;
using System.Collections;
using BreadcrumbAi;

[System.Serializable]
public class DemoEnemySounds{
	public AudioClip audio_hit_1, audio_hit_2, audio_dead_1, audio_dead_2, audio_melee_attack_1, audio_melee_attack_2;
}

public class DemoEnemyControls : MonoBehaviour {

	public DemoEnemySounds audioClips;
	public enum EnemyType {Melee, Ranged, Special};
	public GameObject healthPickUpPrefab;
	public bool _canDropPickUp;
	public EnemyType enemyType;
	public Rigidbody rangedProjectilePrefab;
	
	public GameObject bloodPrefab;
	public GameObject specialPrefab;
	private Transform player;
	
	private Ai ai;
	
	private bool _removeBody, _isHit, _animAttack;
	private AudioSource audioSource;
	
	private float rangedAttackNext = 0.0f;
	private float rangedAttackRate = 2.0f;
	private float meleeAttackNext = 0.0f;
	private float meleeAttackRate = 1.0f;
	
	private Animator anim;
	private string animRun = "Run";
	private string animDeath1 = "Death1";
	private string animAttack = "Attack";
	
	
	private bool _pointScored;

  

    void Start(){
		ai = GetComponent<Ai>();
		anim = GetComponent<Animator>();
		audioSource = gameObject.AddComponent<AudioSource>();
		GameObject go = GameObject.FindGameObjectWithTag("Player");
		if(go){
			player = go.transform;
		}
	}
	
	void Update () {
		CheckHealth();
		CheckDeathZone();
	}
	
	void FixedUpdate(){
		Animation();
		Attack();
	}
	
	private void CheckDeathZone(){
		if(transform.position.y < -10 || transform.position.y > 10){
			UpdateEnemyCount();
		}
	}
	
	private void Animation(){
		if(ai.lifeState == Ai.LIFE_STATE.IsAlive){
			if(ai.moveState != Ai.MOVEMENT_STATE.IsIdle){
				anim.SetBool(animRun, true);
			} else {
				anim.SetBool(animRun, false);
			}
			if(_animAttack){
				anim.SetBool(animAttack, true);
			} else {
				anim.SetBool(animAttack, false);
			}
        } else if(ai.lifeState == Ai.LIFE_STATE.IsDead){
            anim.SetBool(animDeath1, true);
        }
    }
    
    private void Attack(){
    	if(player){
	    	if(ai.lifeState == Ai.LIFE_STATE.IsAlive){
		    	if(enemyType != EnemyType.Ranged){
					if(ai.attackState == Ai.ATTACK_STATE.CanAttackPlayer && Time.time > meleeAttackNext){
						meleeAttackNext = Time.time + meleeAttackRate;
						float rand = Random.value;
						if(rand <= 0.4f){
							audioSource.clip = audioClips.audio_melee_attack_1;
						} else {
							audioSource.clip = audioClips.audio_melee_attack_2;
						}
						audioSource.PlayOneShot(audioSource.clip);
						player.GetComponent<DemoPlayerControls>()._isHit = true;
						player.GetComponent<DemoPlayerControls>().Bleed(transform.rotation);
						_animAttack = true;
					} else {
						_animAttack = false;
					}
		    	} else {
					if(ai.attackState == Ai.ATTACK_STATE.CanAttackPlayer && Time.time > rangedAttackNext){
						rangedAttackNext = Time.time + rangedAttackRate;
						Rigidbody spit = Instantiate(rangedProjectilePrefab, transform.position + transform.forward + transform.up, transform.rotation) as Rigidbody;
						spit.AddForce(transform.forward * 500);
						_animAttack = true;
					} else {
						_animAttack = false;
					}
		    	}
	    	}
    	}
    }

    public bool _isDead = false;
    private void CheckHealth()
    {
        if (_isDead) return;

        if (_isHit && this != null)
        {
            float rand = Random.value;
            if (ai.Health > 0)
            {
                if (rand > 0.5f)
                {
                    if (rand < 0.7f)
                    {
                        audioSource.clip = audioClips.audio_hit_2;
                    }
                    else
                    {
                        audioSource.clip = audioClips.audio_hit_1;
                    }
                    audioSource.PlayOneShot(audioSource.clip);
                }
            }
            if (ai.Health <= 0)
            {
                if (rand > 0.5f)
                {
                    audioSource.clip = audioClips.audio_dead_1;
                }
                else
                {
                    audioSource.clip = audioClips.audio_dead_2;
                }
                audioSource.PlayOneShot(audioSource.clip);
            }
            _isHit = false;
        }

        if (ai.lifeState == Ai.LIFE_STATE.IsDead)
        {
            // Force the death animation trigger
            anim.SetBool(animDeath1, true);

            _isDead = true;
            Debug.Log("[CheckHealth] Life State is Dead. Triggering OnDeath.");

            StartCoroutine(DelayedDeath());
        }
    }
    private IEnumerator DelayedDeath()
    {
        yield return new WaitForSeconds(1.5f); // Wait for animation to play out

        Debug.Log("[DelayedDeath] Destroying components after animation.");
        OnDeath();

        // Optional: Disable the script entirely
        this.enabled = false;

        // Proceed to destroy components after animation
        Destroy(GetComponent<Rigidbody>());
        Destroy(GetComponent<Collider>());
        Destroy(GetComponent<Ai>());
        Destroy(anim); // Detach animator to prevent looping
    }

    IEnumerator DestroyBody(){
		if(enemyType == EnemyType.Special){
			Destroy(specialPrefab);
		}
		yield return new WaitForSeconds(2);

		Invoke("UpdateEnemyCount", 3);
		_removeBody = true;
	}
	
	void UpdateEnemyCount(){
		if(enemyType == EnemyType.Special){
			GameObject.Find("Spawners").GetComponent<DemoSpawnerControl>().specialEnemyCount--;
		}
		GameObject.Find("Spawners").GetComponent<DemoSpawnerControl>().enemyCount--;
		Destroy(gameObject);
	}

    public EnemyAlignment enemyAlignment;

    public void OnDeath()
    {
        if (_pointScored)
		{
            Debug.Log("[OnDeath] Already scored, not running again.");
            return;
        }  
		// Prevents double execution

        Debug.Log("Enemy Died and OnDeath Called");
        if (DungeonMoodManager.Instance != null)
        {
            bool towardsFriendly = enemyAlignment == EnemyAlignment.HostileAligned;
            DungeonMoodManager.Instance.ShiftMood(towardsFriendly);
        }

        _pointScored = true;
    }

    void OnCollisionEnter(Collision col){
		if(col.collider.name.Contains("Bullet")){
			_isHit = true;
			ai.Health -= 25;
			GameObject blood = Instantiate(bloodPrefab, col.collider.transform.position, col.collider.transform.rotation) as GameObject;
			Destroy(blood, 3);
		}
        else if (col.collider.name.Contains("Sword"))
        {
            _isHit = true;
            ai.Health -= 40;
			CheckHealth();
            GameObject blood = Instantiate(bloodPrefab, col.collider.transform.position, col.collider.transform.rotation) as GameObject;
            Destroy(blood, 3);
        }
    }
}
