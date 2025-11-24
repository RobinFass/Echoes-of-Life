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

    public void ChangeSprite(int level)
    {
        spriteLibrary.spriteLibraryAsset = spriteLibraryAsset[level];
    }
}