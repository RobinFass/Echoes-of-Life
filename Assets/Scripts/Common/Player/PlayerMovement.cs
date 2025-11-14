using System;
using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")] [SerializeField] private float moveForce = 2000f;

    [SerializeField] private float sprintMultiplier = 2f;
    [SerializeField] private float sprintStaminaCostPerSecond = 2f;

    [Header("Dash")] [SerializeField] private float dashDistance = 5f;

    [SerializeField] private float dashDuration = 0.15f;
    [SerializeField] private float dashStaminaCost = 2f;

    [SerializeField] private Rigidbody2D rigidBody;

    private Vector2 lastMoveInput = new(1f, 0f);
    public static PlayerMovement Instance { get; private set; }
    private GameInput input => GameInput.Instance;
    private PlayerStats stats => Player.Instance.Stats;
    private PlayerAnimation anime => Player.Instance.Animation;
    private GameManager GameManager => GameManager.Instance;

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
        if(GameManager.State != GameState.Playing) return;
        var moveInput = input ? input.OnMove() : Vector2.zero;
        if (moveInput == Vector2.zero)
        {
            anime.PauseSprint();
            return;
        }

        lastMoveInput = moveInput;

        var speed = moveForce;
        if (input.OnSprint())
        {
            var used = sprintStaminaCostPerSecond * Time.fixedDeltaTime;
            if (stats.UseStamina(used))
            {
                anime.PlaySprint();
                speed *= sprintMultiplier;
            }
        }

        rigidBody.AddForce(Time.fixedDeltaTime * speed * moveInput);
    }

    private void OnDashEvent(object sender, EventArgs e)
    {
        if (!stats.UseStamina(dashStaminaCost)) return;
        var dir = input.OnMove().normalized;
        if (dir == Vector2.zero) dir = lastMoveInput.normalized;
        StartCoroutine(DashCoroutine(dir));
        anime.PlayDash();
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