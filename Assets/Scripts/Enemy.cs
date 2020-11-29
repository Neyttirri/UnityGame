using System.Collections;
using System.Collections.Generic;

using UnityEngine;





public class Enemy : MonoBehaviour
{
    public enum EnemyState
    {
        Idle,
        Wander,
        Follow,
        Attack,
        Die
    };

    public enum EnemyType
    {
        Weak,
        Strong
    };

    GameObject player;
    public EnemyState currState = EnemyState.Idle;
    public EnemyType enemyType;
    public float range;
    public float speed;
    public float attackRange;
    public float coolDown;
    private bool coolDownAttack = false;
    private Vector3 randomDir;
    public int countDown;
    public GameObject particleEffects;


    


    // Start is called before the first frame update
    void Start()
    {

        countDown = 3;
    }
    

    // Update is called once per frame
    void Update()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        switch (currState)
        {
            case (EnemyState.Idle):
                Idle();
                break;
            case (EnemyState.Wander):
                Wander();
                break;
            case (EnemyState.Follow):
                Follow();
                break;
            case (EnemyState.Attack):
                Attack();
                break;
            case (EnemyState.Die):
                Die();
                break;
        }
        //avoid Null Reference each time a new level loads
        if (player != null)
        {
            if (IsPlayerInRange(range) && currState != EnemyState.Die)
            {
                currState = EnemyState.Follow;
            }
            else if (!IsPlayerInRange(range) && currState != EnemyState.Die)
            {
                currState = EnemyState.Wander;
            }
            if (Vector3.Distance(transform.position, player.transform.position) <= attackRange)
            {
                currState = EnemyState.Attack;
            }
        }
        else { currState = EnemyState.Idle; }

    }
    private bool IsPlayerInRange(float range)
    {
       
            return (player.transform.position - transform.position).sqrMagnitude <= range;
        
    }

   /* private IEnumerator ChooseDirection()
    {
        chooseDir = true;
        yield return new WaitForSeconds(Random.Range(0.5f, 1.5f));
        randomDir = new Vector3(0, 0, Random.Range(0, 60));
        Quaternion nextRotation = Quaternion.Euler(randomDir);
        transform.rotation = Quaternion.Lerp(transform.rotation, nextRotation, Random.Range(0.5f, 1f));
        chooseDir = false;
    }*/
    void Idle() { return; }

    void Wander()
    {
        
   
           /* if (!chooseDir)
            {
                StartCoroutine(ChooseDirection());
            }*/

            transform.position += -transform.right * speed * Time.deltaTime;
            if (IsPlayerInRange(range))
            {
                currState = EnemyState.Follow;
            }
        
    }

    void Follow()
    {
        
            transform.position = Vector2.MoveTowards(this.transform.position, player.transform.position, speed * Time.deltaTime);
        }
    

    void Attack()
    {
        if (!coolDownAttack)
        {
            switch (enemyType)
            {
                case (EnemyType.Weak):
                    //GameController.DamagePlayer(1);
                    StartCoroutine(CoolDown());
                    break;
                case (EnemyType.Strong):
                    // GameController.DamagePlayer(2);
                    StartCoroutine(CoolDown());
                    break;
            }
        }
    }
    void Die()
    {

        Destroy(gameObject);
        GameObject VFX = Instantiate(particleEffects, transform.position, transform.rotation);
        Destroy(VFX, 0.5f);
    }

    private IEnumerator CoolDown()
    {
        coolDownAttack = true;
        yield return new WaitForSeconds(coolDown);
        coolDownAttack = false;
    }
    void OnCollisionEnter2D(Collision2D col)
    {
       if (col.gameObject.CompareTag("Projectile"))
        {
			Debug.Log("bullet hitting an enemy");
			Destroy(col.gameObject);
            countDown--;
            Debug.Log("enemy life: " + countDown);
            if (countDown == 2) {
                GetComponent<SpriteRenderer>().color = Color.gray;
            }
            else if (countDown == 1) {
                GetComponent<SpriteRenderer>().color = Color.red;
            }
            else if(countDown<=0) { 
                Die(); 
            }
        }
        
    }
}

