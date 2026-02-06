using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class FmodRaceEventsStarter : MonoBehaviour
{
    [Header("FMOD Events to Play On Start")]
    [SerializeField] private EventReference[] eventsToPlay;

    [Header("Settings")]
    [SerializeField] private bool attachToGameObject = true;

    private void Start()
    {
        foreach (var eventRef in eventsToPlay)
        {
            if (eventRef.IsNull)
                continue;

            if (attachToGameObject)
            {
                EventInstance instance = RuntimeManager.CreateInstance(eventRef);
                RuntimeManager.AttachInstanceToGameObject(instance, transform, GetComponent<Rigidbody>());
                instance.start();
                instance.release();
            }
            else
            {
                RuntimeManager.PlayOneShot(eventRef, transform.position);
            }
        }
    }
}