using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    [SerializeField] private Door topDoor;
    [SerializeField] private Door rightDoor;
    [SerializeField] private Door bottomDoor;
    [SerializeField] private Door leftDoor;
    [SerializeField] private CameraBounds cameraBounds;

    private void Awake()
    {
        topDoor.GetComponent<SpriteRenderer>().enabled = topDoor != null;
        topDoor.GetComponent<SpriteRenderer>().enabled = rightDoor != null;
        topDoor.GetComponent<SpriteRenderer>().enabled = bottomDoor != null;
        topDoor.GetComponent<SpriteRenderer>().enabled = leftDoor != null;
    }
    
    public List<Door> GetDoors()
    {
        var doorsList = new List<Door>();
        if (topDoor != null) doorsList.Add(topDoor);
        if (rightDoor != null) doorsList.Add(rightDoor);
        if (bottomDoor != null) doorsList.Add(bottomDoor);
        if (leftDoor != null) doorsList.Add(leftDoor);
        return doorsList;
    }

    public CameraBounds GetCameraBounds()
    {
        return cameraBounds;
    }
}
