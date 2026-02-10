using UnityEngine;
using UnityEngine.UI;

public class BackgroundSound : MonoBehaviour
{
    [Header("General Information")]
    public AudioSource mainSound;
    public AudioClip normal;
    public AudioClip gameOver;
    public AudioClip gameClear;

    public PlayerHealth player;

    void Start()
    {
        player = GetComponent<PlayerHealth>();
        
        PlayNormalSound();
    }

    private void PlayNormalSound()
    {
        mainSound.Stop();
        mainSound.clip = normal;
        mainSound.Play();
    }

    public void PlayGameOverSound()
    {
        mainSound.Stop();
        mainSound.clip = gameOver;
        mainSound.Play();
    }

    public void PlayGameClearSound()
    {
        mainSound.Stop();
        mainSound.clip = gameClear;
        mainSound.loop = false;
        mainSound.Play();
    }
}
