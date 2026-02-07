using UnityEngine;

public class DestroyOnContact : MonoBehaviour
{
    [SerializeField] private LayerMask targetLayer;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private GameObject deathFxPrefab;
    [SerializeField] private SmoothShakeFree.SmoothShake cameraShake;
    [SerializeField] private float fxLifetime = 2f;
    [SerializeField] private float stopDuration = 0.1f;
    
    [Header("Shredder Interaction")]
    [SerializeField] private float pushBackAmount = 5f;

    private void OnCollisionEnter(Collision collision)
    {
        if (((1 << collision.gameObject.layer) & playerLayer) != 0)
        {
            if (collision.gameObject.GetComponent<SmoothCar>().IsBoostingBuffered)
            {
                ShredderController shredder = FindFirstObjectByType<ShredderController>();
                if (shredder != null)
                {
                    shredder.PushBack(pushBackAmount);
                    Debug.Log($"Killed enemy! Pushed shredder back by {pushBackAmount}m");
                }

                SpawnEffects();
                if (cameraShake != null) cameraShake.StartShake();
                TimeManager.Instance.RequestFreeze(stopDuration);
                gameObject.SetActive(false);
            }
        }
        else if (((1 << collision.gameObject.layer) & targetLayer) != 0)
        {
            SpawnEffects();
            gameObject.SetActive(false);
        }
    }

    private void SpawnEffects()
    {
        GameObject fx = Instantiate(deathFxPrefab, transform.position, Quaternion.identity);
        
        foreach (var p in fx.GetComponentsInChildren<ParticleSystem>())
        {
            var main = p.main;
            main.useUnscaledTime = true;
        }

        Destroy(fx, fxLifetime);
    }
}