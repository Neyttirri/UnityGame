using UnityEngine;
using System;
using System.Collections.Generic;       //Allows us to use Lists.
using Random = UnityEngine.Random;      //Tells Random to use the Unity Engine random number generator.

namespace Completed

{

    public class BoardManager : MonoBehaviour
    {
        public class Count
        {
            public int minimum;             //Minimum value for our Count class.
            public int maximum;             //Maximum value for our Count class.

			public Count(int min, int max)
            {
                minimum = min;
                maximum = max;
            }
        }

        [SerializeField] private int columns = 32;                                        // Number of columns in our game board
        [SerializeField] private int rows = 32;                                           // Number of rows in our game board
        [SerializeField] private Count coinCount = new Count(3, 10);                      // Lower and upper limit for our random number of food items per level
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private GameObject nemoPrefab;
        [SerializeField] private GameObject bossPrefab;
        [SerializeField] private GameObject exitPrefab;
        [SerializeField] private GameObject waterTile;                  // water prefabs
        [SerializeField] private GameObject[] innerWallTiles;           // Array of wall prefabs
        [SerializeField] private GameObject[] coinTiles;                // Array of food prefabs
        [SerializeField] private GameObject[] enemyTiles;               // Array of enemy prefabs
        [SerializeField] private GameObject[] outerWallTiles;			// Array of outer tile prefabs
        

        private Transform boardHolder;                                  // A variable to store a reference to the transform of our Board object.
        private List<Vector3> gridPositions = new List<Vector3>();      // A list of possible locations to place tiles.
        private List<Vector3> unvisited = new List<Vector3>();			// List to hold unvisited cells.
        private List<Vector3> visited = new List<Vector3>();            // will be the water tiles, since it is every second cell possible (and the one connecting two cells)
        private List<Vector3> stack = new List<Vector3>();				// List to store 'stack' cells, cells being checked during generation

        private Vector3 startCell = new Vector3(1, 1, 0f);
        private Vector3 currentCell;
        private Vector3 checkCell;

        private PlayerController playerController;
        private GameObject playerObject;								// Gonna re-use the first instantiated player prefab
        private GameObject bossObject;
        private GameObject exitTile;
        // And the boss prefab, because we were short on time for a more beautiful solution

        // helper array to calculate the next four possible cells to visit from the current cell 
        // every second cell (in steps of 2) because the walls are the same size as the "corridors", aka to make sure there's place for them - for water and wall tiles 
        private Vector3[] neighbourPositions = new Vector3[] { new Vector3(-2, 0, 0), new Vector3(2, 0, 0), new Vector3(0, 2, 0), new Vector3(0, -2, 0) };
        private int maxLevels;											// to determine which level is the last one - there will be the boss
		
		
		// ==================================================== public Methods ====================================================
		//SetupScene initializes our level and calls the previous functions to lay out the game board
        public void SetupScene(int level)
        {
            DestroyAllGameObjects();
            // Creates the outer walls and floor.
            BoardSetup();
            Debug.Log("board set up finished");
            // Reset the grid positions, visited and unvisited lists
            InitialiseList();
            Debug.Log("initialize lists finished");
            // here the magic happens
            GenerateMaze(rows, columns);
            Debug.Log("maze generated");
			// Player - only one instance created through out the game
			if (playerObject == null){
            	playerObject = Instantiate(playerPrefab);
            	playerController = playerObject.GetComponent<PlayerController>();
            } else {
            	playerObject.transform.position = new Vector3(0, 0, 0);
            }
			if(level == maxLevels)
			{
				// It is boss time! 
				//Layout water tiles
				for (int x = 0; x < columns; x++)
				{
					for (int y = 0; y < rows; y++)
					{
						GameObject water = Instantiate(waterTile, new Vector3(x, y, 0), Quaternion.identity);
						water.transform.SetParent(boardHolder);
					}
				}
                if (bossObject == null)
                {
                    bossObject = Instantiate(bossPrefab, RandomPosition(), Quaternion.identity);
                }
                else
                {
                    playerObject.transform.position = RandomPosition();
                }

                Debug.Log("Boss scene set at level: " + level);
            } else {
				LayoutWaterTiles();
				LayoutExitTile();
				LayoutInnerWalls();
				// Instantiate a random number of coins based on minimum and maximum, at randomized positions.
				LayoutObjectAtRandom(coinTiles, coinCount.minimum, coinCount.maximum);
				Debug.Log("items placed");
				// Determine number of enemies based on current level number, based on a logarithmic progression
				int enemyCount = level * 3; // or sth more fancy
				// Instantiate a random number of enemies based on minimum and maximum, at randomized positions.
				LayoutObjectAtRandom(enemyTiles, enemyCount, enemyCount);       
				Debug.Log("Scene set for level: " + level);
			}
	    }

   
		public void SetMaxLevels(int val)
		{
			maxLevels = val;
		}
		
		public void SetExitActive()
		{
			exitTile.SetActive(true);
		}

        public void DropAllPlayerUpgrades()
		{
			playerController.DropUpgrades();
		}
		
		public void UpgradePlayerShield()
		{
			playerController.UseShield();
		}
		
		public void UpgradePlayerSpeed()
		{
			playerController.UseSpeed(1);
		}
        
        public void CreateNemo()
        {
        	int randomX = Random.Range(0, rows);
        	int randomY = Random.Range(0, columns); 
        	Debug.Log("x and y: " + randomX + " " + randomY); 
        	GameObject nemo = Instantiate(nemoPrefab, new Vector3(randomX, randomY, 0), Quaternion.identity) as GameObject;
            nemo.transform.SetParent(boardHolder);
        	Debug.Log("nemo at: " + nemo.transform.position);
        }
		// ==================================================== private Methods ====================================================
        //Clears our list gridPositions and prepares it to generate a new board
        private void InitialiseList()
        {
            //Clear our list gridPositions.
            gridPositions.Clear();
            unvisited.Clear();
            visited.Clear();
            stack.Clear();

            //Loop through x axis (columns).
            for (int x = 1; x < columns - 1; x++)
            {
                //Within each column, loop through y axis (rows).
                for (int y = 1; y < rows - 1; y++)
                {
                    //At each index add a new Vector3 to our list with the x and y coordinates of that position.
                    gridPositions.Add(new Vector3(x, y, 0f));
                    if (x % 2 == 1 && y % 2 == 1)
                        unvisited.Add(new Vector3(x, y, 0f));
                }
            }
            Debug.Log("grid: " + gridPositions.Count + "unvisited: " + unvisited.Count);
            unvisited.Remove(startCell);
            currentCell = startCell;
        }

        // Sets up the outer walls of the game board.
        private void BoardSetup()
        {
            // Instantiate Board and set boardHolder to its transform.
            boardHolder = new GameObject("Board").transform;

            // Loop along x axis, starting from -1 (to fill corner) with floor or outerwall edge tiles.
            for (int x = -1; x <= columns; x++)
            {
                // Loop along y axis, starting from -1 to place floor or outerwall tiles.
                for (int y = -1; y <= rows; y++)
                {

                    // Check if we current position is at board edge, if so choose a random outer wall prefab from our array of outer wall tiles.
                    if (x == -1 || x == columns || y == -1 || y == rows)
                    {
                        GameObject toInstantiate = outerWallTiles[Random.Range(0, outerWallTiles.Length)];
                        // Instantiate the GameObject instance using the prefab chosen for toInstantiate at the Vector3 corresponding to current grid position in loop, cast it to GameObject.
                        GameObject instance = Instantiate(toInstantiate, new Vector3(x, y, 0f), Quaternion.identity) as GameObject;
                        // Set the parent of our newly instantiated object instance to boardHolder, this is just organizational to avoid cluttering hierarchy.
                        instance.transform.SetParent(boardHolder);
                    }
                }
            }
        }


        // RandomPosition returns a random position from our list visited - to place an enemy or an item on it 
        private Vector3 RandomPosition()
        {
            // Declare an integer randomIndex, set it's value to a random number between 0 and the count of items in the List
            int randomIndex = Random.Range(0, visited.Count - 1);       // visited.Count - 1 : to exclude the exit tile, which is always the last one

            // Declare a variable of type Vector3 called randomPosition, set it's value to the entry at randomIndex from the List
            Vector3 randomPosition = visited[randomIndex];

            // Remove the entry at randomIndex from the list so that it can't be re-used.
            visited.RemoveAt(randomIndex);
            return randomPosition;
        }


        // LayoutObjectAtRandom accepts an array of game objects to choose from along with a minimum and maximum range for the number of objects to create.
        // used to layout items or enemies 
        private void LayoutObjectAtRandom(GameObject[] tileArray, int minimum, int maximum)
        {
            //Choose a random number of objects to instantiate within the minimum and maximum limits
            int objectCount = Random.Range(minimum, maximum + 1);
            //Instantiate objects until the randomly chosen limit objectCount is reached
            for (int i = 0; i < objectCount; i++)
            {
                Vector3 randomPosition = RandomPosition();
                //Choose a random tile from tileArray and assign it to tileChoice
                GameObject tileChoice = tileArray[Random.Range(0, tileArray.Length)];
                //Instantiate tileChoice at the position returned by RandomPosition with no change in rotation
                GameObject created = Instantiate(tileChoice, randomPosition, Quaternion.identity);
                created.transform.SetParent(boardHolder);
            }
        }

		// Can comment out the logs - were for debugging! (surprise)
        private void GenerateMaze(int rows, int cols)
        {
            // While we have unvisited cells.
            while (unvisited.Count > 0)
            {
                List<Vector3> unvisitedNeighbours = GetUnvisitedNeighbours(currentCell);
                // Debug.Log("unvisitedNeighbours: " + unvisitedNeighbours.Count);
                // Debug.Log("unvisited size: " + unvisited.Count);
                if (unvisitedNeighbours.Count > 0)
                {
                    // Get a random unvisited neighbour.
                    checkCell = unvisitedNeighbours[Random.Range(0, unvisitedNeighbours.Count)];
                    // Add current cell to stack.
                    stack.Add(currentCell);
                    // Debug.Log("add to stack: " + currentCell);
                    visited.Add(currentCell);
                    visited.Add(new Vector3(currentCell.x + (checkCell.x - currentCell.x) / 2, currentCell.y + (checkCell.y - currentCell.y) / 2, 0));
                    // Make currentCell the neighbour cell.
                    currentCell = checkCell;
                    // Debug.Log("next cell:" + checkCell);
                    // Mark new current cell as visited.
                    unvisited.Remove(currentCell);
                }
                else if (stack.Count > 0)
                {
                    // Make current cell the most recently added Cell from the stack.
                    currentCell = stack[stack.Count - 1];
                    // Remove it from stack.
                    stack.Remove(currentCell);
                    // Debug.Log("remove from stack: " + currentCell);
                }
            }
        }

        private List<Vector3> GetUnvisitedNeighbours(Vector3 curCell)
        {
            List<Vector3> neighbours = new List<Vector3>();
            Vector3 nCell = curCell;
            foreach (Vector3 p in neighbourPositions)
            {
                // Find position of neighbour on grid, relative to current.
                Vector3 nPos = new Vector3(nCell.x + p.x, nCell.y + p.y, 0);
                // If cell is unvisited - if it doesn't exist in the grid, it also won't be in the unvisited list 
                if (unvisited.Contains(nPos)) neighbours.Add(nPos);
            }
            return neighbours;
        }

        private void LayoutWaterTiles()
        {
            for (int i = 0; i < visited.Count; i++)
            {
                Vector3 position = visited[i];
                GameObject water = Instantiate(waterTile, position, Quaternion.identity);
                water.transform.SetParent(boardHolder);
            }
            // just for the pic and until we figure out the doors and all the details, optimize it later!!!!
            for (int x = 0; x < columns; x++)
            {
                for (int y = 0; y < rows; y++)
                {

                    //Check if we current position is at board edge, if so choose a random outer wall prefab from our array of outer wall tiles.
                    if (x == 0 || x == columns - 1 || y == 0 || y == rows - 1)
                    {
                        GameObject water = Instantiate(waterTile, new Vector3(x, y, 0), Quaternion.identity);
                        water.transform.SetParent(boardHolder);
                    }
                }
            }
        }
 
        private void LayoutExitTile()
        {
            Debug.Log("exit at: " + visited[visited.Count - 1]);

            exitTile = Instantiate(exitPrefab, visited[visited.Count - 1], Quaternion.identity);
            exitTile.transform.SetParent(boardHolder);
            exitTile.SetActive(false);  // will be active only when a condition is fulfilled (all coins were collected)
        }

        private void LayoutInnerWalls()
        {
            GameObject sth = new GameObject();
            foreach (Vector3 vec in gridPositions)
            {
                if (!visited.Contains(vec))
                    sth = Instantiate(innerWallTiles[Random.Range(0, innerWallTiles.Length)], vec, Quaternion.identity);
                sth.transform.SetParent(boardHolder);
            }
        }

        private void DestroyAllGameObjects()
        {
            GameObject[] GameObjects = (FindObjectsOfType<GameObject>() as GameObject[]);

            for (int i = 0; i < GameObjects.Length; i++)
            {
                if (GameObjects[i].tag == "Wall" || GameObjects[i].tag == "Water" || GameObjects[i].tag == "Coin" || GameObjects[i].tag == "Nemo" || GameObjects[i].tag == "Enemy"|| GameObjects[i].tag == "Exit"  )
                    Destroy(GameObjects[i]);
            }
        }
	}
}