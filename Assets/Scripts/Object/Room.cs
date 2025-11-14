using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    [SerializeField] private Door topDoor;
    [SerializeField] private Door rightDoor;
    [SerializeField] private Door bottomDoor;
    [SerializeField] private Door leftDoor;
    [SerializeField] private CameraBounds cameraBounds;

    private void Start()
    {
        topDoor.gameObject.SetActive(topDoor.DestinationDoor != null);
        rightDoor.gameObject.SetActive(rightDoor.DestinationDoor != null);
        bottomDoor.gameObject.SetActive(bottomDoor.DestinationDoor != null);
        leftDoor.gameObject.SetActive(leftDoor.DestinationDoor != null);
    }

    public List<Door> GetDoors()
    {
        return new List<Door> { topDoor, rightDoor, bottomDoor, leftDoor };
    }

    public CameraBounds GetCameraBounds()
    {
        return cameraBounds;
    }
}