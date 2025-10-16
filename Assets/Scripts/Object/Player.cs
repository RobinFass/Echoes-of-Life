using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public static Player Instance { get; private set; }
    
    [SerializeField] private float moveSpeed = 4000f;
    
    [SerializeField] private float sprintMultiplier = 1.5f;
    
    [SerializeField] private float dashDistance = 5f;
    [SerializeField] private float dashDuration = 0.1f;
    [SerializeField] private float dashCooldown = 3f;
    
    [SerializeField] private float maxStamina = 20f;
    [SerializeField] private float staminaRegenRate = 2f;
    [SerializeField] private float staminaRegenCooldown = 2f;
    [SerializeField] private float sprintStaminaCost = 2f;
    [SerializeField] private float dashStaminaCost = 2f;
    
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator animator;
    
    private Rigidbody2D _rigidbody;
    private bool isSprinting => gameInput.OnSprint();
    private float dashCooldownTimer = 0f;
    private float staminaRegenCooldownTimer = 0f;
    private Vector2 dashDirection;
    private Vector2 moveInput => gameInput.OnMove();
    private Vector2 lastMoveInput = new Vector2(1f, 0f);
    private float stamina;
    private GameManager gameManager => GameManager.Instance;
    private GameInput gameInput => GameInput.Instance;
    public event EventHandler OnPickUpCoin;
    public event EventHandler OnEnemyHit;
    public event EventHandler<Room> OnChangingRoom;
    public event EventHandler OnPlayerWin;
    
    
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
        gameManager.OnPlayerDeath += GameManager_OnGameOver;
        gameInput.OnDashEvent += GameInput_OnOnDashEvent;
    }
    
    private void FixedUpdate()
    {
        switch (gameManager.State)
        {
            case GameState.Playing:
                break;
            default:
                return;
        }

        var speed = moveSpeed;

        if (isSprinting && stamina <= 0)
        {            
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
        
        animator.SetBool("isMoving", moveInput != Vector2.zero);
        
        if(moveInput.x > 0)
            spriteRenderer.flipX = false;
        else if(moveInput.x < 0)
            spriteRenderer.flipX = true;
        
        _rigidbody.AddForce(Time.fixedDeltaTime * speed * moveInput);

        if (dashCooldownTimer > 0f)
            dashCooldownTimer -= Time.fixedDeltaTime;
        
        if (staminaRegenCooldownTimer > 0f)
            staminaRegenCooldownTimer -= Time.fixedDeltaTime;
        stamina = Mathf.Clamp(stamina, 0, maxStamina);
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
            case Enemy enemy:
            {
                enemy.Health--;
                OnEnemyHit?.Invoke(this, EventArgs.Empty);
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
    
    private void GameManager_OnGameOver(object sender, EventArgs e)
    {
        gameObject.SetActive(false);
    }
    
    private void GameInput_OnOnDashEvent(object sender, EventArgs e)
    {
        if (stamina >= dashStaminaCost)
        {
            stamina -= dashStaminaCost;
            staminaRegenCooldownTimer = staminaRegenCooldown;
            var direction = moveInput.normalized == Vector2.zero ? lastMoveInput.normalized : moveInput.normalized;
            StartCoroutine(DashCoroutine(direction));
        }
    }
    
    private IEnumerator DashCoroutine(Vector2 direction)
    {
        var elapsed = 0f;
        var dashSpeed = dashDistance / dashDuration;
        while (elapsed < dashDuration)
        {
            _rigidbody.linearVelocity = direction * dashSpeed;
            elapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        _rigidbody.linearVelocity = Vector2.zero;
        dashCooldownTimer = dashCooldown;
    }
}
