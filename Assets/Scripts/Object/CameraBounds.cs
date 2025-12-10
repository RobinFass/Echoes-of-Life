using UnityEngine;

namespace Object
{
    public class CameraBounds : MonoBehaviour
    {
        public Collider2D GetBounds()
        {
            return GetComponent<Collider2D>();
        }
    }
}
