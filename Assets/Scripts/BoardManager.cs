using UnityEngine;
using System;
using System.Collections.Generic;       //Allows us to use Lists.
using Random = UnityEngine.Random;      //Tells Random to use the Unity Engine random number generator.

namespace Completed

{

    public class BoardManager : MonoBehaviour
    {
        // Using Serializable allows us to embed a class with sub properties in the inspector.
        [Serializable]
        public class Count
        {
            public int minimum;             //Minimum value for our Count class.
            public int maximum;             //Maximum value for our Count class.


            //Assignment constructor.
            public Count(int min, int max)
            {
                minimum = min;
                maximum = max;
            }
        }

        public int columns = 32;                                         //Number of columns in our game board.
        public int rows = 32;                                            //Number of rows in our game board.
        public Count foodCount = new Count(3, 10);                      //Lower and upper limit for our random number of food items per level.

        public GameObject playerPrefab;
        public GameObject nemoPrefab;
        public GameObject bossPrefab;
        public GameObject exitTile;
        //Prefab to spawn for exit.
        // if we do it with background image, there is no need of floor tiles, no one water tile is added 
        public GameObject waterTile;                                 	//water prefabs.
        public GameObject[] innerWallTiles;                             //Array of wall prefabs.
        public GameObject[] foodTiles;                                  //Array of food prefabs.
        public GameObject[] enemyTiles;                                 //Array of enemy prefabs.
        public GameObject[] outerWallTiles;
        //Array of outer tile prefabs.

        private Transform boardHolder;                                  //A variable to store a reference to the transform of our Board object.
        private List<Vector3> gridPositions = new List<Vector3>();   //A list of possible locations to place tiles.
                                                                     // List to hold unvisited cells.
        private List<Vector3> unvisited = new List<Vector3>();
        private List<Vector3> visited = new List<Vector3>();            // will be the water tiles, since it is every second cell possible (and the one connecting two cells)
                                                                        // List to store 'stack' cells, cells being checked during generation.
        private List<Vector3> stack = new List<Vector3>();

        private Vector3 startCell = new Vector3(1, 1, 0f);
        private Vector3 currentCell;
        private Vector3 checkCell;

        private PlayerController playerController;
        private GameObject playerObject;

        // every second cell to cuz for now the walls are the same size as the "corridors", aka to make sure there's place for them 
        // another option would be to make different 2D objects with walls (BoxCollider only on the surface of the wall) etc... 

        private Vector3[] neighbourPositions = new Vector3[] { new Vector3(-2, 0, 0), new Vector3(2, 0, 0), new Vector3(0, 2, 0), new Vector3(0, -2, 0) };

        [HideInInspector] public int maxLevels;
        //Clears our list gridPositions and prepares it to generate a new board.
        void InitialiseList()
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


        //Sets up the outer walls of the game board.
        void BoardSetup()
        {
            //Instantiate Board and set boardHolder to its transform.
            boardHolder = new GameObject("Board").transform;

            //Loop along x axis, starting from -1 (to fill corner) with floor or outerwall edge tiles.
            for (int x = -1; x <= columns; x++)
            {
                //Loop along y axis, starting from -1 to place floor or outerwall tiles.
                for (int y = -1; y <= rows; y++)
                {

                    //Check if we current position is at board edge, if so choose a random outer wall prefab from our array of outer wall tiles.
                    if (x == -1 || x == columns || y == -1 || y == rows)
                    {
                        GameObject toInstantiate = outerWallTiles[Random.Range(0, outerWallTiles.Length)];
                        //Instantiate the GameObject instance using the prefab chosen for toInstantiate at the Vector3 corresponding to current grid position in loop, cast it to GameObject.
                        GameObject instance =
                       Instantiate(toInstantiate, new Vector3(x, y, 0f), Quaternion.identity) as GameObject;
                        //Set the parent of our newly instantiated object instance to boardHolder, this is just organizational to avoid cluttering hierarchy.
                        instance.transform.SetParent(boardHolder);
                    }
                }
            }
        }


        //RandomPosition returns a random position from our list visited - to place an enemy or an item on it 
        Vector3 RandomPosition()
        {
            //Declare an integer randomIndex, set it's value to a random number between 0 and the count of items in the List
            int randomIndex = Random.Range(0, visited.Count - 1);       // visited.Count - 1 : to exclude the exit tile, which is always the last one

            //Declare a variable of type Vector3 called randomPosition, set it's value to the entry at randomIndex from the List
            Vector3 randomPosition = visited[randomIndex];

            //Remove the entry at randomIndex from the list so that it can't be re-used.
            visited.RemoveAt(randomIndex);
            return randomPosition;
        }


        //LayoutObjectAtRandom accepts an array of game objects to choose from along with a minimum and maximum range for the number of objects to create.
        // used to layout items or enemies 
        void LayoutObjectAtRandom(GameObject[] tileArray, int minimum, int maximum)
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


        //SetupScene initializes our level and calls the previous functions to lay out the game board
        public void SetupScene(int level)
        {
            DestroyAllGameObjects();
            //Creates the outer walls and floor.
            BoardSetup();
            Debug.Log("board set up finished");
            //Reset the grid positions, visited and unvisited lists
            InitialiseList();
            Debug.Log("initialize lists finished");
            // here the magic happens
            GenerateMaze(rows, columns);
            Debug.Log("maze generated");
            LayoutWaterTiles();
            LayoutExitTile(level);
            LayoutInnerWalls();
            if (playerObject == null){
            	playerObject = Instantiate(playerPrefab);
            	playerController = playerObject.GetComponent<PlayerController>();
            } else {
            	playerObject.transform.position = new Vector3(0, 0, 0);
            }
            //Instantiate a random number of food tiles based on minimum and maximum, at randomized positions.
            LayoutObjectAtRandom(foodTiles, foodCount.minimum, foodCount.maximum);
            Debug.Log("items placed");
            //Determine number of enemies based on current level number, based on a logarithmic progression
            int enemyCount = level+5; // or sth more fancy

            //Instantiate a random number of enemies based on minimum and maximum, at randomized positions.
            LayoutObjectAtRandom(enemyTiles, enemyCount, enemyCount);       // here could be a bug with out of range, check wenn so weit NB!!!!
            Debug.Log("enemies placed");
        }

        public void SetupBossScene()
        {
            DestroyAllGameObjects();
            //Creates the outer walls and floor.
            BoardSetup();
            //Reset the grid positions, visited and unvisited lists
            InitialiseList();
            GenerateMaze(rows, columns);
            //Layout water tiles
            for (int x = 0; x < columns; x++)
            {
                for (int y = 0; y < rows; y++)
                {
                    GameObject water = Instantiate(waterTile, new Vector3(x, y, 0), Quaternion.identity);
                    water.transform.SetParent(boardHolder);
                }
            }
            if (playerObject == null){
            	playerObject = Instantiate(playerPrefab);
            	playerController = playerObject.GetComponent<PlayerController>();
            } else {
            	playerObject.transform.position = new Vector3(0, 0, 0);
            }
            GameObject boss = Instantiate(bossPrefab, RandomPosition(), Quaternion.identity);
            boss.transform.SetParent(boardHolder);
        }

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
            // put also the exit (for now i am gonna leave the extra method below this one )
            //if( i == visited.Count - 1)
            //	Instantiate(exitTile, visited[visited.Count - 1], Quaternion.identity);


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

        private void LayoutExitTile(int level)
        {
            if (level == maxLevels)
            {
                // put Nemo in the end instead of an exit 
                // maybe also some particles around him, if you feel like it 
            }
            else
            {
                Debug.Log("exit at: " + visited[visited.Count - 1]);
                exitTile = Instantiate(exitTile, visited[visited.Count - 1], Quaternion.identity);
                exitTile.transform.SetParent(boardHolder);
                exitTile.SetActive(false);  // will be active only when a condition is fulfilled
            }
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
                if (GameObjects[i].tag == "Wall" || GameObjects[i].tag == "Water" || GameObjects[i].tag == "Coin" || GameObjects[i].tag == "Exit"  )
                    Destroy(GameObjects[i]);
            }
        }

        public PlayerController GetPlayerController()
        {
            return playerController;
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
    }
}