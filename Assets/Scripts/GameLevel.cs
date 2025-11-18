using UnityEngine;

public class GameLevel : MonoBehaviour
{
    [SerializeField] private int levelNumber;
    [SerializeField] private Transform startPosition;
    public int LevelNumber => levelNumber;
    public Vector3 StartPosition => startPosition.position;

    public Room GetStartRoom()
    {
        var rooms = GetComponentsInChildren<Room>();
        Room closestRoom = null;
        var minDistance = float.MaxValue;
        foreach (var room in rooms)
        {
            var dist = Vector3.Distance(room.transform.position, startPosition.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                closestRoom = room;
            }
        }

        if (!closestRoom) Debug.LogError("No room found in the level");
        return closestRoom;
    }
}