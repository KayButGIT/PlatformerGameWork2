using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class GameOver : MonoBehaviour
{
    public enum GameState { GameOver, GameClear }
    public GameState currentGameState;
    public TextMeshProUGUI retryText;
    public float blinkInterval = 1f;
    
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI scoreText;


    private void OnEnable()
    {
        StartCoroutine(BlinkRetryText());
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            HandleInputAction();
        }
    }

    void HandleInputAction()
    {
        switch (currentGameState)
        {
            case GameState.GameOver:
                ReloadScene();
                break;
            case GameState.GameClear:
                QuitGame();
                break;
        }
    }
    
    public void TriggerGameClear(float clearTime, int score)
    {
        currentGameState = GameState.GameClear;
        
        int minutes = Mathf.FloorToInt(clearTime / 60f);
        int seconds = Mathf.FloorToInt(clearTime % 60f);

        timeText.text = $"Time: {minutes:00}:{seconds:00}";
        scoreText.text = $"Score: {score}";

        Time.timeScale = 0f;
        gameObject.SetActive(true);
    }


    void ReloadScene()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentScene);
    }

    void QuitGame()
    {
        Application.Quit();
        Debug.Log("Game Closed!");

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    IEnumerator BlinkRetryText()
    {
        Color originalColor = retryText.color;
        while (true) // Infinite loop until scene reloads
        {
            retryText.color = Color.white;
            yield return new WaitForSeconds(blinkInterval);
            retryText.color = Color.grey;
            yield return new WaitForSeconds(blinkInterval);
        }
    }
}
