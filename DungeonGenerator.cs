using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
    // The size of the dungeon (in number of rooms)
    public int dungeonSize = 10;

    // The prefabs for the rooms and hallways
    public GameObject roomPrefab;
    public GameObject hallwayPrefab;

    // The list of rooms and hallways in the dungeon
    private List<GameObject> rooms = new List<GameObject>();
    private List<GameObject> hallways = new List<GameObject>();

    // The possible directions to move in
    private List<Vector3> directions = new List<Vector3>
    {
        Vector3.forward,
        Vector3.back,
        Vector3.left,
        Vector3.right
    };

   private void Start()
    {
        // Generate the initial room
        GenerateRoom();

        // Generate the rest of the dungeon
        for (int i = 0; i < dungeonSize; i++)
        {
            // Choose a random room to start from
            int currentRoomIndex = Random.Range(0, rooms.Count);
            GameObject currentRoom = rooms[currentRoomIndex];

            // Choose a random direction to move in
            Vector3 direction = directions[Random.Range(0, directions.Count)];

            // Check if the space in that direction is empty
            if (Physics.OverlapBox(currentRoom.transform.position + direction, Vector3.one * 0.1f).Length == 0)
            {
                // If it's empty, generate a new room there
                GenerateRoom(currentRoom.transform.position + direction);

                // Add a hallway between the current room and the new room
                GenerateHallway(currentRoom, direction);
            }
        }
    }

    void GenerateRoom(Vector3 position = default)
    {
        // If no position is provided, generate a random position
        if (position == default)
        {
            position = new Vector3(Random.Range(-5, 5), 0, Random.Range(-5, 5));
        }

        // Instantiate the room at the position
        GameObject room = Instantiate(roomPrefab, position, Quaternion.identity);

        // Add the room to the list of rooms
        rooms.Add(room);
    }

   void GenerateHallway(GameObject room, Vector3 direction)
    {
        // Calculate the position of the hallway
        Vector3 hallwayPosition = room.transform.position + direction * 0.5f;

        // Instantiate the hallway
        GameObject hallway = Instantiate(hallwayPrefab, hallwayPosition, Quaternion.identity);
        
        // Calculate the rotation of the hallway
        hallway.transform.rotation = Quaternion.LookRotation(direction);

        // Set the hallway's parent to the room
        hallway.transform.parent = room.transform;
    }
}
