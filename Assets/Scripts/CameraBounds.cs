using UnityEngine;

public class CameraBounds : MonoBehaviour
{
    public Collider2D GetBounds()
    {
        return GetComponent<Collider2D>();
    }
}
