using UnityEngine;

public class CarCamera2 : MonoBehaviour
{
    [Header("Targets")]
    public Transform target;
    public Rigidbody targetRb;
    public Transform shooterCameraMount;

    [Header("Settings")]
    public Vector3 offset = new Vector3(0, 5, -10);
    public float recenteringMultiplier = 2f;
    public float transitionSpeed = 5f;

    [Header("Shooting Settings")]
    public float shootingTransitionSpeed = 10f;

    [Header("Driving Follow Intensity")]
    public float driveLateralSpeed = 15f;
    public float driveLongitudinalSpeed = 40f;
    public float driveVerticalSpeed = 20f;
    public float driveRotationSpeed = 15f;

    [Header("Drifting Follow Intensity")]
    public float driftLateralSpeed = 5f;
    public float driftLongitudinalSpeed = 20f;
    public float driftVerticalSpeed = 10f;
    public float driftRotationSpeed = 5f;

    [Header("FOV Settings")]
    public float minFov = 60f;
    public float maxFov = 90f;
    public float fovSpeedCap = 80f;
    public float fovTransitionSpeed = 2f;

    private Car2 carController;
    private CarShooter carShooter;
    private Camera cam;

    private float currentLateralSpeed;
    private float currentLongitudinalSpeed;
    private float currentVerticalSpeed;
    private float currentRotationSpeed;

    private float shootBlend = 0f;

    void Start()
    {
        cam = GetComponent<Camera>();

        if (target != null)
        {
            carController = target.GetComponent<Car2>();
            carShooter = target.GetComponentInChildren<CarShooter>();
        }

        currentLateralSpeed = driveLateralSpeed;
        currentLongitudinalSpeed = driveLongitudinalSpeed;
        currentVerticalSpeed = driveVerticalSpeed;
        currentRotationSpeed = driveRotationSpeed;
    }

    void FixedUpdate()
    {
        if (!target) return;

        if (cam != null && targetRb != null)
        {
            float speed = targetRb.linearVelocity.magnitude;
            float t = Mathf.Clamp01(speed / fovSpeedCap);
            t = Mathf.SmoothStep(0f, 1f, t);
            
            float targetFov = Mathf.Lerp(minFov, maxFov, t);
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFov, fovTransitionSpeed * Time.fixedDeltaTime);
        }

        float driftFactor = carController != null ? carController.DriftFactor : 0f;

        float targetLateral = Mathf.Lerp(driveLateralSpeed, driftLateralSpeed, driftFactor);
        float targetLongitudinal = Mathf.Lerp(driveLongitudinalSpeed, driftLongitudinalSpeed, driftFactor);
        float targetVertical = Mathf.Lerp(driveVerticalSpeed, driftVerticalSpeed, driftFactor);
        float targetRotationSpeed = Mathf.Lerp(driveRotationSpeed, driftRotationSpeed, driftFactor);

        if (carController != null && Mathf.Abs(carController.SteerInput) <= 0.1f)
        {
            targetLateral *= recenteringMultiplier;
            targetRotationSpeed *= recenteringMultiplier;
        }

        currentLateralSpeed = Mathf.Lerp(currentLateralSpeed, targetLateral, transitionSpeed * Time.fixedDeltaTime);
        currentLongitudinalSpeed = Mathf.Lerp(currentLongitudinalSpeed, targetLongitudinal, transitionSpeed * Time.fixedDeltaTime);
        currentVerticalSpeed = Mathf.Lerp(currentVerticalSpeed, targetVertical, transitionSpeed * Time.fixedDeltaTime);
        currentRotationSpeed = Mathf.Lerp(currentRotationSpeed, targetRotationSpeed, transitionSpeed * Time.fixedDeltaTime);

        Vector3 targetWorldPos = target.TransformPoint(offset);
        Vector3 worldError = targetWorldPos - transform.position;
        Vector3 localError = target.InverseTransformDirection(worldError);

        float xStep = localError.x * Mathf.Clamp01(currentLateralSpeed * Time.fixedDeltaTime);
        float yStep = localError.y * Mathf.Clamp01(currentVerticalSpeed * Time.fixedDeltaTime);
        float zStep = localError.z * Mathf.Clamp01(currentLongitudinalSpeed * Time.fixedDeltaTime);

        Vector3 drivePos = transform.position + target.TransformDirection(new Vector3(xStep, yStep, zStep));

        Vector3 lookDirection;
        if (targetRb != null && targetRb.linearVelocity.magnitude > 2f)
        {
            Vector3 velocityHeading = targetRb.linearVelocity.normalized;
            Vector3 facingHeading = target.forward;
            lookDirection = Vector3.Lerp(facingHeading, velocityHeading, 0.3f);
        }
        else
        {
            lookDirection = target.forward;
        }

        Quaternion driveRot = Quaternion.LookRotation(lookDirection, Vector3.up);
        driveRot = Quaternion.Slerp(transform.rotation, driveRot, currentRotationSpeed * Time.fixedDeltaTime);

        bool isShooting = carShooter != null && carShooter.IsAiming && shooterCameraMount != null;

        shootBlend = Mathf.MoveTowards(shootBlend, isShooting ? 1f : 0f, shootingTransitionSpeed * Time.fixedDeltaTime);

        if (shootBlend <= 0.001f)
        {
            transform.position = drivePos;
            transform.rotation = driveRot;
        }
        else if (shootBlend >= 0.999f)
        {
            transform.position = shooterCameraMount.position;
            transform.rotation = carShooter.AimRotation;
        }
        else
        {
            transform.position = Vector3.Lerp(drivePos, shooterCameraMount.position, shootBlend);
            transform.rotation = Quaternion.Slerp(driveRot, carShooter.AimRotation, shootBlend);
        }
    }
}