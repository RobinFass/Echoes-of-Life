using System;
using Common.Player;
using UnityEngine;
using UnityEngine.U2D.Animation;

namespace Object
{
    public class Player : MonoBehaviour
    {
        [SerializeField] private SpriteLibrary spriteLibrary;
        [SerializeField] private SpriteLibraryAsset[] spriteLibraryAsset;

        public static Player Instance { get; private set; }

        public static PlayerAttack Attack => PlayerAttack.Instance;
        public static PlayerStats Stats => PlayerStats.Instance;
        public static PlayerAnimation Animation => PlayerAnimation.Instance;

        public event EventHandler<Enemy> OnEnemyHit;
        public event EventHandler<Room> OnChangingRoom;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            if (PlayerAttack.Instance != null)
                PlayerAttack.Instance.OnAttack += PlayerAttack_OnAttack;
            AudioManager.Instance?.PlaySfx("spawn");
        }

        private void OnCollisionStay2D(Collision2D other)
        {
            if (Stats.HurtCooldownTime > 0) return;
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
                    AudioManager.Instance?.PlaySfx("room");
                    if (destinationDoor.GetComponent<Common.BossDoor>() != null)
                    {
                        AudioManager.Instance?.StopMusic();
                        AudioManager.Instance?.PlayBossMusic(GameManager.LevelNumber);
                    }

                    OnChangingRoom?.Invoke(this, room);
                    break;
                }
                case Heal heal:
                {
                    AudioManager.Instance?.PlaySfx("heal");
                    Stats.Heal(7);
                    heal.SelfDestruct();
                    break;
                }
                case Enemy enemy:
                {
                    OnEnemyHit?.Invoke(this, enemy);
                    break;
                }
            }
        }

        public void BeingHit(Enemy enemy)
        {
            OnEnemyHit?.Invoke(this, enemy);
        }

        private static void PlayerAttack_OnAttack(object sender, EventArgs e)
        {
            AudioManager.Instance?.PlaySfx("sword");
        }
    }
}
