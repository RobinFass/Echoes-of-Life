// csharp
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class EnemyController2D : MonoBehaviour
{
    private enum State { Idle, Patrol, Chase }

    [Header("Room")]
    [Tooltip("Room area that gates AI. Logic only runs when Player is inside this collider.")]
    [SerializeField] private Collider2D roomArea;

    [Header("Targeting")]
    [SerializeField] private float detectionRange = 7f;
    [SerializeField] private float loseSightRange = 10f;
    [SerializeField] private float stoppingDistance = 1.2f;
    [SerializeField] private bool requireLineOfSight = false;
    [SerializeField] private LayerMask obstacleMask = 0;

    [Header("Movement")]
    [SerializeField] private float patrolSpeed = 2.0f;
    [SerializeField] private float chaseSpeed = 3.5f;
    [SerializeField] private float acceleration = 20f;

    [Header("Patrol")]
    [Tooltip("Optional patrol points. If empty, enemy idles until player is detected.")]
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private float waypointTolerance = 0.2f;
    [SerializeField] private float waitAtWaypoint = 0.5f;

    private Rigidbody2D rb;
    private Transform target; // Player
    private int patrolIndex;
    private float waitTimer;
    private Vector2 velocity;
    private State state = State.Idle;

    private bool ready;
    private bool playerInRoom;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.gravityScale = 0f;
        rb.constraints |= RigidbodyConstraints2D.FreezeRotation;
    }

    private void OnEnable()
    {
        ready = false;
        StartCoroutine(ResolvePlayerOnce());
    }

    private IEnumerator ResolvePlayerOnce()
    {
        // Wait for Player singleton to exist
        while (Player.Instance == null)
            yield return null;

        target = Player.Instance.transform;
        ready = true;
    }

    private void Update()
    {
        if (!ready) return;

        // Room gating
        playerInRoom = roomArea == null || (target != null && roomArea.OverlapPoint(target.position));
        if (!playerInRoom) return;

        float dist = target != null ? Vector2.Distance(transform.position, target.position) : Mathf.Infinity;
        bool canSeeTarget = !requireLineOfSight || HasLineOfSight();

        switch (state)
        {
            case State.Idle:
                // Transition to chase if detected
                if (dist <= detectionRange && canSeeTarget)
                {
                    state = State.Chase;
                }
                else if (patrolPoints != null && patrolPoints.Length > 0)
                {
                    state = State.Patrol;
                    waitTimer = 0f;
                }
                break;

            case State.Patrol:
                // Detect and switch to chase
                if (dist <= detectionRange && canSeeTarget)
                {
                    state = State.Chase;
                }
                break;

            case State.Chase:
                // Lose target -> fallback to patrol/idle
                if (dist > loseSightRange || (requireLineOfSight && !canSeeTarget))
                {
                    state = (patrolPoints != null && patrolPoints.Length > 0) ? State.Patrol : State.Idle;
                }
                break;
        }
    }

    private void FixedUpdate()
    {
        if (!ready || !playerInRoom) return;

        switch (state)
        {
            case State.Idle:
                Decelerate();
                break;

            case State.Patrol:
                PatrolMove();
                break;

            case State.Chase:
                ChaseMove();
                break;
        }
    }

    private void PatrolMove()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            state = State.Idle;
            Decelerate();
            return;
        }

        Transform wp = patrolPoints[patrolIndex];
        if (wp == null)
        {
            AdvanceWaypoint();
            return;
        }

        Vector2 pos = rb.position;
        Vector2 to = (Vector2)wp.position - pos;
        float dist = to.magnitude;

        if (dist <= waypointTolerance)
        {
            waitTimer += Time.fixedDeltaTime;
            if (waitTimer >= waitAtWaypoint)
            {
                waitTimer = 0f;
                AdvanceWaypoint();
            }
            Decelerate();
            return;
        }

        Vector2 desired = to.normalized * patrolSpeed;
        velocity = Vector2.MoveTowards(velocity, desired, acceleration * Time.fixedDeltaTime);
        rb.MovePosition(pos + velocity * Time.fixedDeltaTime);
    }

    private void ChaseMove()
    {
        if (target == null)
        {
            state = (patrolPoints != null && patrolPoints.Length > 0) ? State.Patrol : State.Idle;
            Decelerate();
            return;
        }

        Vector2 pos = rb.position;
        Vector2 to = (Vector2)target.position - pos;
        float dist = to.magnitude;

        if (dist <= stoppingDistance)
        {
            // At desired range
            Decelerate();
            return;
        }

        Vector2 desired = to.normalized * chaseSpeed;
        velocity = Vector2.MoveTowards(velocity, desired, acceleration * Time.fixedDeltaTime);
        rb.MovePosition(pos + velocity * Time.fixedDeltaTime);
    }

    private void Decelerate()
    {
        velocity = Vector2.MoveTowards(velocity, Vector2.zero, acceleration * Time.fixedDeltaTime);
        if (velocity.sqrMagnitude > 0f)
            rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
    }

    private void AdvanceWaypoint()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return;
        patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
    }

    private bool HasLineOfSight()
    {
        if (target == null) return false;
        Vector2 from = transform.position;
        Vector2 to = target.position;
        var hit = Physics2D.Linecast(from, to, obstacleMask);
        return hit.collider == null;
    }

    private void OnDrawGizmosSelected()
    {
        // Ranges
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, loseSightRange);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, stoppingDistance);

        // Patrol path
        if (patrolPoints != null && patrolPoints.Length > 1)
        {
            Gizmos.color = Color.green;
            for (int i = 0; i < patrolPoints.Length; i++)
            {
                var a = patrolPoints[i];
                var b = patrolPoints[(i + 1) % patrolPoints.Length];
                if (a != null && b != null)
                {
                    Gizmos.DrawLine(a.position, b.position);
                    Gizmos.DrawWireSphere(a.position, 0.1f);
                }
            }
        }
    }
}
