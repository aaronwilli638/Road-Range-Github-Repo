using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MatchManager : MonoBehaviour
{
    public static MatchManager Instance;

    public int totalLaps = 3;
    public float startFreezeDuration = 3f;
    
    public List<float> lapTimes = new List<float>();
    public float totalRaceTime;

    private void Awake()
    {
        Instance = this;
    }

    private IEnumerator Start()
    {
        yield return new WaitForEndOfFrame();

        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.RequestFreeze(startFreezeDuration);
        }
    }

    public void RecordLap(float time)
    {
        lapTimes.Add(time);
        totalRaceTime += time;

        if (lapTimes.Count >= totalLaps)
        {
            TimeManager.Instance.RequestFreeze(99999f);
        }
    }
}