using UnityEngine;

public class BossBarrierDestroyer : MonoBehaviour
{
    [Header("Barrier To Destroy")]
    public GameObject barrier;

    private Animator animator;
    private bool barrierDestroyed = false;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (barrierDestroyed) return;

        // Check if boss is dead via animator bool
        if (animator.GetBool("IsDie"))
        {
            DestroyBarrier();
        }
    }

    void DestroyBarrier()
    {
        barrierDestroyed = true;

        if (barrier != null)
        {
            Destroy(barrier);
        }
    }
}
