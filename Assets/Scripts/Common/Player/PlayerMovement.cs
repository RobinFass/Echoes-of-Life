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
    private AudioManager audioManager;

    private Vector2 lastMoveInput = new(1f, 0f);
    public static PlayerMovement Instance { get; private set; }
    private GameInput input => GameInput.Instance;
    private PlayerStats stats => Player.Instance.Stats;
    private PlayerAnimation anime => Player.Instance.Animation;
    private GameManager GameManager => GameManager.Instance;

    // track what loop we are currently playing to avoid restarting each frame
    private enum MoveSfxState { None, Walk, Run }
    private MoveSfxState currentMoveSfx = MoveSfxState.None;
    
    public bool dashing { get; private set; }

    private void Awake()
    {
        Instance = this;
        audioManager = AudioManager.Instance;
    }

    private void Start()
    {
        if (input)
            input.OnDashEvent += OnDashEvent;
    }

    private void FixedUpdate()
    {
        if(PlayerStats.Instance.NormalizedHealth == 0) return;
        // stop movement SFX when game is not in Playing state
        if (GameManager.State != GameState.Playing)
        {
            if (currentMoveSfx != MoveSfxState.None && audioManager != null)
            {
                audioManager.StopLoopingSfx();
                currentMoveSfx = MoveSfxState.None;
            }
            return;
        }

        var moveInput = input ? input.OnMove() : Vector2.zero;

        // No movement: no sound, no sprint anim
        if (moveInput == Vector2.zero)
        {
            if (currentMoveSfx != MoveSfxState.None && audioManager != null)
            {
                audioManager.StopLoopingSfx();
                currentMoveSfx = MoveSfxState.None;
            }

            anime.PauseSprint();
            return;
        }

        lastMoveInput = moveInput;

        var speed = moveForce;
        var sprintKeyHeld = input.OnSprint();

        // compute sprint state EXACTLY as we drive the animation: key + stamina
        bool isSprintingThisFrame = false;
        if (sprintKeyHeld)
        {
            var used = sprintStaminaCostPerSecond * Time.fixedDeltaTime;
            if (stats.UseStamina(used))
            {
                isSprintingThisFrame = true;
            }
        }

        // Animations
        if (isSprintingThisFrame)
        {
            anime.PlaySprint();
            speed *= sprintMultiplier;
        }
        else
        {
            anime.PauseSprint();
        }

        // Sounds: mirror the same condition
        if (audioManager != null)
        {
            // running
            if (isSprintingThisFrame)
            {
                if (currentMoveSfx != MoveSfxState.Run)
                {
                    audioManager.StopLoopingSfx();
                    audioManager.PlayLoopingSfx("run");
                    currentMoveSfx = MoveSfxState.Run;
                }
            }
            // walking (moving but not sprinting)
            else
            {
                if (currentMoveSfx != MoveSfxState.Walk)
                {
                    audioManager.StopLoopingSfx();
                    audioManager.PlayLoopingSfx("walk");
                    currentMoveSfx = MoveSfxState.Walk;
                }
            }
        }

        rigidBody.AddForce(Time.fixedDeltaTime * speed * moveInput);
    }

    private void OnDashEvent(object sender, EventArgs e)
    {
        if (!stats.UseStamina(dashStaminaCost)) return;
        var dir = input.OnMove().normalized;
        if (dir == Vector2.zero) dir = lastMoveInput.normalized;
        dashing = true;
        AudioManager.Instance?.PlaySfx("dash");
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
        dashing = false;
    }
}