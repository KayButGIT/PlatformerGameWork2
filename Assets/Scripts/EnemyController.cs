using UnityEngine;
using System.Collections;

public class EnemyController : MonoBehaviour
{
    [Header("Enemy General Information")]
    public int health = 2;
    public float moveSpeed = 2f;
    public LayerMask groundLayer;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask playerLayer;
    public Animator animator;

    [Header("Enemy Attack")]
    public float detectionRadius = 5f;
    public float detectionOffset = 1f;
    public float attackDistance = 1.2f;
    public float attackRate = 1f;
    public int attackDamage = 1;
    public Transform attackPoint;
    public float attackRange = 0.5f;
    public float attackDelay = 0.5f;
    public float resumeMovementAfterAttack = 1.5f;

    [Header("Enemy SFX")]
    public AudioClip getHitSound;
    public AudioClip attackSound;
    public AudioClip dieSound;

    [Header("Scores given")]
    public int minscore = 100;
    public int maxscore = 120;

    [Header("Knockback Manage")]
    public float knockbackStrength = 20f;
    public float hitStunAttackDelay = 0.1f;

    // >>> ONLY NEW PART <<<
    [Header("Ledge Check")]
    public Transform ledgeCheck;
    public float ledgeCheckDistance = 0.5f;
    // >>> ONLY NEW PART <<<

    private Transform player;
    private Rigidbody2D rb;
    private float nextAttackTime;
    private bool isGrounded, isFacingRight = true, canMove = true, isDie = false;

    void Start()
    {
        isFacingRight = false;
        transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);

        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player == null) Debug.LogError("Player not found!");

        animator.SetInteger("Health", health);
    }

    void CheckGrounded() => isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

    // >>> ONLY NEW FUNCTION <<<
    bool IsGroundAhead()
    {
        RaycastHit2D hit = Physics2D.Raycast(ledgeCheck.position, Vector2.down, ledgeCheckDistance, groundLayer);
        return hit.collider != null;
    }
    // >>> ONLY NEW FUNCTION <<<

    bool DetectPlayerInFront()
    {
        int direction = transform.localScale.x > 0 ? 1 : -1;

        Vector2 dir = Vector2.right * direction;
        Vector2 offset = dir * -detectionOffset;

        RaycastHit2D hit = Physics2D.BoxCast(
            transform.position + (Vector3)offset,
            new Vector2(attackRange, 1f),
            0f,
            dir,
            detectionRadius,
            playerLayer
        );

        return hit.collider != null && hit.collider.CompareTag("Player");
    }


    void Update()
    {
        CheckGrounded();
        animator.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));

        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (!isDie)
        {
            if (distanceToPlayer <= attackDistance)
            {
                AttackPlayer();
            }
            else if (DetectPlayerInFront() && canMove)
            {
                ChasePlayer();
            }
        }
    }

    void FixedUpdate()
    {
        if (canMove && player != null && DetectPlayerInFront())
        {
            ChasePlayer();
        }
    }

    void ChasePlayer()
    {
        if (player == null) return;

        // >>> ONLY NEW GUARD <<<
        if (!IsGroundAhead())
        {
            // Stop only forward movement
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

            // Allow turning if player changes side
            FlipTowardsPlayer();
            return;
        }

        // >>> ONLY NEW GUARD <<<

        FlipTowardsPlayer();

        Vector2 direction = (player.position - transform.position).normalized;
        rb.linearVelocity = new Vector2(direction.x * moveSpeed, rb.linearVelocity.y);
    }

    void FlipTowardsPlayer()
    {
        if (player == null || isDie) return;

        bool shouldFaceRight = player.position.x > transform.position.x;

        if (shouldFaceRight != isFacingRight)
        {
            Flip();
        }
    }

    void Flip()
    {
        isFacingRight = !isFacingRight;
        transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
    }

    void AttackPlayer()
    {
        if (Time.time < nextAttackTime) return;

        nextAttackTime = Time.time + attackRate;
        rb.linearVelocity = Vector2.zero;
        canMove = false;

        FlipTowardsPlayer();

        animator.SetTrigger("Attack");
        PlayAttackSound();
        StartCoroutine(ApplyDamageAfterDelay());
        StartCoroutine(ResumeMovementAfterAttack());
    }

    IEnumerator ApplyDamageAfterDelay()
    {
        yield return new WaitForSeconds(attackDelay);
        Collider2D[] hitPlayers = Physics2D.OverlapBoxAll(attackPoint.position, new Vector2(attackRange, attackRange / 2), 0, playerLayer);
        foreach (Collider2D hitPlayer in hitPlayers)
        {
            var playerHealth = hitPlayer?.GetComponent<PlayerHealth>();
            playerHealth?.TakeDamage(attackDamage, (hitPlayer.transform.position - rb.transform.position).normalized, knockbackStrength);
        }
    }

    IEnumerator ResumeMovementAfterAttack()
    {
        yield return new WaitForSeconds(resumeMovementAfterAttack);
        canMove = true;
    }

    public void TakeDamage(int damage, Vector2 hitDirection)
    {
        health -= damage;
        animator.SetTrigger("GetHit");
        PlayGetHitSound();
        Knockback(hitDirection);

        if (health <= 0)
        {
            PlayDieSound();
            canMove = false;
            animator.SetBool("IsDie", true);
            rb.linearVelocity = Vector2.zero;
            rb.isKinematic = true;
            rb.simulated = false;
            isDie = true;
            GetComponent<Collider2D>().enabled = false;

            PlayerHealth playerg = player.GetComponent<PlayerHealth>();
            if (playerg != null)
            {
                playerg.AddCoin(Random.Range(minscore, maxscore));
            }
        }
    }

    void Knockback(Vector2 hitDirection)
    {
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(hitDirection * knockbackStrength, ForceMode2D.Impulse);
        nextAttackTime = Time.time + hitStunAttackDelay;
        StopAllCoroutines();
        canMove = false;
        StartCoroutine(ResumeMovementAfterAttack());
    }

    void PlayGetHitSound() => AudioManager.Instance.PlaySoundGlobal(getHitSound);
    void PlayAttackSound() => AudioManager.Instance.PlaySoundGlobal(attackSound);
    void PlayDieSound() => AudioManager.Instance.PlaySoundGlobal(dieSound);
}
