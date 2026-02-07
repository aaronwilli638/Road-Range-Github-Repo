using UnityEngine;

[RequireComponent(typeof(Camera))]
public class SmoothFOV : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("FOV Settings")]
    public float minFov = 60f;
    public float maxFov = 90f;
    public float fovSpeedCap = 80f;
    public float fovTransitionSpeed = 2f;

    private Camera cam;
    private Vector3 lastTargetPos;

    void Start()
    {
        cam = GetComponent<Camera>();
        
        if (target != null)
        {
            lastTargetPos = target.position;
        }
    }

    void LateUpdate()
    {
        if (!target) return;

        float dt = Time.deltaTime;
        if (dt < 0.0001f) return;

        Vector3 smoothVelocity = (target.position - lastTargetPos) / dt;
        lastTargetPos = target.position;
        float speed = smoothVelocity.magnitude;

        float t = Mathf.Clamp01(speed / fovSpeedCap);
        t = Mathf.SmoothStep(0f, 1f, t);
        float targetFov = Mathf.Lerp(minFov, maxFov, t);
        
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFov, fovTransitionSpeed * dt);
    }
}