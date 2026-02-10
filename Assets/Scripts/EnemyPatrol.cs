using UnityEngine;
using System.Collections;

public class EnemyPatrolAI : MonoBehaviour
{
    public LayerMask playerLayer;
    public float playerDetectRadius = 3f;
    public Transform playerCheck;

    [Header("Refs")]
    public Rigidbody2D rb;
    public Transform ledgeCheck;
    public Transform wallCheck;
    public LayerMask groundLayer;
    public EnemyController enemyController;

    [Header("Speed")]
    public float patrolSpeed = 1.5f;
    public float chaseSpeed = 2.5f;

    [Header("Ledge / Wall")]
    public float ledgeRayDistance = 0.4f;
    public float wallRayDistance = 0.2f;

    [Header("Random Behaviour")]
    public float minIdleTime = 1f;
    public float maxIdleTime = 3f;
    public float flipChance = 0.3f;

    private bool isIdle = false;
    private bool isChasing = false;
    private Transform player;

    void Start()
    {
        if (!rb) rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        enemyController = GetComponent<EnemyController>();
        StartCoroutine(RandomIdleRoutine());
    }

    void FixedUpdate()
    {
        if (enemyController != null && enemyController.health <= 0)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        DetectPlayer();

        if (isChasing)
        {
            Chase();
            return;
        }

        if (isIdle)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }

        Patrol();
    }

    void DetectPlayer()
    {
        if (!player || !playerCheck)
        {
            isChasing = false;
            return;
        }

        bool seen = Physics2D.OverlapCircle(
            playerCheck.position,
            playerDetectRadius,
            playerLayer
        );

        isChasing = seen;
    }

    void Patrol()
    {
        int dir = transform.localScale.x > 0 ? 1 : -1;

        bool groundAhead = Physics2D.Raycast(
            ledgeCheck.position,
            Vector2.down,
            ledgeRayDistance,
            groundLayer
        );

        bool wallAhead = Physics2D.Raycast(
            wallCheck.position,
            Vector2.right * dir,
            wallRayDistance,
            groundLayer
        );

        if (!groundAhead || wallAhead)
        {
            Flip();
            return;
        }

        rb.linearVelocity = new Vector2(dir * patrolSpeed, rb.linearVelocity.y);
    }

    void Chase()
    {
        int dir = player.position.x > transform.position.x ? 1 : -1;

        if ((dir == 1 && transform.localScale.x < 0) ||
            (dir == -1 && transform.localScale.x > 0))
        {
            Flip();
        }

        bool groundAhead = Physics2D.Raycast(
            ledgeCheck.position,
            Vector2.down,
            ledgeRayDistance,
            groundLayer
        );

        bool wallAhead = Physics2D.Raycast(
            wallCheck.position,
            Vector2.right * dir,
            wallRayDistance,
            groundLayer
        );

        if (!groundAhead || wallAhead)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }

        rb.linearVelocity = new Vector2(dir * chaseSpeed, rb.linearVelocity.y);
    }

    void Flip()
    {
        Vector3 s = transform.localScale;
        s.x *= -1;
        transform.localScale = s;
    }

    IEnumerator RandomIdleRoutine()
    {
        while (true)
        {
            float wait = Random.Range(minIdleTime, maxIdleTime);
            yield return new WaitForSeconds(wait);

            if (isChasing) continue;

            isIdle = true;

            if (Random.value < flipChance)
                Flip();

            float idleDuration = Random.Range(0.5f, 1.5f);
            yield return new WaitForSeconds(idleDuration);

            isIdle = false;
        }
    }

    void OnDrawGizmosSelected()
    {
        if (ledgeCheck)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(
                ledgeCheck.position,
                ledgeCheck.position + Vector3.down * ledgeRayDistance
            );
        }

        if (wallCheck)
        {
            int dir = transform.localScale.x > 0 ? 1 : -1;
            Gizmos.color = Color.red;
            Gizmos.DrawLine(
                wallCheck.position,
                wallCheck.position + Vector3.right * dir * wallRayDistance
            );
        }

        if (playerCheck)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(playerCheck.position, playerDetectRadius);
        }
    }
}
