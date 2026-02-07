using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using System.Collections.Generic;

public class FmodParameterTracker : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private AudioStatus audioStatus;

    [Header("FMOD Events")] // What events parameters are being pulled from
    [SerializeField]
    private List<string> fmodEventPaths = new List<string>()
    {
        "event:/"
    };

    [Header("FMOD Parameters")] // Type name of parameter exactly how it appears in FMOD
    [SerializeField] private string speedParameter = "Car Speed";
    [SerializeField] private string driftingParameter = "Drifting";
    [SerializeField] private string distanceParameter = "Distance to Shredder";
    [SerializeField] private string slickModeParameter = "Slick Mode";
    [SerializeField] private string lapProgression = "Race Progression";
    [SerializeField] private string deathStatus = "PlayerDeathStatus";

    private readonly List<EventInstance> eventInstances = new();

    private void Start()
    {
        foreach (string path in fmodEventPaths)
        {
            if (string.IsNullOrEmpty(path))
                continue;

            EventInstance instance = RuntimeManager.CreateInstance(path);
            instance.start();
            eventInstances.Add(instance);
        }
    }

    private void Update()
    {
        if (audioStatus == null || eventInstances.Count == 0)
            return;

        // values from AudioStatus
        float speed = audioStatus.GetSpeed();
        float drifting = audioStatus.GetDriftingStatus() ? 1f : 0f;
        float distance = audioStatus.GetDistanceToShredder();
        float slickMode = audioStatus.GetSlickMode() ? 1f : 0f;
        float lap = audioStatus.GetLap();

        // push parameters to FMOD events
        foreach (EventInstance instance in eventInstances)
        {
            if (!instance.isValid())
                continue;

            instance.setParameterByName(speedParameter, speed);
            instance.setParameterByName(driftingParameter, drifting);
            instance.setParameterByName(distanceParameter, distance);
            instance.setParameterByName(slickModeParameter, slickMode);
            instance.setParameterByName(lapProgression, lap);
        }
    }
}
