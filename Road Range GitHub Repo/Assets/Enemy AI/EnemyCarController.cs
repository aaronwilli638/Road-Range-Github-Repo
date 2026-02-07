using UnityEngine;
using FMODUnity; 
using FMOD.Studio; //needed for fmod commands

[RequireComponent(typeof(Rigidbody))]
public class EnemyCarController : MonoBehaviour
{
    public SmoothCar playerReference;
    public float steerMultiplier = 1.0f;

    [Header("Hover Physics")]
    public float hoverHeight = 2.0f;
    public float hoverDamping = 10f;
    public float rotationSmoothing = 5f;
    public LayerMask groundLayer;

    [Header("FMOD")] //finding the fmod event to trigger
    private string engineEvent = "event:/SFX/Enemy Engine";

    private Rigidbody rb;
    private float driveAcceleration;
    private float driveTurnSpeed;
    private float inputSteer;
    private float inputThrottle;

    private EventInstance engineInstance; //stores engine sound so it can be stopped smoothly

    public Vector3 Forward => -transform.forward;
    public Vector3 Right => -transform.right;
    public float CurrentSpeed => rb.linearVelocity.magnitude;

    public float MaxSpeed
    {
        get
        {
            if (rb.linearDamping <= 0) return driveAcceleration;
            return driveAcceleration / rb.linearDamping;
        }
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.useGravity = false;
        rb.maxAngularVelocity = 20f;
        rb.angularDamping = 5f;
    }

    private void OnEnable() //starts sound when activates, and attaches the 3d event to the rigidbofy
    {
        if (string.IsNullOrEmpty(engineEvent))
            return;

        engineInstance = RuntimeManager.CreateInstance(engineEvent);
        RuntimeManager.AttachInstanceToGameObject(engineInstance, transform, rb);
        engineInstance.start();
    }

    private void OnDisable() //fmod stop engine sound when car blows up
    {
        if (!engineInstance.isValid())
            return;

        engineInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        engineInstance.release();
    }

    private void Start()
    {
        if (playerReference != null)
        {
            rb.linearDamping = playerReference.driveDrag;
            driveTurnSpeed = playerReference.driveTurnSpeed * steerMultiplier;
            driveAcceleration = playerReference.driveAcceleration;
        }
    }

    public void SetInputs(float steer, float throttle)
    {
        inputSteer = Mathf.Clamp(steer, -1f, 1f);
        inputThrottle = Mathf.Clamp(throttle, -1f, 1f);
    }

    private void FixedUpdate()
    {
        Quaternion currentRotation = rb.rotation;
        Quaternion nextRotation = currentRotation;

        Ray ray = new Ray(transform.position, -transform.up);
        if (Physics.Raycast(ray, out RaycastHit hit, hoverHeight + 5f, groundLayer))
        {
            Vector3 targetPosition = hit.point + (hit.normal * hoverHeight);
            Vector3 smoothedPosition = Vector3.Lerp(rb.position, targetPosition, Time.fixedDeltaTime * hoverDamping);
            rb.MovePosition(smoothedPosition);

            Quaternion targetAlign = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;
            nextRotation = Quaternion.Slerp(currentRotation, targetAlign, Time.fixedDeltaTime * rotationSmoothing);
        }
        else
        {
            rb.AddForce(Vector3.down * 20f, ForceMode.Acceleration);
        }

        if (Mathf.Abs(inputSteer) > 0.01f)
        {
            float turn = inputSteer * driveTurnSpeed * Time.fixedDeltaTime;
            nextRotation *= Quaternion.Euler(0f, turn, 0f);
        }

        rb.MoveRotation(nextRotation);

        if (Mathf.Abs(inputThrottle) > 0.01f)
        {
            rb.AddForce(Forward * inputThrottle * driveAcceleration, ForceMode.Acceleration);
        }
    }
}