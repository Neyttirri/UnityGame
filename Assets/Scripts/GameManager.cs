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
		[SerializeField] private int bossLevel;
        
        private GameObject pauseGameText;
		private GameObject newHighscoreText;
        
        public static GameManager Instance { get; private set; }
        public enum State { MENU, INIT, PLAY, LEVELCOMPLETED, LOADLEVEL, UPGRADE, GAMEOVER, VICTORY }
        
        private State _state;
        private bool _isSwitchingState;
		private float elapsedTime;
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
			boardScript.UpgradePlayerSpeed();
            SwitchState(State.PLAY);
        }

        public void ButtonShieldClicked()
        {
            Debug.Log("shield chosen");
            boardScript.UpgradePlayerShield();
            SwitchState(State.PLAY);
        }

        void Awake()
        {
            boardScript = GetComponent<BoardManager>();
            boardScript.SetMaxLevels(bossLevel);
            Instance = this;
            DontDestroyOnLoad(gameObject);  // needed?
            SwitchState(State.MENU);
        }
        
        void Start()
        {
            pauseGameText = panelPlay.transform.GetChild(4).gameObject;
            pauseGameText.SetActive(false);
			newHighscoreText = panelVictory.transform.GetChild(0).gameObject.transform.GetChild(4).gameObject;
			newHighscoreText.SetActive(false);
        }

        void InitGame()
        {
            boardScript.SetupScene(level);
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
                	elapsedTime = Time.time - startTime;
                	timerText.text = "TIME: " + (int)(elapsedTime / 60) + ":" + (int)(elapsedTime % 60); 
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
                	elapsedTime += Time.deltaTime;
                    panelPlay.SetActive(true);
					break;
                case State.LEVELCOMPLETED:
                    Level++;
                    panelLevelCompleted.SetActive(true);
                    SwitchState(State.LOADLEVEL);
                    break;
                case State.LOADLEVEL:
                    InitGame(); // set up board 		
                    SwitchState(State.PLAY, 2f);
                    break;
                case State.UPGRADE:
                    PauseGame();    
                    Cursor.visible = true;
                    panelUpgrade.SetActive(true);
                    break;
                case State.GAMEOVER:
                	PauseGame();
                    Level = 1;
                    Coin = 0;
                    Lifes = 6;
                    elapsedTime = 0;
					boardScript.DropAllPlayerUpgrades();		// make sure when starting the game (also next times), all the upgrades for the player are gone
                    Cursor.visible = true;
                    panelGameOver.SetActive(true);
                    break;
                case State.VICTORY:
                	panelVictory.SetActive(true);
					bestTimeText.text = "Best time: " + (int)(bestTime / 60) + ":" + (int)(bestTime % 60); 
					currentTimeText.text = "TIME: " + (int)(elapsedTime / 60) + ":" + (int)(elapsedTime % 60); 
					if( elapsedTime > bestTime)
						newHighscoreText.SetActive(true);
                	Cursor.visible = true;
					boardScript.DropAllPlayerUpgrades();		// make sure when starting the game (also next times), all the upgrades for the player are gone
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
                    Cursor.visible = false;
					ResumeGame();   
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
			boardScript.SetExitActive();
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
