using Unity.Cinemachine;
using UnityEngine;

namespace Object
{
    public class CineCamera : MonoBehaviour
    {
        private CinemachineConfiner2D confiner;
        
        public static CineCamera Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
            confiner = GetComponent<CinemachineConfiner2D>();
        }

        public void SetCameraBounds(Collider2D bounds)
        {
            confiner.BoundingShape2D = bounds;
        }
    }
}
