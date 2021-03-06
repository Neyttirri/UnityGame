﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Completed
{
public class Boss : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private float lineOfSite;
    [SerializeField] private float shootingRange;
    [SerializeField] private GameObject waterBall;
	[SerializeField] private GameObject waterBallOrigin;
    [SerializeField] private float shootRate = 1f;
    [SerializeField] private float nextShootTime;
    [SerializeField] private int lifes;
    [SerializeField] private GameObject healthbar;
    private GameObject player;
    private GameManager gm;
    private Transform bar;
    private float scaleChange;
    private GameObject health;
    private float barWidth;
    private float barHeight;
    
    
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        health = Instantiate(healthbar);
        bar = health.transform.Find("Bar");
        barWidth = ((RectTransform)health.transform).rect.width;
        barWidth = ((RectTransform)health.transform).rect.height;
        scaleChange = 1f / lifes;

    }

    // Update is called once per frame
    void Update()
    {
    	// Vector3 topRight = new Vector3(Camera.main.pixelWidth, Camera.main.pixelHeight, 0f);
 		// health.transform.position = new Vector2(Camera.main.ScreenToWorldPoint(topRight).x, Camera.main.ScreenToWorldPoint(topRight).y);
 		
        float distanceFromPlayer = Vector2.Distance(player.transform.position, this.transform.position);
        if (distanceFromPlayer <= lineOfSite && distanceFromPlayer >= shootingRange)
        {
            transform.position = Vector2.MoveTowards(this.transform.position, player.transform.position, speed * Time.deltaTime);
        } 
        else if (distanceFromPlayer < shootingRange && nextShootTime < Time.time)
        {
            ShootWaterBall();
        } 
    }
	
	private void FixedUpdate()
	{
		health. transform.position = new Vector3(Camera.main.ViewportToWorldPoint(new Vector3(1,1,0)).x - 1.9f, Camera.main.ViewportToWorldPoint(new Vector3(1,1,0)).y - 0.5f, 0);
	}
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(this.transform.position, lineOfSite);
        Gizmos.DrawWireSphere(this.transform.position, shootingRange);
    }		
	private void ShootWaterBall()
	{
		Instantiate(waterBall, waterBallOrigin.transform.position, Quaternion.identity);
        nextShootTime = Time.time + shootRate;
	}
	
	void OnCollisionEnter2D(Collision2D col)
    {
       if (col.gameObject.CompareTag("Projectile"))
        {
        	ReduceLife();
        	Destroy(col.gameObject);
            GetComponent<SpriteRenderer>().color = Color.gray;
        } 
    }
    
    private void ReduceLife()
    {
    	Vector3 temp = bar.localScale;
    	temp.x -= scaleChange;
    	bar.localScale = temp;
    	if(temp.x <= 0)
    	{
    		Debug.Log("boss is dead");
            gm.RevealNemo();
            Destroy(health);
            Destroy(gameObject);
    	}
    }
}
}