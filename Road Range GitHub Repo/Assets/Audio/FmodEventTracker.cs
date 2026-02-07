using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using System;
using System.Collections.Generic;

public class FmodEventTracker : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private AudioStatus audioStatus;

    [Header("Tracked Parameters")]
    [SerializeField] private List<TrackedParameter> trackedParameters = new List<TrackedParameter>();

    [Serializable]
    public class TrackedParameter
    {
        public ParameterName parameter;
        public List<string> fmodEventPaths = new List<string>();

        [HideInInspector] public List<EventInstance> eventInstances = new List<EventInstance>();
        [HideInInspector] public bool wasTrueLastFrame = false;
    }

    public enum ParameterName
    {
        Boosting,
        Drifting,
        SlickMode
    }

    private void Start()
    {
        foreach (var tracked in trackedParameters)
        {
            tracked.eventInstances.Clear();

            foreach (var path in tracked.fmodEventPaths)
            {
                if (!string.IsNullOrEmpty(path))
                {
                    EventInstance instance = RuntimeManager.CreateInstance(path);
                    tracked.eventInstances.Add(instance);
                }
            }
        }
    }

    private void Update()
    {
        if (audioStatus == null)
            return;

        foreach (var tracked in trackedParameters)
        {
            bool currentValue = GetParameterValue(tracked.parameter);

            if (currentValue && !tracked.wasTrueLastFrame)
            {
                foreach (var instance in tracked.eventInstances)
                {
                    if (instance.isValid())
                        instance.start();
                }
            }
            else if (!currentValue && tracked.wasTrueLastFrame)
            {
                foreach (var instance in tracked.eventInstances)
                {
                    if (instance.isValid())
                        instance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                }
            }

            tracked.wasTrueLastFrame = currentValue;
        }
    }

    private bool GetParameterValue(ParameterName param)
    {
        switch (param)
        {
            case ParameterName.Boosting:
                return audioStatus.GetBoostingStatus();
            case ParameterName.Drifting:
                return audioStatus.GetDriftingStatus();
            case ParameterName.SlickMode:
                return audioStatus.GetSlickMode();
            default:
                return false;
        }
    }
}
