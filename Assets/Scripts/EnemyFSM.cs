using UnityEngine;
using UnityEngine.AI;

public class EnemyFSM : MonoBehaviour
{
    public enum EnemyState
    {
        Patrol,
        Alert,
        Chase,
        Search,
        Attack,
        Flee,
        Heal,   // Tahap 89: Memulihkan HP di SafePoint
        Dead
    }

    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private EnemyPerception perception;
    [SerializeField] private EnemyHealth health;
    [SerializeField] private Animator animator;

    [Header("Patrol")]
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float waypointTolerance = 0.5f;

    [Header("Alert")]
    [SerializeField] private float alertDuration = 1.5f;

    [Header("Chase")]
    [SerializeField] private float chaseSpeed = 4f;
    [SerializeField] private float lostPlayerDelay = 2f;

    [Header("Search")]
    [SerializeField] private float searchDuration = 3f;
    [SerializeField] private float searchSpeed = 2.5f;

    [Header("Attack")]
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackExitRange = 2.75f;
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private float attackCooldown = 1.5f;

    [Header("Flee")]
    [SerializeField] private Transform safePoint;
    [SerializeField] private float fleeSpeed = 5f;
    [SerializeField] private float lowHealthThreshold = 30f;
    [SerializeField] private float safeDistance = 12f;

    [Header("Heal (Tahap 89)")]
    [SerializeField] private float healRate = 25f;

    [Header("Debug")]
    [SerializeField] private EnemyState currentState;

    private int currentPatrolIndex = 0;

    private float alertTimer = 0f;
    private float searchTimer = 0f;
    private Vector3 lastSeenPosition;

    private float lostPlayerTimer = 0f;
    private float nextAttackTime = 0f;

    private bool fleeTriggered = false;

    private PlayerHealth playerHealth;

    public EnemyState CurrentState => currentState;

    private void Awake()
    {
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        if (perception == null)
            perception = GetComponent<EnemyPerception>();

        if (health == null)
            health = GetComponent<EnemyHealth>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (player != null)
            playerHealth = player.GetComponent<PlayerHealth>();
    }

    private void Start()
    {
        ChangeState(EnemyState.Patrol);
    }

    private void Update()
    {
        // ================================
        // GLOBAL TRANSITION PRIORITY
        // ================================

        if (health.IsDead)
        {
            ChangeState(EnemyState.Dead);
            return;
        }

        if (!fleeTriggered &&
            health.CurrentHealth <= lowHealthThreshold &&
            currentState != EnemyState.Flee &&
            currentState != EnemyState.Heal)
        {
            fleeTriggered = true;
            ChangeState(EnemyState.Flee);
        }

        // ================================
        // UPDATE CURRENT STATE
        // ================================

        switch (currentState)
        {
            case EnemyState.Patrol:
                UpdatePatrol();
                break;

            case EnemyState.Alert:
                UpdateAlert();
                break;

            case EnemyState.Chase:
                UpdateChase();
                break;

            case EnemyState.Search:
                UpdateSearch();
                break;

            case EnemyState.Attack:
                UpdateAttack();
                break;

            case EnemyState.Flee:
                UpdateFlee();
                break;

            case EnemyState.Heal:
                UpdateHeal();
                break;

            case EnemyState.Dead:
                UpdateDead();
                break;
        }
    }

    // =====================================
    // CHANGE STATE
    // =====================================

    private void ChangeState(EnemyState newState)
    {
        if (currentState == newState)
            return;

        ExitState(currentState);

        currentState = newState;

        Debug.Log(
            gameObject.name +
            " → State: " +
            currentState
        );

        EnterState(currentState);
    }

    // =====================================
    // ENTER STATE
    // =====================================

    private void EnterState(EnemyState state)
    {
        switch (state)
        {
            case EnemyState.Patrol:

                agent.isStopped = false;
                agent.speed = patrolSpeed;

                if (animator != null)
                    animator.Play("Z_walk");

                SetPatrolDestination();

                break;

            case EnemyState.Alert:

                agent.isStopped = true;
                agent.ResetPath();
                alertTimer = 0f;

                if (animator != null)
                    animator.Play("Z_idle_A");

                break;

            case EnemyState.Chase:

                agent.isStopped = false;
                agent.speed = chaseSpeed;

                if (animator != null)
                    animator.Play("Z_run");

                lostPlayerTimer = 0f;

                break;

            case EnemyState.Search:

                agent.isStopped = false;
                agent.speed = searchSpeed;
                searchTimer = 0f;

                agent.SetDestination(lastSeenPosition);

                if (animator != null)
                    animator.Play("Z_walk");

                break;

            case EnemyState.Attack:

                agent.isStopped = true;
                agent.ResetPath();

                if (animator != null)
                    animator.Play("Z_attack_A");

                break;

            case EnemyState.Flee:

                agent.isStopped = false;
                agent.speed = fleeSpeed;

                if (animator != null)
                    animator.Play("Z_run");

                if (safePoint != null)
                {
                    agent.SetDestination(
                        safePoint.position
                    );
                }

                break;

            case EnemyState.Heal:

                agent.isStopped = true;
                agent.ResetPath();

                if (animator != null)
                    animator.Play("Z_idle_A");

                break;

            case EnemyState.Dead:

                agent.isStopped = true;
                agent.ResetPath();

                if (animator != null)
                    animator.Play("Z_death_A");

                break;
        }
    }

    // =====================================
    // EXIT STATE
    // =====================================

    private void ExitState(EnemyState state)
    {
        switch (state)
        {
            case EnemyState.Alert:
            case EnemyState.Attack:
            case EnemyState.Heal:
                agent.isStopped = false;
                break;
        }
    }

    // =====================================
    // PATROL
    // =====================================

    private void UpdatePatrol()
    {
        if (perception.CanSeePlayer)
        {
            ChangeState(EnemyState.Alert);
            return;
        }

        if (patrolPoints == null ||
            patrolPoints.Length == 0)
            return;

        if (!agent.pathPending &&
            agent.remainingDistance <=
            waypointTolerance)
        {
            currentPatrolIndex++;

            if (currentPatrolIndex >=
                patrolPoints.Length)
            {
                currentPatrolIndex = 0;
            }

            SetPatrolDestination();
        }
    }

    private void SetPatrolDestination()
    {
        if (patrolPoints == null ||
            patrolPoints.Length == 0)
            return;

        Transform point =
            patrolPoints[currentPatrolIndex];

        if (point != null)
        {
            agent.SetDestination(
                point.position
            );
        }
    }

    // =====================================
    // ALERT
    // =====================================

    private void UpdateAlert()
    {
        FacePlayer();

        if (!perception.CanSeePlayer)
        {
            ChangeState(EnemyState.Patrol);
            return;
        }

        alertTimer += Time.deltaTime;

        if (alertTimer >= alertDuration)
        {
            ChangeState(EnemyState.Chase);
            return;
        }
    }

    // =====================================
    // CHASE
    // =====================================

    private void UpdateChase()
    {
        float distance =
            Vector3.Distance(
                transform.position,
                player.position
            );

        if (perception.CanSeePlayer)
        {
            lostPlayerTimer = 0f;

            lastSeenPosition = player.position;

            agent.SetDestination(
                player.position
            );

            if (distance <= attackRange)
            {
                ChangeState(
                    EnemyState.Attack
                );

                return;
            }
        }
        else
        {
            lostPlayerTimer +=
                Time.deltaTime;

            if (lostPlayerTimer >=
                lostPlayerDelay)
            {
                ChangeState(
                    EnemyState.Search
                );

                return;
            }
        }
    }

    // =====================================
    // SEARCH
    // =====================================

    private void UpdateSearch()
    {
        if (perception.CanSeePlayer)
        {
            ChangeState(EnemyState.Chase);
            return;
        }

        searchTimer += Time.deltaTime;

        if (searchTimer >= searchDuration)
        {
            ChangeState(EnemyState.Patrol);
            return;
        }
    }

    // =====================================
    // ATTACK
    // =====================================

    private void UpdateAttack()
    {
        float distance =
            Vector3.Distance(
                transform.position,
                player.position
            );

        FacePlayer();

        if (!perception.CanSeePlayer ||
            distance > attackExitRange)
        {
            ChangeState(
                EnemyState.Chase
            );

            return;
        }

        if (Time.time >= nextAttackTime)
        {
            AttackPlayer();

            nextAttackTime =
                Time.time +
                attackCooldown;
        }
    }

    private void AttackPlayer()
    {
        Debug.Log("Enemy attacks Player!");

        if (playerHealth != null)
        {
            playerHealth.TakeDamage(
                attackDamage
            );
        }
    }

    private void FacePlayer()
    {
        Vector3 direction =
            player.position -
            transform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f)
            return;

        Quaternion targetRotation =
            Quaternion.LookRotation(
                direction
            );

        transform.rotation =
            Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                10f * Time.deltaTime
            );
    }

    // =====================================
    // FLEE
    // =====================================

    private void UpdateFlee()
    {
        if (safePoint == null)
            return;

        float playerDistance =
            Vector3.Distance(
                transform.position,
                player.position
            );

        float safePointDistance =
            Vector3.Distance(
                transform.position,
                safePoint.position
            );

        if (playerDistance >= safeDistance ||
            safePointDistance <= 1.5f)
        {
            // Tahap 89: Pindah ke State Heal saat sampai di SafePoint
            ChangeState(
                EnemyState.Heal
            );

            return;
        }
    }

    // =====================================
    // HEAL (Tahap 89)
    // =====================================

    private void UpdateHeal()
    {
        if (perception.CanSeePlayer)
        {
            ChangeState(EnemyState.Chase);
            return;
        }

        if (health != null)
        {
            health.Heal(healRate * Time.deltaTime);

            if (health.CurrentHealth >= health.MaxHealth)
            {
                fleeTriggered = false;
                ChangeState(EnemyState.Patrol);
                return;
            }
        }
        else
        {
            fleeTriggered = false;
            ChangeState(EnemyState.Patrol);
            return;
        }
    }

    // =====================================
    // DEAD
    // =====================================

    private void UpdateDead()
    {
        // Tidak melakukan action.
    }

    // =====================================
    // DEBUG GIZMOS
    // =====================================

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(
            transform.position,
            attackRange
        );

        Gizmos.color = Color.magenta;

        Gizmos.DrawWireSphere(
            transform.position,
            attackExitRange
        );

        if (currentState == EnemyState.Search)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(lastSeenPosition, 0.8f);
            Gizmos.DrawLine(transform.position, lastSeenPosition);
        }
    }
}