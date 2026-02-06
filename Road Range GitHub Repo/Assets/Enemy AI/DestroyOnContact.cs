using UnityEngine;
using FMODUnity;

public class DestroyOnContact : MonoBehaviour
{
    [SerializeField] private LayerMask targetLayer;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private GameObject deathFxPrefab;
    [SerializeField] private SmoothShakeFree.SmoothShake cameraShake;
    [SerializeField] private float fxLifetime = 2f;
    [SerializeField] private float stopDuration = 0.1f;

    [Header("FMOD")]
    [SerializeField] private string explosionEvent = "event:/SFX/Explosions";
    [SerializeField] private Vector3 audioOffset = Vector3.zero;

    private void OnCollisionEnter(Collision collision)
    {
        if (((1 << collision.gameObject.layer) & playerLayer) != 0)
        {
            if (collision.gameObject.GetComponent<SmoothCar>().IsBoostingBuffered)
            {
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

        // added FMOD explosion logic
        if (!string.IsNullOrEmpty(explosionEvent))
        {
            RuntimeManager.PlayOneShot(
                explosionEvent,
                transform.position + audioOffset
            );
        }

        Destroy(fx, fxLifetime);
    }
}
