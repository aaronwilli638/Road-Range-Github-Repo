using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using System.Collections.Generic;

public class FmodBoostTracker : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private AudioStatus audioStatus;

    [Header("FMOD Boost Event")]
    [SerializeField]
    private List<string> fmodEventPaths = new List<string>()
    {
        "event:/SFX/Boost"
    };

    private readonly List<EventInstance> eventInstances = new();
    private bool wasBoostingLastFrame = false;

    private void Start()
    {
        foreach (string path in fmodEventPaths)
        {
            if (string.IsNullOrEmpty(path))
                continue;

            EventInstance instance = RuntimeManager.CreateInstance(path);
            eventInstances.Add(instance);
        }
    }

    private void Update()
    {
        if (audioStatus == null || eventInstances.Count == 0)
            return;

        bool isBoosting = audioStatus.GetBoostingStatus();

        // Boost START
        if (isBoosting && !wasBoostingLastFrame)
        {
            foreach (EventInstance instance in eventInstances)
            {
                if (instance.isValid())
                    instance.start();
            }
        }
        // Boost STOP
        else if (!isBoosting && wasBoostingLastFrame)
        {
            foreach (EventInstance instance in eventInstances)
            {
                if (instance.isValid())
                    instance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            }
        }

        wasBoostingLastFrame = isBoosting;
    }
}