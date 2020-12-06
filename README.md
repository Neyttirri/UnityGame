# Finding Nemo 

Finding Nemo is a free open source rogue-like 2D game made with Unity

## Introduction
 >  You'll be swimming as a cute tiny fish through underwater mazes. Driven by greediness you have to collect all coins to activate the exit.
	Beware of evil creatures on the way - they can't shoot, but maybe they can bite.
	The project is made for an university course "Game Engines".  

## Technologies/Prerequisites
 * programming lanuage is C# 
 * Unity - version 2019.4.11f (LTS) (64-bit) 

## Launch
 * download from git and add add in Unity (Hub)
 * check out the web version on Itch.io [here](https://lliesevzy.itch.io/finding-nemo) - password is GameEngineWS20Group4. Note - you have to play it <b>fullscreen</b> !!
	
## Features
 * everything is in one single scene - it is filled, cleared (repeat)
 * procedural level generation - the maze is generated using DFS (backtracking), the size of the maze and the amount of levels can be changed at any time (see GameManager); objects (coins and enemies)are spawned randomly in the maze; only the last (boss) level is manually created
 * transition between states of the game are handled by the GameManager; different UI panels for the states
 * player: moves with arrow keys, shoots with AWSD (for each direction), has 6 Lifes, collects coins in order to activate the exit in each level, after a certain amount of collected coins player can choose an upgrade from a) increased speed or b) shield that protects him / her from enemies for the next 10 sec. There is a limit for the speed - it can not exceed a certain float (can be set in the Player script). When player dies, all of the upgrades are lost and player begins again at level 1
 * enemies: wander around or chill until the player is close enough to them - then they start hunting. Also have lifes (have to be hit multiple times to be destroyed). The last enemy - the shark or the boss - has more lifes and can shoot a water ball, also has better senses and a personal healthbar. When the boss is destroyed, Nemo appears.
 * the whole atmosphere is kind of dimmed, the player and some objects have a bit of more light around them
 * there is a timer - no pressure, it only counts seconds and saves the best score
 * some audio accompanying you
 * GameManager is Singleton, player is also instantiated only once 
 
 

## Installation
 * OS: Windows 7 SP1+, 8, 10, 64-bit versions only; macOS 10.12+
 * CPU: SSE2 instruction set support.
 * GPU: Graphics card with DX10 (shader model 4.0) capabilities.
 
 * -> basically the ones you need to install Unity 2019 

## Contribute 
 * drop a message at s0564815@htw-berlin.de for example!  

## Credits and Sources 
Some of the sources of inspiration are:
* https://hub.packtpub.com/creating-simple-gamemanager-using-unity3d/
* https://www.coursera.org/lecture/game-development/game-manager-9TBT9 
* https://www.youtube.com/watch?v=qAf9axsyijY
* https://www.youtube.com/watch?v=9Ly7NHL0v6w&list=PLosGp2abdYXSafraOainLlvmMRfFy5H5A&index=3
* https://github.com/c00pala/Unity-2D-Maze-Generator
* looking at free Assets in the Unity Store :)  
