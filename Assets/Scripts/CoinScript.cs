using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Completed
{
public class CoinScript : MonoBehaviour
{
    [HideInInspector] public GameManager gm;
    private int coinsForUpgrade;
    private GameObject[] coins;
    
    void Awake()
    {
    	gm = GameObject.Find("GameManager").GetComponent<GameManager>();
    	coinsForUpgrade = gm.GetAmountCoinsForUpgrade();
    	coins = GameObject.FindGameObjectsWithTag("Coin");
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
 	{
		// all the ckecks (for upgrade and for the exit) are here instead of in the update method in the GM
		// because here the code will be executed way less times.. although it's not too beautiful 

 		if (collision.gameObject.CompareTag("Player")) 
     	{
     		coins = GameObject.FindGameObjectsWithTag("Coin");
       	 	Destroy(gameObject);
       	 	gm.Coin++;
       	 	
       	 	// check if that was the last coin
       	 	if(coins.Length == 1)
       	 	{
       	 		gm.SetExitActive();
                gm.ShowExitAnnounce();
            }
       	 	
       	 	if(gm.Coin == coinsForUpgrade)
       	 	{
       	 		gm.ActivateUpgrade();
       	 	}
     	}
 	}
}
}