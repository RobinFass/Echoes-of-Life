using UnityEngine;

public class Door : MonoBehaviour
{
    private Door _door;

    public Door DestinationDoor
    {
        get => _door;
        set
        {
            var destinationDoor = value;
            if (destinationDoor != null) _door = destinationDoor;
        }
    }
}