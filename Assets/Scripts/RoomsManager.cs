// Assets/Scripts/RoomsManager.cs

using System.Collections.Generic;
using UnityEngine;

public class RoomsManager : MonoBehaviour
{
    [SerializeField] private float maxLinkingDistance = 5f;
    private List<Room> rooms;

    private void Awake()
    {
        rooms = new List<Room>(FindObjectsByType<Room>(FindObjectsSortMode.None));
        var allDoors = new List<Door>();
        foreach (var room in rooms)
            allDoors.AddRange(room.GetDoors());

        foreach (var d1 in allDoors)
        foreach (var d2 in allDoors)
        {
            if (d1 == d2) continue;
            if (Vector2.Distance(d1.transform.position, d2.transform.position) < maxLinkingDistance)
            {
                d1.DestinationDoor = d2;
                d2.DestinationDoor = d1;
            }
        }
    }
}