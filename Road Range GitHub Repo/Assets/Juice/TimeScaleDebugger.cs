using UnityEngine;

public class TimeScaleDebugger : MonoBehaviour
{
    [Range(0f, 1f)]
    [SerializeField]
    private float currentTimeScale = 1f;

    private void Update()
    {
        Time.timeScale = currentTimeScale;
        Time.fixedDeltaTime = 0.02f * Mathf.Max(currentTimeScale, 0.0001f);
    }
}