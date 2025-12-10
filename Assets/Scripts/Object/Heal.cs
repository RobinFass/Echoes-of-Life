using UnityEngine;

namespace Object
{
    public class Heal : MonoBehaviour
    {
        public void SelfDestruct()
        {
            Destroy(gameObject);
        }
    }
}
