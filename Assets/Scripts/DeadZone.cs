using System.Collections;
using UnityEngine;
using Unity.Cinemachine;

public class DeadZone : MonoBehaviour
{
    [Header("General Information")]
    public GameObject gameOver;

    [System.Obsolete]
    private void OnTriggerEnter2D(Collider2D other)
    {

        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();

            if (playerHealth != null)
            {
                playerHealth.TakeDamage(playerHealth.maxHealth, Vector2.zero, 0f);

                CinemachineBrain brain = Camera.main.GetComponent<CinemachineBrain>();
                if (brain != null)
                {
                    brain.enabled = false;
                }
            }
        }
    }
}
