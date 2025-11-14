using System;
using UnityEngine;
using UnityEngine.U2D.Animation;

public class Player : MonoBehaviour
{
    [SerializeField] private SpriteLibrary spriteLibrary;
    [SerializeField] private SpriteLibraryAsset[] spriteLibraryAsset;
    public static Player Instance { get; private set; }

    public PlayerAttack Attack => PlayerAttack.Instance;
    public PlayerMovement Movement => PlayerMovement.Instance;
    public PlayerStats Stats => PlayerStats.Instance;
    public PlayerAnimation Animation => PlayerAnimation.Instance;

    private GameManager gameManager => GameManager.Instance;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        Stats.OnDeath += Stats_OnDeath;
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

    private void OnTriggerEnter2D(Collider2D other)
    {
        other.gameObject.TryGetComponent(out MonoBehaviour obj);
        switch (obj)
        {
            case Door door:
            {
                var destinationDoor = door.DestinationDoor;
                var room = destinationDoor.GetComponentInParent<Room>();

                var roomCenter = room.transform.position;
                var doorPos = destinationDoor.transform.position;
                var direction = (roomCenter - doorPos).normalized;

                var offset = direction * 3f;
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
            case Heal heal:
            {
                Stats.Heal(7);
                heal.SelfDestruct();
                break;
            }
        }
    }

    public event EventHandler<Enemy> OnEnemyHit;
    public event EventHandler<Room> OnChangingRoom;
    public event EventHandler OnPlayerWin;

    private void Stats_OnDeath(object sender, EventArgs eventArgs)
    {
        gameManager.State = GameState.Dead;

    }

    public void ChangeSprite(int level)
    {
        spriteLibrary.spriteLibraryAsset = spriteLibraryAsset[level];
    }
}