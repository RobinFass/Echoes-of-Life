using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public static PlayerMovement Instance { get; private set; }
    
    [Header("Movement")]
    [SerializeField] private float moveForce = 2000f;
    [SerializeField] private float sprintMultiplier = 2f;
    [SerializeField] private float sprintStaminaCostPerSecond = 2f;

    [Header("Dash")]
    [SerializeField] private float dashDistance = 5f;
    [SerializeField] private float dashDuration = 0.15f;
    [SerializeField] private float dashStaminaCost = 2f;

    [SerializeField] private Rigidbody2D rigidBody;

    private Vector2 lastMoveInput = new Vector2(1f, 0f);
    private GameInput input => GameInput.Instance;
    private PlayerStats stats => Player.Instance != null ? Player.Instance.Stats : null;

    private void Awake()
    {
        Instance = this;
    }
    
    private void Start()
    {
        if (input)
            input.OnDashEvent += OnDashEvent;
    }

    private void FixedUpdate()
    {
        var moveInput = input ? input.OnMove() : Vector2.zero;
        if (moveInput != Vector2.zero) lastMoveInput = moveInput;

        var speed = moveForce;
        if (input && input.OnSprint() && stats)
        {
            var used = sprintStaminaCostPerSecond * Time.fixedDeltaTime;
            if (stats.UseStamina(used))
            {
                speed *= sprintMultiplier;
            }
        }

        if (rigidBody)
            rigidBody.AddForce(Time.fixedDeltaTime * speed * moveInput);
    }

    private void OnDashEvent(object sender, System.EventArgs e)
    {
        if (!stats || !input || !rigidBody) return;

        if (stats.UseStamina(dashStaminaCost))
        {
            var dir = input.OnMove().normalized;
            if (dir == Vector2.zero) dir = lastMoveInput.normalized;
            StartCoroutine(DashCoroutine(dir));
        }
    }

    private IEnumerator DashCoroutine(Vector2 direction)
    {
        var elapsed = 0f;
        var dashSpeed = dashDistance / dashDuration;
        while (elapsed < dashDuration)
        {
            if (rigidBody)
                rigidBody.linearVelocity = direction * dashSpeed;
            elapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        if (rigidBody)
            rigidBody.linearVelocity = Vector2.zero;
    }
}
