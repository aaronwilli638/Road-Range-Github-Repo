using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EnemyCarController : MonoBehaviour
{
    public SmoothCar playerReference;
    
    public float speedMultiplier = 0.6f;
    public float steerMultiplier = 1.0f;

    [Header("Hover Physics")]
    public float hoverHeight = 2.0f;
    public float springStrength = 200f;
    public float springDamper = 10f;
    public LayerMask groundLayer;

    private Rigidbody rb;
    private float driveAcceleration;
    private float driveTurnSpeed;
    private float inputSteer;
    private float inputThrottle;

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

    private void Start()
    {
        if (playerReference != null)
        {
            rb.linearDamping = playerReference.driveDrag;
            driveTurnSpeed = playerReference.driveTurnSpeed * steerMultiplier;
            driveAcceleration = playerReference.driveAcceleration * speedMultiplier;
        }
    }

    public void SetInputs(float steer, float throttle)
    {
        inputSteer = Mathf.Clamp(steer, -1f, 1f);
        inputThrottle = Mathf.Clamp(throttle, -1f, 1f);
    }

    private void FixedUpdate()
    {
        Ray ray = new Ray(transform.position, Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hit, hoverHeight + 2f, groundLayer))
        {
            Vector3 vel = rb.linearVelocity;
            Vector3 rayDir = Vector3.down;
            
            float rayDirVel = Vector3.Dot(rayDir, vel);
            float x = hit.distance - hoverHeight;
            float springForce = (x * springStrength) - (rayDirVel * springDamper);

            rb.AddForce(Vector3.up * springForce);

            Quaternion targetRotation = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;
            rb.MoveRotation(Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * 10f));
        }
        else
        {
            rb.AddForce(Vector3.down * 20f, ForceMode.Acceleration);
        }

        if (Mathf.Abs(inputSteer) > 0.01f)
        {
            float turn = inputSteer * driveTurnSpeed * Time.fixedDeltaTime;
            Quaternion turnRot = Quaternion.Euler(0f, turn, 0f);
            rb.MoveRotation(rb.rotation * turnRot);
        }

        if (Mathf.Abs(inputThrottle) > 0.01f)
        {
            rb.AddForce(Forward * inputThrottle * driveAcceleration, ForceMode.Acceleration);
        }
    }
}