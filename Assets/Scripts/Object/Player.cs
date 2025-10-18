using System;
using Unity.VisualScripting;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player Instance { get; private set; }

    public PlayerAttack Attack => PlayerAttack.Instance;
    public PlayerMovement Movement => PlayerMovement.Instance;
    public PlayerStats Stats => PlayerStats.Instance;
    public PlayerAnimation PlayerAnimation => PlayerAnimation.Instance;
    
    private GameManager gameManager => GameManager.Instance;
    
    public event EventHandler OnPickUpCoin;
    public event EventHandler<Enemy> OnEnemyHit;
    public event EventHandler<Room> OnChangingRoom;
    public event EventHandler OnPlayerWin;
    
    private void Awake()
    {
        Instance = this;
    }
    
    private void Start()
    {
        Stats.OnDeath += Stats_OnDeath;
    }

    private void Stats_OnDeath(object sender, EventArgs eventArgs)
    {
        gameObject.SetActive(false);
    }

    
    private void OnTriggerEnter2D(Collider2D other)
    {
        other.gameObject.TryGetComponent(out MonoBehaviour obj);
        switch (obj)
        {
            case Coin coin:
            {
                OnPickUpCoin?.Invoke(this, EventArgs.Empty);
                coin.SelfDestruct();
                break;
            }
            case Door door:
            {
                var destinationDoor = door.DestinationDoor;
                var room = destinationDoor.GetComponentInParent<Room>();

                var roomCenter = room.transform.position;
                var doorPos = destinationDoor.transform.position;
                var direction = (roomCenter - doorPos).normalized;

                var offset = direction * 2f;
                transform.position = destinationDoor.transform.position + offset;

                OnChangingRoom?.Invoke(this, room);
                break;
            }
            case End _:
            {
                OnPlayerWin?.Invoke(this, EventArgs.Empty);
                gameManager.State = GameState.Won;
                break;
            }
        }
    }
    
    private void OnCollisionEnter2D(Collision2D other)
    {
        other.gameObject.TryGetComponent(out MonoBehaviour obj);
        switch (obj)
        {
            case Enemy enemy:
            {
                OnEnemyHit?.Invoke(this, enemy);
                break;
            }
        }
    }
}