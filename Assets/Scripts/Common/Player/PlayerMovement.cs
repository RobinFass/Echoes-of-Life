using System;
using System.Collections;
using UnityEngine;

namespace Common.Player
{
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Movement")] [SerializeField] private float moveForce = 2000f;
        [SerializeField] private float sprintMultiplier = 2f;
        [SerializeField] private float sprintStaminaCostPerSecond = 2f;

        [Header("Dash")] [SerializeField] private float dashDistance = 5f;
        [SerializeField] private float dashDuration = 0.15f;
        [SerializeField] private float dashStaminaCost = 2f;

        [SerializeField] private Rigidbody2D rigidBody;
        
        public static PlayerMovement Instance { get; private set; }
        private static GameInput Input => GameInput.Instance;
        private static PlayerStats Stats => Object.Player.Stats;
        private static PlayerAnimation Anime => Object.Player.Animation;
        private static GameManager GameManager => GameManager.Instance;
        
        private AudioManager audioManager;
        private Vector2 lastMoveInput = new(1f, 0f);
        private enum MoveSfxState { None, Walk, Run }
        private MoveSfxState currentMoveSfx = MoveSfxState.None;
    
        public bool Dashing { get; private set; }

        private void Awake()
        {
            Instance = this;
            audioManager = AudioManager.Instance;
        }

        private void Start()
        {
            if (Input) Input.OnDashEvent += OnDashEvent;
        }

        private void FixedUpdate()
        {
            if (PlayerStats.Instance.NormalizedHealth == 0 || GameManager.State != GameState.Playing)
            {
                if (currentMoveSfx == MoveSfxState.None || audioManager == null) return;
                audioManager.StopLoopingSfx();
                currentMoveSfx = MoveSfxState.None;
                return;
            }
            var moveInput = Input ? Input.OnMove() : Vector2.zero;
            if (moveInput == Vector2.zero)
            {
                if (currentMoveSfx != MoveSfxState.None && audioManager != null)
                {
                    audioManager.StopLoopingSfx();
                    currentMoveSfx = MoveSfxState.None;
                }

                Anime.PauseSprint();
                return;
            }
            lastMoveInput = moveInput;
            float speed = moveForce;
            bool sprintKeyHeld = Input.OnSprint();
            bool isSprintingThisFrame = false;
            if (sprintKeyHeld)
            {
                float used = sprintStaminaCostPerSecond * Time.fixedDeltaTime;
                if (Stats.UseStamina(used))
                {
                    isSprintingThisFrame = true;
                }
            }
            if (isSprintingThisFrame)
            {
                Anime.PlaySprint();
                speed *= sprintMultiplier;
            }
            else
            {
                Anime.PauseSprint();
            }
            if (audioManager != null)
            {
                if (isSprintingThisFrame)
                {
                    if (currentMoveSfx != MoveSfxState.Run)
                    {
                        audioManager.StopLoopingSfx();
                        audioManager.PlayLoopingSfx("run");
                        currentMoveSfx = MoveSfxState.Run;
                    }
                }
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
            if (!Stats.UseStamina(dashStaminaCost)) return;
            if (GameManager.State != GameState.Playing) return;
            var dir = Input.OnMove().normalized;
            if (dir == Vector2.zero) dir = lastMoveInput.normalized;
            Dashing = true;
            AudioManager.Instance?.PlaySfx("dash");
            StartCoroutine(DashCoroutine(dir));
            Anime.PlayDash();
        }

        private IEnumerator DashCoroutine(Vector2 direction)
        {
            float elapsed = 0f;
            float dashSpeed = dashDistance / dashDuration;
            while (elapsed < dashDuration)
            {
                if (rigidBody) rigidBody.linearVelocity = direction * dashSpeed;
                elapsed += Time.fixedDeltaTime;
                yield return new WaitForFixedUpdate();
            }
            if (rigidBody) rigidBody.linearVelocity = Vector2.zero;
            Dashing = false;
        }
    }
}
