using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Player Movement")]
    public float moveSpeed = 8f;
    public float maxSpeed = 10f;
    public float acceleration = 20f;
    public float deceleration = 15f;
    public Animator animator;

    private float moveInput;
    private Rigidbody2D rb;
    private bool canUse;
    private bool isFacingRight = true;

    [Header("Player Jump")]
    public float jumpForce = 15f;
    public float focusJumpForceMultiplier = 1.5f;
    public float fallMultiplier = 2.5f;
    public float lowJumpMultiplier = 2f;
    private bool isGrounded;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;
    public LayerMask wallLayer;

    [Header("Wall Check")]
    public Transform wallCheckPoint;
    public float wallCheckDistance = 0.5f;
    private bool isOnWall;

    [Header("Player Dash")]
    public float dashSpeed = 20f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;
    public float Invincibility = 1f;
    public int dashConsumeStamina = 50;
    private bool isDashing;
    private float dashTimeLeft;
    private float lastDashTime = -Mathf.Infinity;

    [Header("Player Attack")]
    public Transform attackPoint;
    public float attackRange = 0.5f;
    public int attackDamage = 1;
    public float attackDelay = 0.3f;
    public LayerMask enemyLayers;
    public bool isAttacking = false;

    [Header("Player SFX")]
    public AudioClip jumpSound;
    public AudioClip attackSound;
    public AudioClip dashSound;

    [Header("Player Integration")]
    public PlayerHealth playerHealth;
    public Skill skill;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        skill = GetComponent<Skill>();
    }

    void Update()
    {
        moveInput = Input.GetAxisRaw("Horizontal");
        CheckWall();

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            Jump();
        }

        if (rb.linearVelocity.y < 0)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
        }
        else if (rb.linearVelocity.y > 0 && !Input.GetKey(KeyCode.Space))
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
        }

        if (Input.GetKeyDown(KeyCode.Z) && Time.time > lastDashTime + dashCooldown && Mathf.Abs(moveInput) >= 0.01 && isGrounded)
        {
            canUse = playerHealth.UseDash(dashConsumeStamina);
            if (canUse)
            {
                StartDash();
            }
        }

        if (Input.GetKeyDown(KeyCode.X) && !isAttacking && isGrounded && !skill.isSkillActive)
        {
            Attack();
        }

        // Animator flags
        animator.SetFloat("Speed", Mathf.Abs(moveInput));
        animator.SetBool("IsGrounded", isGrounded);
        animator.SetBool("IsOnWall", isOnWall);
    }

    void FixedUpdate()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // Movement (disable while airborne & pressing into wall)
        if (!isDashing && !isAttacking &&
            !(isOnWall && !isGrounded &&
            ((moveInput > 0 && isFacingRight) || (moveInput < 0 && !isFacingRight))))
        {
            float targetSpeed = moveInput * moveSpeed;
            float speedDifference = targetSpeed - rb.linearVelocity.x;
            float accelerationRate = Mathf.Abs(targetSpeed) > 0.01f ? acceleration : deceleration;
            float movement = speedDifference * accelerationRate;
            rb.AddForce(movement * Vector2.right, ForceMode2D.Force);
        }

        // Flip sprite
        if (moveInput > 0 && !isFacingRight)
        {
            Flip();
        }
        else if (moveInput < 0 && isFacingRight)
        {
            Flip();
        }

        // Dashing logic
        if (isDashing)
        {
            if (dashTimeLeft > 0)
            {
                dashTimeLeft -= Time.fixedDeltaTime;
            }
            else
            {
                isDashing = false;
                animator.SetBool("IsDashing", isDashing);
            }
        }
    }

    void CheckWall()
    {
        Vector2 direction = isFacingRight ? Vector2.right : Vector2.left;
        RaycastHit2D wallHit = Physics2D.Raycast(wallCheckPoint.position, direction, wallCheckDistance, wallLayer);
        isOnWall = wallHit.collider != null;
    }

    void Jump()
    {
        animator.SetTrigger("Jump");
        PlayJumpSound();
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
    }

    void StartDash()
    {
        isDashing = true;
        animator.SetTrigger("IsDashing");
        dashTimeLeft = dashDuration;
        lastDashTime = Time.time;

        float dashDirection = isFacingRight ? 1 : -1;
        rb.linearVelocity = new Vector2(dashDirection * dashSpeed, 0);
        PlayDashSound();

        StartCoroutine(InvincibilityCoroutine(Invincibility));
    }

    IEnumerator InvincibilityCoroutine(float duration)
    {
        gameObject.layer = LayerMask.NameToLayer("Invincible");
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        Physics2D.IgnoreLayerCollision(gameObject.layer, enemyLayer, true);
        yield return new WaitForSeconds(duration);
        Physics2D.IgnoreLayerCollision(gameObject.layer, enemyLayer, false);
        gameObject.layer = LayerMask.NameToLayer("Player");
    }

    void Attack()
    {
        isAttacking = true;
        animator.SetTrigger("Attack");
        PlayAttackSound();
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        Vector2 attackPosition = attackPoint.position;
        Vector2 attackSize = new Vector2(attackRange, attackRange / 2);

        Collider2D[] hitEnemies = Physics2D.OverlapBoxAll(attackPosition, attackSize, 0, enemyLayers);

        foreach (Collider2D enemy in hitEnemies)
        {
            EnemyController enemyController = enemy.GetComponent<EnemyController>();
            if (enemyController != null)
            {
                enemyController.TakeDamage(attackDamage, (enemy.transform.position - rb.transform.position).normalized);
            }
        }

        Invoke("ResetAttack", attackDelay);
    }

    void ResetAttack()
    {
        isAttacking = false;
    }

    public void Flip()
    {
        isFacingRight = !isFacingRight;
        transform.Rotate(0f, 180f, 0f);
    }

    public void PlayJumpSound()
    {
        AudioManager.Instance.PlaySoundGlobal(jumpSound);
    }

    public void PlayAttackSound()
    {
        AudioManager.Instance.PlaySoundGlobal(attackSound);
    }

    public void PlayDashSound()
    {
        AudioManager.Instance.PlaySoundGlobal(dashSound);
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(attackPoint.position, new Vector3(attackRange, attackRange / 2, 1));
        }

        if (groundCheck != null)
        {
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }

        if (wallCheckPoint != null)
        {
            Gizmos.color = Color.green;
            Vector3 direction = isFacingRight ? Vector3.right : Vector3.left;
            Gizmos.DrawLine(wallCheckPoint.position, wallCheckPoint.position + direction * wallCheckDistance);
        }
    }
}
