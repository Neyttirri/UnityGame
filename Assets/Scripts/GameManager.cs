using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Completed

{
    public class GameManager : MonoBehaviour
    {

        public BoardManager boardScript;

        public Text coinsText;
        public Text levelText;
        public Text timerText;
		public Text bestTimeText;
		public Text currentTimeText;

        public GameObject panelMenu;
        public GameObject panelPlay;
        public GameObject panelUpgrade;
        public GameObject panelGameOver;
        public GameObject panelLevelCompleted;
        public GameObject panelVictory;

        [SerializeField] private Image healthbarImage;
        [SerializeField] private Sprite[] healthbarImages;
        [SerializeField] private int neededForUpgradeCoins = 10;

        public int bossLevel;
        
        private GameObject pauseGameText;
		private GameObject newHighscoreText;
        
        public static GameManager Instance { get; private set; }
        public enum State { MENU, INIT, PLAY, LEVELCOMPLETED, LOADLEVEL, UPGRADE, GAMEOVER, VICTORY }
        private readonly int MAX_LEVELS = 6;
    
        private State _state;
        private bool _isSwitchingState;
		private float time;
		private float startTime; 
		private float bestTime = 0;

        private int _coins;
        public int Coin
        {
            get { return _coins; }
            set
            {
                _coins = value;
                coinsText.text = "x " + _coins;
            }
        }

        private int _lifes;
        public int Lifes
        {
            get { return _lifes; }
            set { _lifes = value; }
        }
		
		private int level = 1;  
        public int Level
        {
            get { return level; }
            set
            {
                level = value;
                levelText.text = "LEVEL: " + level;
            }
        }
        
         

        public void PlayClicked()
        {
            SwitchState(State.INIT);
        }
        
        public void GoBackToMenuClicked()
        {
            Debug.Log("button go back to menu clicked");
            SwitchState(State.MENU);
        }

        public void ButtonFasterClicked()
        {
            Debug.Log("faster chosen");
			boardScript.playerPrefab.GetComponent<PlayerController>().useSpeed(1);
            SwitchState(State.PLAY);
        }

        public void ButtonShieldClicked()
        {
            Debug.Log("shield chosen");
            // code for (color) shield for player
            
            boardScript.GetPlayerController().useShield();
            // TODO: still include timeout or whatever
            SwitchState(State.PLAY);
        }

        void Awake()
        {
            boardScript = GetComponent<BoardManager>();
            boardScript.maxLevels = MAX_LEVELS;
            Instance = this;
            DontDestroyOnLoad(gameObject);  // needed?
                                            // InitGame(); -> do it in the play state 
            SwitchState(State.MENU);
        }
        
        void Start()
        {
            pauseGameText = panelPlay.transform.GetChild(4).gameObject;
            pauseGameText.SetActive(false);
			newHighscoreText = panelVictory.transform.GetChild(0).gameObject.transform.GetChild(4).gameObject;
			newHighscoreText.SetActive(false);
            // Instance = this;
            // SwitchState(State.MENU);
        }

        void InitGame()
        {
            if (level == bossLevel)
            {
                boardScript.SetupBossScene();
            } else {
                boardScript.SetupScene(level);
            }
        }

        void Update()
        {
            switch (_state)
            {
                case State.MENU:
                    break;
                case State.INIT:
                    break;
                case State.PLAY:
                	time = Time.time - startTime;
                	timerText.text = "TIME: " + (int)(time / 60) + ":" + (int)(time % 60); 
                    if (Input.GetKeyDown(KeyCode.Space))
       				{
            			if(Time.timeScale == 0)
            			{
            				ResumeGame();
            				pauseGameText.SetActive(false);
            				Debug.Log("Game resumed.");
            			}
            			else{
            				PauseGame();
            				pauseGameText.SetActive(true);
            				Debug.Log("Game paused.");
            			}
        			}
        			
        			
                    
// !!!!!------------ CHECKS FOR COINS; UPGRADES; LOST LIFES; REACHED EXIT-> MOVED TO THE METHODS FOR THE COLLISIONS, AKA WHEN THERE IS SUCH A METHOD FROM THE GM IS CALLED-----!!!!!!
// can find the methods in the end (ActivateUpgrade(), SetExitActive() -> called from Coin script; LostLife() -> called from Player script; WhenExitReached_Update() -> called from Exit script                    
                    
                    break;
                case State.LEVELCOMPLETED:
                    break;
                case State.UPGRADE:
                    break;
                case State.LOADLEVEL:
                    break;
                case State.GAMEOVER:
                    break;
                case State.VICTORY:
                    break;    
            }
        }
        
        public void SwitchState(State newState, float delay = 0)
        {
            StartCoroutine(SwitchDelay(newState, delay));
        }

        IEnumerator SwitchDelay(State newState, float delay)
        {
            _isSwitchingState = true;
            yield return new WaitForSeconds(delay);
            EndState();
            _state = newState;
            BeginState(newState);
            _isSwitchingState = false;
        }

        private void BeginState(State newState)
        {
            switch (newState)
            {
                case State.MENU:
                    Cursor.visible = true;
                    panelMenu.SetActive(true);
                    break;
                case State.INIT:
                    Coin = 0;
                    Lifes = 5;
                    Level = 1;
                    startTime = Time.time; 
                    timerText.text = "TIME: 0:0";
                    Cursor.visible = false;
                    panelPlay.SetActive(true);
                    healthbarImage.sprite = healthbarImages[Lifes] as Sprite;	// überflüssig?
                	ResumeGame();
                    SwitchState(State.LOADLEVEL);
                    break;
                case State.PLAY:
                	time += Time.deltaTime;
                    panelPlay.SetActive(true);
					break;
                case State.LEVELCOMPLETED:
                    //exitReached = false;
                    Level++;
                    panelLevelCompleted.SetActive(true);
                    SwitchState(State.LOADLEVEL);
                    break;
                case State.LOADLEVEL:
                    // do other stuff
                    InitGame(); // set up board 		// consider: if player is every time instantiated, is there any info that should be kept with him
                    SwitchState(State.PLAY, 2f);
                    break;
                case State.UPGRADE:
                    PauseGame();    // test it tho... 
                    Cursor.visible = true;
                    panelUpgrade.SetActive(true);
                    break;
                case State.GAMEOVER:
                	PauseGame();
                    Level = 1;
                    // drop all the updates
                    Coin = 0;
                    Lifes = 6;
                    time = 0;
                    Cursor.visible = true;
                    panelGameOver.SetActive(true);
                    break;
                case State.VICTORY:
                	panelVictory.SetActive(true);
					bestTimeText.text = "Best time: " + (int)(bestTime / 60) + ":" + (int)(bestTime % 60); 
					currentTimeText.text = "TIME: " + (int)(time / 60) + ":" + (int)(time % 60); 
					if( time > bestTime)
						newHighscoreText.SetActive(true);
                	Cursor.visible = true;
                    break;   
            }
        }

        private void EndState()
        {
            switch (_state)
            {
                case State.MENU:
                    panelMenu.SetActive(false);
                    break;
                case State.INIT:
                    break;
                case State.PLAY:
                    panelPlay.SetActive(false);
                    break;
                case State.LEVELCOMPLETED:
                    break;
                case State.LOADLEVEL:
                    panelLevelCompleted.SetActive(false);
                    break;
                case State.UPGRADE:
                    Coin = 0;
                    panelUpgrade.SetActive(false);
                    //panelPlay.SetActive(true);
                    Cursor.visible = false;

                    ResumeGame();   // test it tho... 
                    break;
                case State.GAMEOVER:
                    panelGameOver.SetActive(false);
                    break;
                case State.VICTORY:
                	panelVictory.SetActive(false);
                    break;    
            }
        }

        public bool IsSwitchingStates()
        {
            return _isSwitchingState;
        }

        public void SetExitActive()
        {
			boardScript.exitTile.SetActive(true);
        }
        
        public int GetAmountCoinsForUpgrade()
        {
        	return neededForUpgradeCoins;
        }
        
        public void ActivateUpgrade()
        {
        	SwitchState(State.UPGRADE);	
        }

		public void LostLife()
		{
			Lifes--;
            if (Lifes < 0)  //  < 0 cuz the lifes are 6, but from 0 till 5 
            {
            	SwitchState(State.GAMEOVER);
            }
            else
            {
            	// maybe try with Dictionary and load from Resources 
				healthbarImage.sprite = healthbarImages[Lifes] as Sprite;
                Debug.Log("change Image healthbar!!");
			}
		}
		
		public void WhenExitReached_Update()
		{
			Debug.Log("exit reached in play");
			SwitchState(State.LEVELCOMPLETED);
		}
		
		public void RevealNemo()
        {
			boardScript.CreateNemo();
        }
        
        public void NemoWasFound()
        {
        	SwitchState(State.VICTORY);
        }
        
        private void PauseGame()
        {
            Time.timeScale = 0;
        }

        private void ResumeGame()
        {
            Time.timeScale = 1;
        }
    }

}
