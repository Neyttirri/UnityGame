using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class waterBall : MonoBehaviour
{
	public GameObject target;
	public float speed;
	public Rigidbody2D waterBallRB;
	
    // Start is called before the first frame update
    void Start()
    {
        waterBallRB = GetComponent<Rigidbody2D>();
		target = GameObject.FindGameObjectWithTag("Player");
		Vector2 moveDir = (target.transform.position - transform.position).normalized * speed;
		waterBallRB.velocity = new Vector2(moveDir.x, moveDir.y);
		Destroy(this.gameObject, 2);
    }

    private void OnCollisionEnter2D(Collision2D collision)
 	{
		if (collision.gameObject.CompareTag("Wall")) 
     	{
       	 	Destroy(gameObject);
       	}
 	}
}
