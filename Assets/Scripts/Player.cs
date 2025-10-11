using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public static Player Instance { get; private set; }
    
    private Rigidbody2D _rigidbody;
    public event EventHandler OnPickUpCoin;
    public event EventHandler OnEnemyHit;
    public event EventHandler<Room> OnChangingRoom;
    private void Awake()
    {
        Instance = this;
        _rigidbody = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        const float kidMoveSpeed = 7500f;
        // const float adultMoveSpeed = 5000f;
        // const float OldMoveSpeed = 2500f;
        if (Keyboard.current.upArrowKey.isPressed)
        {
            _rigidbody.AddForce(Time.deltaTime * kidMoveSpeed *  transform.up );
        }
        if (Keyboard.current.downArrowKey.isPressed)
        {
            _rigidbody.AddForce(Time.deltaTime * -kidMoveSpeed *  transform.up );
        }
        if (Keyboard.current.leftArrowKey.isPressed)
        {
            _rigidbody.AddForce(Time.deltaTime * -kidMoveSpeed *  transform.right );
        }
        if (Keyboard.current.rightArrowKey.isPressed)
        {
            _rigidbody.AddForce(Time.deltaTime * kidMoveSpeed *  transform.right );
        }
        
    }
    
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.TryGetComponent(out Coin coin))
        {        
            OnPickUpCoin?.Invoke(this, EventArgs.Empty);
            coin.SelfDestruct();
        }
        if (other.gameObject.TryGetComponent(out Door door))
        {
            var destinationDoor = door.DestinationDoor;
            var room = destinationDoor.GetComponentInParent<Room>();

            var roomCenter = room.transform.position;
            var doorPos = destinationDoor.transform.position;
            var direction = (roomCenter - doorPos).normalized;

            var offset = direction * 2f;
            transform.position = destinationDoor.transform.position + offset;

            OnChangingRoom?.Invoke(this, room);
        }

        if (other.gameObject.TryGetComponent(out Enemy enemy))
        {
            enemy.Health--;
            OnEnemyHit?.Invoke(this, EventArgs.Empty);
        }
    }
}
