using UnityEngine;

public class FrameLimiter : MonoBehaviour
{
    public int targetFPS = 60;
    public bool disableVSync = true;

    void Awake()
    {
        QualitySettings.vSyncCount = disableVSync ? 0 : 1;
        Application.targetFrameRate = targetFPS;
    }
}