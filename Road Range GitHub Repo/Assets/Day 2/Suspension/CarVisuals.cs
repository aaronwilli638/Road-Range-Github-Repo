using UnityEngine;

public class CarVisuals : MonoBehaviour
{
    public Car2 carController;
    public Rigidbody carRb;
    public Transform visualBody;
    public Vector3 modelAdjustment; 

    public Transform axleFL, axleFR, axleRL, axleRR;
    public Transform wheelFL, wheelFR, wheelRL, wheelRR;

    public float frontRadius = 0.35f;
    public float rearRadius = 0.40f;
    
    public float rayOffset = 0.5f; 
    public float rayLength = 2.5f;
    public LayerMask groundLayer;
    public float smoothSpeed = 12f;

    public float maxSteerAngle = 30f;
    public float bodyTiltFactor = 2.5f;

    [Header("Visual Suspension")]
    public float springStiffness = 100f;
    public float springDamping = 10f;
    public float impactSensitivity = 0.05f;
    public float maxCompression = 0.5f;

    private float springPos;
    private float springVel;
    private float lastVerticalVelocity;
    private Vector3 initialLocalPos;

    void Start()
    {
        if (!carController) carController = GetComponent<Car2>();
        if (!carRb) carRb = GetComponent<Rigidbody>();

        if (visualBody == transform || visualBody == carRb.transform)
        {
            Debug.LogError("CarVisuals ERROR: 'Visual Body' is assigned to the Rigidbody root! It must be a CHILD object.");
            this.enabled = false;
            return;
        }
        if (visualBody) initialLocalPos = visualBody.localPosition;
    }

    void Update()
    {
        if (Time.deltaTime <= 0) return;

        Vector3 pFL = AlignWheel(axleFL, wheelFL, frontRadius);
        Vector3 pFR = AlignWheel(axleFR, wheelFR, frontRadius);
        Vector3 pRL = AlignWheel(axleRL, wheelRL, rearRadius);
        Vector3 pRR = AlignWheel(axleRR, wheelRR, rearRadius);

        float steer = carController ? carController.SteerInput : 0f;
        float speed = carRb ? carRb.linearVelocity.magnitude : 0f;

        Quaternion steerRot = Quaternion.Euler(0, steer * maxSteerAngle, 0);
        wheelFL.localRotation = steerRot;
        wheelFR.localRotation = steerRot;

        Vector3 forward = (pFL + pFR) * 0.5f - (pRL + pRR) * 0.5f;
        Vector3 left = (pFL + pRL) * 0.5f - (pFR + pRR) * 0.5f;
        Vector3 up = Vector3.Cross(left, forward).normalized;

        if (carRb != null)
        {
            float currentVerticalVel = transform.InverseTransformDirection(carRb.linearVelocity).y;
            float acceleration = (currentVerticalVel - lastVerticalVelocity) / Time.deltaTime;
            lastVerticalVelocity = currentVerticalVel;

            float targetPos = 0f;
            float force = (targetPos - springPos) * springStiffness;
            force -= springVel * springDamping;
            
            float impact = Mathf.Clamp(acceleration * impactSensitivity, -50f, 50f);
            force -= impact;

            springVel += force * Time.deltaTime;
            springPos += springVel * Time.deltaTime;
            springPos = Mathf.Clamp(springPos, -maxCompression, maxCompression);
        }

        Vector3 targetLocalPos = initialLocalPos + new Vector3(0, springPos, 0);

        if (up != Vector3.zero)
        {
            float tilt = -steer * speed * bodyTiltFactor * 0.5f;
            
            Quaternion groundRot = Quaternion.LookRotation(Vector3.ProjectOnPlane(transform.forward, up), up);
            Quaternion tiltRot = Quaternion.Euler(0, 0, tilt);

            Quaternion targetRot = groundRot * tiltRot * Quaternion.Euler(modelAdjustment);
            visualBody.rotation = Quaternion.Lerp(visualBody.rotation, targetRot, Time.deltaTime * smoothSpeed);
            
            visualBody.localPosition = Vector3.Lerp(visualBody.localPosition, targetLocalPos, Time.deltaTime * smoothSpeed);
        }
        else
        {
            visualBody.rotation = Quaternion.Lerp(visualBody.rotation, transform.rotation * Quaternion.Euler(modelAdjustment), Time.deltaTime * smoothSpeed);
            visualBody.localPosition = Vector3.Lerp(visualBody.localPosition, targetLocalPos, Time.deltaTime * smoothSpeed);
        }
    }

    Vector3 AlignWheel(Transform axle, Transform wheel, float radius)
    {
        Vector3 origin = axle.position + transform.up * rayOffset; 
        Vector3 dir = -transform.up;

        if (Physics.Raycast(origin, dir, out RaycastHit hit, rayLength + rayOffset, groundLayer, QueryTriggerInteraction.Ignore))
        {
             if (hit.collider.transform.root != transform.root)
             {
                Vector3 targetPos = hit.point + (transform.up * radius);
                wheel.position = targetPos;
                return hit.point;
             }
        }

        Vector3 restingPos = axle.position - (transform.up * (rayLength * 0.5f)); 
        wheel.position = restingPos;
        return restingPos - (transform.up * radius);
    }
}