using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public static Player Instance { get; private set; }
    
    [SerializeField] private float moveSpeed = 5000f;
    
    [SerializeField] private float sprintMultiplier = 1.5f;
    
    [SerializeField] private float dashDistance = 5f;
    [SerializeField] private float dashDuration = 0.1f;
    [SerializeField] private float dashCooldown = 3f;
    
    [SerializeField] private float maxStamina = 10f;
    [SerializeField] private float staminaRegenRate = 1f;
    [SerializeField] private float staminaRegenCooldown = 2f;
    [SerializeField] private float sprintStaminaCost = 2f;
    [SerializeField] private float dashStaminaCost = 4f;
    
    private Rigidbody2D _rigidbody;
    private bool isSprinting = false;
    private float dashCooldownTimer = 0f;
    private float staminaRegenCooldownTimer = 0f;
    private Vector2 dashDirection;
    private Vector2 moveInput;
    private Vector2 lastMoveInput = new Vector2(1f, 0f);
    private float stamina;

    public event EventHandler OnPickUpCoin;
    public event EventHandler OnEnemyHit;
    public event EventHandler<Room> OnChangingRoom;
    
    public float GetStaminaNormalized()
    {
        return stamina / maxStamina;
    }
    private void Awake()
    {
        Instance = this;
        stamina = maxStamina;
        _rigidbody = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        GameManager.Instance.OnPlayerDeath += GameManager_OnPlayerDeath;
    }

    private void GameManager_OnPlayerDeath(object sender, EventArgs e)
    {
        gameObject.SetActive(false);
        moveSpeed = 0;
    }

    private void FixedUpdate()
    {
        var speed = moveSpeed;

        if (isSprinting && stamina <= 0)
        {            
            isSprinting = false;
            staminaRegenCooldownTimer = staminaRegenCooldown;
        }
        if (isSprinting && stamina > 0)
        {
            stamina -= sprintStaminaCost * Time.fixedDeltaTime;
            staminaRegenCooldownTimer = staminaRegenCooldown;
            speed = moveSpeed * sprintMultiplier;
        }
        else if (staminaRegenCooldownTimer <= 0f)
        {
            stamina = Mathf.Min(maxStamina, stamina + staminaRegenRate * Time.fixedDeltaTime);
        }
        
        _rigidbody.AddForce(Time.fixedDeltaTime * speed * moveInput);

        if (dashCooldownTimer > 0f)
            dashCooldownTimer -= Time.fixedDeltaTime;
        
        if (staminaRegenCooldownTimer > 0f)
            staminaRegenCooldownTimer -= Time.fixedDeltaTime;

        stamina = Mathf.Clamp(stamina, 0, maxStamina);
    }

    public void OnMove(InputValue value)
    {
        if (value.Get<Vector2>() == Vector2.zero)
        {
            lastMoveInput = moveInput;
        }
        moveInput = value.Get<Vector2>();
    }
    
    public void OnSprint(InputValue value)
    {
        isSprinting = value.isPressed;
    }

    public void OnDash()
    {
        if (stamina >= dashStaminaCost)
        {
            stamina -= dashStaminaCost;
            staminaRegenCooldownTimer = staminaRegenCooldown;
            StartCoroutine(DashCoroutine(lastMoveInput.normalized, dashDistance));
        }
    }

    private IEnumerator DashCoroutine(Vector2 direction, float distance)
    {
        float elapsed = 0f;
        float dashSpeed = distance / dashDuration;
        while (elapsed < dashDuration)
        {
            _rigidbody.linearVelocity = direction * dashSpeed;
            elapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        _rigidbody.linearVelocity = Vector2.zero;
        dashCooldownTimer = dashCooldown;
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
