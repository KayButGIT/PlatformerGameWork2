using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Player Status")] 
    public int maxHealth = 5;
    public int maxStamina = 150;
    public float staminaRegenCooldown = 1f;
    public int staminaRegenRate = 25;

    // Private Value
    private int currentHealth;
    private int currentStamina;
    private int currentScore;
    private float currentStaminaRegenCooldown;
    private Rigidbody2D rb;
    
    public int CurrentScore => currentScore;

    [Header("Player SFX")] 
    public AudioClip getHitSound;
    public AudioClip getHitDieSound;

    [Header("Player UI")] 
    public Slider staminaBar;
    public TextMeshProUGUI score;
    public GameObject gameOver;

    [Header("Player Animator")]
    public Animator animator;
    public Animator animatorHealthAvatar;
    public Sprite[] healthbars;
    public Image healthBar;

    void Start()
    {
        currentHealth = maxHealth;
        currentStamina = maxStamina;
        UpdateUI();
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        RegenerateStamina();
    }

    [System.Obsolete]
    public void TakeDamage(int damage, Vector2 hitDirection, float knockbackStrength)
    {
        if (currentHealth <= 0) return; // Already dead

        currentHealth -= damage;
        UpdateUI();

        if (currentHealth <= 0)
        {
            // Skip hit animations and knockback, just die immediately
            Die();
            return;
        }

        // Still alive - play hit feedback
        PlayGetHitSound();
        animator.SetTrigger("IsHit");
        animatorHealthAvatar.SetTrigger("IsHit");
        Knockback(hitDirection, knockbackStrength);
    }

    private void Knockback(Vector2 hitDirection, float knockbackStrength)
    {
        rb.AddForce(hitDirection.normalized * knockbackStrength, ForceMode2D.Impulse);
    }

    public (int health, int score) CheckPlayerStatus()
    {
        return (currentHealth, currentScore);
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        Debug.Log("Player healed! Current health: " + currentHealth);
        UpdateUI();
    }

    public void AddCoin(int amount)
    {
        currentScore += amount;
        UpdateUI();
    }

    public bool UseDash(int consumeStamina)
    {
        if(currentStamina >= consumeStamina){
            currentStamina -= consumeStamina;
            UpdateUI();
            return true;
        }else{
            return false;
        }
    }

    void RegenerateStamina()
    {
        if (currentStamina < maxStamina)
        {
            currentStaminaRegenCooldown += Time.deltaTime;
            if (currentStaminaRegenCooldown > staminaRegenCooldown)
            {
                currentStaminaRegenCooldown = 0f;
                currentStamina = Mathf.Min(currentStamina + staminaRegenRate, maxStamina);
                UpdateUI();
            }
        }
    }

    [System.Obsolete]
    void Die()
    {
        Debug.Log("Player Died!");
        animatorHealthAvatar.SetBool("IsDead", true);
        animator.SetBool("IsDead", true);
        DisableAllControllers();

        GetComponent<BackgroundSound>().PlayGameOverSound();

        GameObject Background = GameObject.Find("Mid_Background");
        GameObject SubBackground = GameObject.Find("Sub_Background");
        GameObject Prob = GameObject.Find("Probs");
        GameObject Torch = GameObject.Find("Torches");

        if (Background != null)
            Background.SetActive(false);

        if (SubBackground != null)
            SubBackground.SetActive(false);
        
        if (Prob != null)
            Prob.SetActive(false);

        if (Torch != null)
            Torch.SetActive(false);

        ChangeLayerColor("Default", Color.red * 100f);
        ChangeLayerColor("Background", Color.red * 100f);
        ChangeLayerColor("Player", Color.black);
        ChangeLayerColor("Ground", Color.black);
        ChangeLayerColor("Enemy", Color.black);
        ChangeLayerColor("Wall", Color.black);

        PlayGetHitDieSound();
        StartCoroutine(EnableUIAfterDelay(2f));
    }

    [System.Obsolete]
    void DisableAllControllers()
    {
        foreach (EnemyController enemy in FindObjectsOfType<EnemyController>())
        {
            enemy.enabled = false;
            enemy.GetComponent<Animator>().enabled = false;
        }

        foreach (PlayerController player in FindObjectsOfType<PlayerController>())
        {
            player.enabled = false;
        }

        foreach (Coin coin in FindObjectsOfType<Coin>())
        {
            coin.GetComponent<Animator>().enabled = false;
        }

        foreach (HealZone heal in FindObjectsOfType<HealZone>())
        {
            heal.GetComponent<Animator>().enabled = false;
        }

        foreach (Animator anim in FindObjectsOfType<Animator>())
        {
            RuntimeAnimatorController controller = anim.runtimeAnimatorController;
            if (controller != null && controller.name == "Torch")
            {
                anim.enabled = false;
            }
        }

    }

    IEnumerator EnableUIAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (gameOver != null)
        {
            gameOver.SetActive(true);
        }
    }

    [System.Obsolete]
    public void ChangeLayerColor(string layerName, Color color)
    {
        int layer = LayerMask.NameToLayer(layerName);
        GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();

        foreach (var obj in allObjects)
        {
            if (obj.layer == layer)
            {
                var renderer = obj.GetComponent<Renderer>();
                if (renderer != null)
                {
                    // Create a new material instance if not already changed
                    if (!Application.isEditor || renderer.sharedMaterial == renderer.material)
                    {
                        renderer.material = new Material(renderer.material);
                    }

                    renderer.material.color = color;
                }
            }
        }
    }



    void UpdateUI()
    {
        if (staminaBar != null)
        {
            staminaBar.value = currentStamina;
        }

        if (score != null)
        {
            score.text = currentScore.ToString();
        }

        if (animatorHealthAvatar != null)
        {
            animatorHealthAvatar.SetInteger("Health", currentHealth);
        }

        if (healthbars != null && healthBar != null && currentHealth >= 0 && currentHealth < healthbars.Length)
        {
            healthBar.sprite = healthbars[currentHealth];
        }

    }

    public void PlayGetHitSound()
    {
        AudioManager.Instance.PlaySoundGlobal(getHitSound);
    }

    public void PlayGetHitDieSound()
    {
        AudioManager.Instance.PlaySoundGlobal(getHitDieSound);
    }

}
