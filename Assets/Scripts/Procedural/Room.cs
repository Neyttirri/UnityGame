﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Security.Cryptography;
using UnityEngine;

public class Room : MonoBehaviour
{
    public int Width;
    public int Height;
    public int X;
    public int Y;
    public Door leftDoor;
    public Door rightDoor;
    public Door bottomDoor;
    public Door topDoor;
    public List<Door> doors = new List<Door>();
    // Start is called before the first frame update
    void Start()
    {
        if (RoomController.instance == null) 
        {
            UnityEngine.Debug.Log("You play the wrong scene");
            return;

        }
        Door[] ds = GetComponentsInChildren<Door>();
        foreach (Door d in ds) 
        {
            doors.Add(d);
            switch (d.doorType) 
            { case Door.DoorType.right:
                    rightDoor = d;
                    break;
                case Door.DoorType.left:
                    leftDoor = d;
                    break;
                case Door.DoorType.top:
                    topDoor = d;
                    break;
                case Door.DoorType.bottom:
                    bottomDoor = d;
                    break;
            }
        }

            

        RoomController.instance.RegisterRoom(this);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, new Vector3(Width, Height, 0));
    }
    public Vector3 GetRoomCenter() 
    {
        return new Vector3(X * Width, Y * Height);
    }
    void OnTriggerEnter2D(Collider2D other) 
    {
        if (other.tag == "Player") 
        { RoomController.instance.OnPlayerEnterRoom(this); }
    }
    public void RemoveUnconnectedDoors() 
    {
        foreach (Door door in doors) 
        {
            switch (door.doorType) 
            {
                case Door.DoorType.right:
                    if (GetRight() == null) { door.gameObject.SetActive(false); }
                    break;
                case Door.DoorType.left:
                    if (GetLeft() == null) { door.gameObject.SetActive(false); }
                    break;
                case Door.DoorType.top:
                    if (GetTop() == null) { door.gameObject.SetActive(false); }
                    break;
                case Door.DoorType.bottom:
                    if (GetBottom() == null) { door.gameObject.SetActive(false); }
                    break;
            }
        }
    }

    public Room GetRight()
    {
        if (RoomController.instance.DoesRoomExist(X + 1, Y)) 
        {
            return RoomController.instance.FindRoom(X + 1, Y);
        }
        return null;
    }
    public Room GetTop()
    {
        if (RoomController.instance.DoesRoomExist(X, Y+1))
        {
            return RoomController.instance.FindRoom(X, Y+1);
        }
        return null;
    }
    public Room GetLeft()
    {
        if (RoomController.instance.DoesRoomExist(X - 1, Y))
        {
            return RoomController.instance.FindRoom(X - 1, Y);
        }
        return null;
    }
    public Room GetBottom()
    {
        if (RoomController.instance.DoesRoomExist(X , Y-1))
        {
            return RoomController.instance.FindRoom(X , Y-1);
        }
        return null;
    }
}