using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill : MonoBehaviour
{
    [Header("Skill General")]
    public float skillSpeed = 25f;
    public float skillDuration = 0.15f;
    public float skillCooldown = 2f;
    public int skillDamage = 2;
    public int skillConsumeStamina = 50;
    public float skillRadius = 1.5f;
    public float skillOffset = 1f;
    public float Invincibility = 1f;
    public GameObject skillDestinationPoint;

    [Header("Ground Check")]
    private bool isGrounded;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    [Header("Enemies Handler")]
    public LayerMask enemyLayers;


    [Header("Player Intergration")]
    public PlayerHealth playerHealth;
    public PlayerController playerController;

    [Header("Sound")]
    public AudioClip skillSound;

    [Header("Animatation")]
    public Animator animator;

    [Header("Attack Control")]
    private bool canAttack = true;
    public float skillDelay = 20f;

    public bool isSkillActive = false;
    private float skillTimeLeft;
    private float lastSkillTime = -Mathf.Infinity;
    private HashSet<Collider2D> enemiesHitDuringSkillDash = new HashSet<Collider2D>();

    private Rigidbody2D rb;
    private bool canUse;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerHealth = GetComponent<PlayerHealth>();
        playerController = GetComponent<PlayerController>();
    }

    [System.Obsolete]
    void Update()
    {
        if (!playerController.isAttacking)
        {
            if (Input.GetKeyDown(KeyCode.C) && Time.time > lastSkillTime + skillCooldown && isGrounded)
            {
                canUse = playerHealth.UseDash(skillConsumeStamina);
                if (canUse)
                {
                    StartSkillDash();
                }
            }

            if (isSkillActive)
            {
                if (skillTimeLeft > 0)
                {
                    skillTimeLeft -= Time.fixedDeltaTime;
                    SkillHitEnemies();
                }
                else
                {
                    isSkillActive = false;
                }
            }
        }
    }

    void FixedUpdate()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

    }

    [System.Obsolete]
    void StartSkillDash()
    {
        isSkillActive = true;
        skillTimeLeft = skillDuration;
        lastSkillTime = Time.time;
        animator.SetTrigger("IsSkill");

        enemiesHitDuringSkillDash.Clear();
        float moveDirection = (skillDestinationPoint.transform.position.x < transform.position.x) ? -1f : 1f;
        rb.velocity = new Vector2(moveDirection * skillSpeed, rb.velocity.y);

        PlaySkillSound();
        StartCoroutine(InvincibilityCoroutine(Invincibility));

        canAttack = false;
        StartCoroutine(EnableAttackAfterCooldown(skillDelay));
    }

    [System.Obsolete]
    void SkillHitEnemies()
    {
        Vector2 forward = transform.right * Mathf.Sign(transform.localScale.x);
        Vector2 center = (Vector2)transform.position + forward * skillOffset;

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(center, skillRadius, enemyLayers);

        foreach (Collider2D enemyCollider in hitEnemies)
        {
            if (!enemiesHitDuringSkillDash.Contains(enemyCollider))
            {
                enemiesHitDuringSkillDash.Add(enemyCollider);

                EnemyController enemyController = enemyCollider.gameObject.GetComponent<EnemyController>();
                if (enemyController != null)
                {
                    enemyController.TakeDamage(skillDamage, (enemyCollider.transform.position - transform.position).normalized);
                }

            }
        }
    }

    IEnumerator InvincibilityCoroutine(float duration)
    {
        gameObject.layer = LayerMask.NameToLayer("Invincible");
        yield return new WaitForSeconds(duration);
        gameObject.layer = LayerMask.NameToLayer("Player");
    }

    public void PlaySkillSound()
    {
        AudioManager.Instance.PlaySoundGlobal(skillSound);
    }

    IEnumerator EnableAttackAfterCooldown(float duration)
    {
        yield return new WaitForSeconds(duration);
        canAttack = true;
    }

    public bool CanAttack()
    {
        return canAttack;
    }

}
