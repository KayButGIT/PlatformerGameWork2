using UnityEngine;

public class HealZone : MonoBehaviour
{
    [Header("General Information")]    
    public int amountHeal = 20;
    public AudioClip healSound;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth player = other.GetComponent<PlayerHealth>();
            if (player != null)
            {
                player.Heal(amountHeal);
            }
            PlayHealSound();
            Destroy(gameObject);
        }
    }

    public void PlayHealSound()
    {
        AudioManager.Instance.PlaySoundGlobal(healSound);
    }

}
