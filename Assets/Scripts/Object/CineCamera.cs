using Unity.Cinemachine;
using UnityEngine;

public class CineCamera : MonoBehaviour
{
    private CinemachineConfiner2D _confiner;
    public static CineCamera Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        _confiner = GetComponent<CinemachineConfiner2D>();
    }

    public void SetCameraBounds(Collider2D bounds)
    {
        _confiner.BoundingShape2D = bounds;
    }
}