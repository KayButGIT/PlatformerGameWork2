using UnityEngine;

public class Trap : MonoBehaviour
{
    public int damage = 1;
    public float knockbackStrength = 8f;
    public float hitCooldown = 1.5f;

    float lastHitTime = -Mathf.Infinity;

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryHit(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryHit(other);
    }

    void TryHit(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (Time.time < lastHitTime + hitCooldown) return;

        lastHitTime = Time.time;

        PlayerHealth player = other.GetComponent<PlayerHealth>();
        if (player != null)
        {
            Vector2 direction = (other.transform.position - transform.position).normalized;
            player.TakeDamage(damage, direction, knockbackStrength);
        }
    }
}
