// Assets/Scripts/RoomsManager.cs
using System.Collections.Generic;
using UnityEngine;

public class RoomsManager : MonoBehaviour
{
    private List<Room> rooms;
    [SerializeField] private float proximityThreshold = 0.5f;

    private void Awake()
    {
        rooms = new List<Room>(Object.FindObjectsByType<Room>(FindObjectsSortMode.None));
        var allDoors = new List<Door>();
        foreach (var room in rooms)
            allDoors.AddRange(room.GetDoors());

        foreach (var d1 in allDoors)
        {
            foreach (var d2 in allDoors)
            {
                if (d1 == d2) continue;
                if (Vector2.Distance(d1.transform.position, d2.transform.position) < proximityThreshold)
                {
                    d1.DestinationDoor = d2;
                    d2.DestinationDoor = d1;
                }
            }
        }
    }
}