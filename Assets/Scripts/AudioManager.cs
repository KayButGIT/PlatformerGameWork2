using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    private AudioSource globalAudioSource;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            globalAudioSource = gameObject.AddComponent<AudioSource>();
            globalAudioSource.playOnAwake = false;
        }
        else
        {
            //Destroy(gameObject);
        }
    }

    void Start()
    {
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        Physics2D.IgnoreLayerCollision(enemyLayer, enemyLayer, true);
    }

    public void PlaySoundGlobal(AudioClip clip)
    {
        if (clip != null)
        {
            globalAudioSource.PlayOneShot(clip);
        }
    }
}
