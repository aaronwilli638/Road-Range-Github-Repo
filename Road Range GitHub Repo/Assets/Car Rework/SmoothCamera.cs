using UnityEngine;

public class SmoothCamera : MonoBehaviour
{
    [Header("Targets")]
    public Transform target;
    public Rigidbody targetRb;

    [Header("Settings")]
    public Vector3 offset = new Vector3(0, 5, -10);
    public float recenteringMultiplier = 0.5f; 
    public float transitionSpeed = 5f;

    [Header("Follow Stiffness")]
    public float driveLateralStiffness = 15f;
    public float driveLongitudinalStiffness = 40f;
    public float driveVerticalStiffness = 20f;
    public float driveRotationStiffness = 15f;

    [Header("Drifting Stiffness")]
    public float driftLateralStiffness = 5f;
    public float driftLongitudinalStiffness = 20f;
    public float driftVerticalStiffness = 10f;
    public float driftRotationStiffness = 5f;

    [Header("FOV Settings")]
    public float minFov = 60f;
    public float maxFov = 90f;
    public float fovSpeedCap = 80f;
    public float fovTransitionSpeed = 2f;

    private SmoothCar carController;
    private Camera cam;

    private float currentLatTime;
    private float currentLongTime;
    private float currentVertTime;
    private float currentRotTime;

    private Vector3 currentLocalVelocity; 
    private Quaternion lastTargetRot;
    
    private Vector3 lastTargetPos;

    void Start()
    {
        cam = GetComponent<Camera>();

        if (target != null)
        {
            carController = target.GetComponent<SmoothCar>();
            targetRb = target.GetComponent<Rigidbody>();
            
            Vector3 targetPos = target.TransformPoint(offset);
            transform.position = targetPos;
            transform.rotation = target.rotation;
            lastTargetRot = target.rotation;
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

        if (cam != null)
        {
            float t = Mathf.Clamp01(speed / fovSpeedCap);
            t = Mathf.SmoothStep(0f, 1f, t);
            float targetFov = Mathf.Lerp(minFov, maxFov, t);
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFov, fovTransitionSpeed * dt);
        }

        float driftFactor = carController != null ? carController.DriftFactor : 0f;

        float targetLatStiff = Mathf.Lerp(driveLateralStiffness, driftLateralStiffness, driftFactor);
        float targetLongStiff = Mathf.Lerp(driveLongitudinalStiffness, driftLongitudinalStiffness, driftFactor);
        float targetVertStiff = Mathf.Lerp(driveVerticalStiffness, driftVerticalStiffness, driftFactor);
        float targetRotStiff = Mathf.Lerp(driveRotationStiffness, driftRotationStiffness, driftFactor);

        if (carController != null && Mathf.Abs(carController.SteerInput) <= 0.1f)
        {
            targetLatStiff *= (1f / recenteringMultiplier); 
            targetRotStiff *= (1f / recenteringMultiplier);
        }

        float targetLatTime = 1f / Mathf.Max(0.1f, targetLatStiff);
        float targetLongTime = 1f / Mathf.Max(0.1f, targetLongStiff);
        float targetVertTime = 1f / Mathf.Max(0.1f, targetVertStiff);
        float targetRotTime = 1f / Mathf.Max(0.1f, targetRotStiff);

        currentLatTime = Mathf.Lerp(currentLatTime, targetLatTime, transitionSpeed * dt);
        currentLongTime = Mathf.Lerp(currentLongTime, targetLongTime, transitionSpeed * dt);
        currentVertTime = Mathf.Lerp(currentVertTime, targetVertTime, transitionSpeed * dt);
        currentRotTime = Mathf.Lerp(currentRotTime, targetRotTime, transitionSpeed * dt);

        Vector3 targetWorldPos = target.TransformPoint(offset);
        
        Vector3 currentLocalPos = target.InverseTransformPoint(transform.position);
        Vector3 targetLocalPos = target.InverseTransformPoint(targetWorldPos);

        Quaternion deltaRot = Quaternion.Inverse(lastTargetRot) * target.rotation;
        currentLocalVelocity = Quaternion.Inverse(deltaRot) * currentLocalVelocity;
        lastTargetRot = target.rotation;

        float newX = Mathf.SmoothDamp(currentLocalPos.x, targetLocalPos.x, ref currentLocalVelocity.x, currentLatTime);
        float newY = Mathf.SmoothDamp(currentLocalPos.y, targetLocalPos.y, ref currentLocalVelocity.y, currentVertTime);
        float newZ = Mathf.SmoothDamp(currentLocalPos.z, targetLocalPos.z, ref currentLocalVelocity.z, currentLongTime);

        transform.position = target.TransformPoint(new Vector3(newX, newY, newZ));

        Vector3 lookDirection;
        if (speed > 2f)
        {
            Vector3 velocityHeading = smoothVelocity.normalized;
            Vector3 facingHeading = target.forward;
            lookDirection = Vector3.Lerp(facingHeading, velocityHeading, 0.3f);
        }
        else
        {
            lookDirection = target.forward;
        }

        Quaternion targetRot = Quaternion.LookRotation(lookDirection, Vector3.up);
        
        float rotSpeed = 1f / Mathf.Max(0.01f, currentRotTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotSpeed * dt);
    }
}