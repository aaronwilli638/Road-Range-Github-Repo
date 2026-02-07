using UnityEngine;
using FMODUnity; //in order for Fmod stuff to work I added this
using System.Collections.Generic; // multikill tracking with this

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

    [Header("FMOD")] //calling the fmod event
    private string explosionEvent = "event:/SFX/Explosions";
    [SerializeField] private Vector3 audioOffset = Vector3.zero;

    [Header("Multikill Settings")]
    [SerializeField] private float multikillWindow = 3f;
    [SerializeField] private int killsRequired = 3;
    [SerializeField] private string multikillEvent = "event:/VA/Player Muiltikill";
    [SerializeField] private float multikillCooldown = 5f;

    private static List<float> killTimestamps = new List<float>(); // Logic for calculating multikills
    private static float lastMultikillTime = -Mathf.Infinity; // Tracks last time announcer played

    private void RegisterPlayerKill()
    {
        float now = Time.time;
        killTimestamps.Add(now);

        killTimestamps.RemoveAll(t => now - t > multikillWindow);

        if (killTimestamps.Count >= killsRequired && now - lastMultikillTime >= multikillCooldown)
        {
            Debug.Log("Multikill Triggered!");

            RuntimeManager.PlayOneShot(multikillEvent);

            lastMultikillTime = now;

            killTimestamps.RemoveAt(0);
        }
    }

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

                    RegisterPlayerKill(); //for fmod event playing
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