using System.Collections;
using TMPro;
using UnityEngine;

public class GameClearTrigger : MonoBehaviour
{
    [Header("UI")]
    public GameObject gameClear;
    public TextMeshProUGUI score;
    public TextMeshProUGUI survive;

    [Header("References")]
    public PlayerHealth playerHealth;

    private float startTime;
    private bool gameCleared = false;

    void Start()
    {
        startTime = Time.time;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (gameCleared) return;

        if (collision.CompareTag("Player"))
        {
            gameCleared = true;
            TriggerGameClear();
        }
    }

    void DisableAllControllers()
    {
        foreach (EnemyController enemy in FindObjectsOfType<EnemyController>())
            enemy.enabled = false;

        foreach (PlayerController player in FindObjectsOfType<PlayerController>())
            player.enabled = false;
    }


    void TriggerGameClear()
    {
        float timeSurvived = Time.time - startTime;

        //Time Display
        int minutes = Mathf.FloorToInt(timeSurvived / 60);
        int seconds = Mathf.FloorToInt(timeSurvived % 60);
        survive.text = string.Format("{0:D2} M {1:D2} S", minutes, seconds);

        // Stop player
        Rigidbody2D rb = playerHealth.GetComponent<Rigidbody2D>();
        if (rb)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }

        // Freeze everything
        DisableAllControllers();


        // Play clear sound
        BackgroundSound bgSound = FindObjectOfType<BackgroundSound>();
        if (bgSound != null)
        {
            bgSound.PlayGameClearSound();
        }

        // Show UI after delay
        StartCoroutine(EnableUIAfterDelay(0.2f));
    }

    IEnumerator EnableUIAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        gameClear.SetActive(true);

        int currentScore = playerHealth.CurrentScore;
        StartCoroutine(AnimateScoreDisplay(currentScore));
    }

    IEnumerator AnimateScoreDisplay(int targetScore)
    {
        string targetScoreString = targetScore.ToString("D4");
        score.text = "0000";

        for (int i = 0; i < targetScoreString.Length; i++)
        {
            string displayedScore =
                targetScoreString.Substring(0, i + 1).PadRight(4, '0');

            score.text = displayedScore;
            yield return new WaitForSeconds(0.2f);
        }

        score.text = targetScoreString;
    }
}