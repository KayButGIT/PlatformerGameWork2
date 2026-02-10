using UnityEngine;

public class Coin : MonoBehaviour
{
    [Header("General Information")]    
    public int minscore = 20;
    public int maxscore = 25;
    public AudioClip coinSound;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth player = other.GetComponent<PlayerHealth>();
            if (player != null)
            {
                player.AddCoin(Random.Range(minscore, maxscore));
            }
            PlayCoinSound();
            Destroy(gameObject);
        }
    }

    public void PlayCoinSound()
    {
        AudioManager.Instance.PlaySoundGlobal(coinSound);
    }

}
