using System;
using Object;
using Unity.VisualScripting;
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
    
    public event EventHandler<Enemy> OnEnemyHit;
    public event EventHandler<Room> OnChangingRoom;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        Stats.OnDeath += Stats_OnDeath;
        // subscribe to attack event so we can play sword SFX
        if (PlayerAttack.Instance != null)
            PlayerAttack.Instance.OnAttack += PlayerAttack_OnAttack;
    }
    
    private void OnCollisionStay2D(Collision2D other)
    {
        if(Stats.HurtCooldownTime > 0) return;
        
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

                AudioManager.Instance?.PlaySfx("room"); // play room-enter sound

                OnChangingRoom?.Invoke(this, room);
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

    // called whenever PlayerAttack triggers an attack
    private void PlayerAttack_OnAttack(object sender, EventArgs e)
    {
        AudioManager.Instance?.PlaySfx("sword");
    }

    public event EventHandler<Enemy> OnEnemyHit;
    public event EventHandler<Room> OnChangingRoom;
    public event EventHandler OnPlayerWin;

    private void Stats_OnDeath(object sender, EventArgs eventArgs)
    {
        gameManager.State = GameState.Dead;
        AudioManager.Instance?.StopLoopingSfx(); // stop walk/run when player dies
    }

    public void ChangeSprite(int level)
    {
        spriteLibrary.spriteLibraryAsset = spriteLibraryAsset[level];
    }
}