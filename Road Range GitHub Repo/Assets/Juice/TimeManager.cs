using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance;
    private float unfreezeTime;

    private void Awake()
    {
        Instance = this;
    }

    public void RequestFreeze(float duration)
    {
        unfreezeTime = Mathf.Max(unfreezeTime, Time.unscaledTime + duration);
    }

    private void Update()
    {
        Time.timeScale = Time.unscaledTime < unfreezeTime ? 0f : 1f;
    }
}