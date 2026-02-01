using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class Car2 : MonoBehaviour
{
    [Header("Drive")]
    public float driveAcceleration = 50f;
    public float driveTurnSpeed = 100f;
    public float driveDrag = 3.5f;

    [Header("Drift")]
    public float driftAcceleration = 50f;
    public float driftTurnSpeed = 180f;
    public float driftDrag = 1f;
    public float driftEaseInSpeed = 5f;
    public float driftEaseOutSpeed = 2f;

    [Header("Shoot")]
    public float shooterAcceleration = 40f; 
    public float shooterTurnSpeed = 80f;    
    public float shooterDrag = 4f;
    public float transitionSpeed = 5f;

    [Header("Boost")]
    public float boostMultiplier = 2.0f;


    [Header("Height")]
    public float hoverHeight = 1.5f;
    public float heightCorrectionSpeed = 10f;
    public float slopeAlignSpeed = 15f;

    [Header("Gravity")]
    public float gravity = 40f;
    public float groundCheckDistance = 3.0f;
    public LayerMask groundLayer;

    private Rigidbody rb;
    private CarShooter carShooter; 
    private EnergySystem energySystem;
    
    private Vector2 moveInput;
    private bool isBoosting;
    private bool isDriftInput;
    private bool isGrounded;
    
    private float driftWeight;

    public float SteerInput => moveInput.x; 
    
    public bool IsDrifting => driftWeight > 0f;
    public bool IsGrounded => isGrounded;

    public float DriftFactor => Mathf.SmoothStep(0f, 1f, driftWeight);

    private float currentAcceleration;
    private float currentTurnSpeed;
    private float currentDrag;

    private float baseAcceleration;
    private float baseTurnSpeed;
    private float baseDrag;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        carShooter = GetComponent<CarShooter>(); 
        energySystem = GetComponent<EnergySystem>();

        rb.linearDamping = 0; 
        rb.useGravity = false; 
        rb.constraints = RigidbodyConstraints.None;

        baseAcceleration = driveAcceleration;
        baseTurnSpeed = driveTurnSpeed;
        baseDrag = driveDrag;

        currentAcceleration = driveAcceleration;
        currentTurnSpeed = driveTurnSpeed;
        currentDrag = driveDrag;
    }

    void Update()
    {
        moveInput = Vector2.zero;
        isBoosting = false;
        isDriftInput = false;

        if (Keyboard.current != null)
        {
            var kb = Keyboard.current;
            moveInput.y = kb.wKey.isPressed ? 1 : 0;
            moveInput.x = (kb.dKey.isPressed ? 1 : 0) - (kb.aKey.isPressed ? 1 : 0);
            isDriftInput = kb.spaceKey.isPressed;
        }

        bool aimActive = carShooter != null && carShooter.IsAiming;

        if (Mouse.current != null && !aimActive && !isDriftInput && Mouse.current.leftButton.isPressed)
        {
            if (energySystem == null || energySystem.TryConsume(energySystem.boostCostPerSec * Time.deltaTime))
            {
                isBoosting = true;
            }
        }
    }

    void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;
        rb.angularVelocity = Vector3.zero;

        RaycastHit hit;
        Vector3 rayOrigin = transform.position + transform.up * 0.5f; 
        isGrounded = Physics.Raycast(rayOrigin, -transform.up, out hit, groundCheckDistance, groundLayer, QueryTriggerInteraction.Ignore);

        if (isGrounded)
        {
            if (hit.collider.transform.root == transform.root || hit.distance < 0.05f)
            {
                isGrounded = false;
            }
        }

        if (isGrounded)
        {
            Quaternion targetRotation = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, slopeAlignSpeed * dt);
        }
        else
        {
            Quaternion upright = Quaternion.FromToRotation(transform.up, Vector3.up) * transform.rotation;
            transform.rotation = Quaternion.Slerp(transform.rotation, upright, 2f * dt);
        }

        bool aimActive = carShooter != null && carShooter.IsAiming;

        float targetDriftWeight = isDriftInput ? 1f : 0f;
        float driftSpeed = isDriftInput ? driftEaseInSpeed : driftEaseOutSpeed;
        driftWeight = Mathf.MoveTowards(driftWeight, targetDriftWeight, driftSpeed * dt);
        
        float driftBlend = Mathf.SmoothStep(0f, 1f, driftWeight);

        float targetBaseAccel = driveAcceleration;
        float targetBaseTurn = driveTurnSpeed;
        float targetBaseDrag = driveDrag;

        if (aimActive)
        {
            targetBaseAccel = shooterAcceleration;
            targetBaseTurn = shooterTurnSpeed;
            targetBaseDrag = shooterDrag;
        }

        baseAcceleration = Mathf.Lerp(baseAcceleration, targetBaseAccel, transitionSpeed * dt);
        baseTurnSpeed = Mathf.Lerp(baseTurnSpeed, targetBaseTurn, transitionSpeed * dt);
        baseDrag = Mathf.Lerp(baseDrag, targetBaseDrag, transitionSpeed * dt);

        currentAcceleration = Mathf.Lerp(baseAcceleration, driftAcceleration, driftBlend);
        currentTurnSpeed = Mathf.Lerp(baseTurnSpeed, driftTurnSpeed, driftBlend);
        currentDrag = Mathf.Lerp(baseDrag, driftDrag, driftBlend);

        if (driftWeight > 0 && energySystem != null)
        {
            energySystem.Refill(rb.linearVelocity.magnitude * dt * energySystem.driftRefillPerMeter * driftBlend);
        }

        if (isGrounded && moveInput.x != 0 && (Mathf.Abs(moveInput.y) > 0.05f || rb.linearVelocity.magnitude > 1f))
        {
            float turnAmount = moveInput.x * currentTurnSpeed * dt;
            transform.Rotate(0, turnAmount, 0); 
        }

        Vector3 currentVelocity = rb.linearVelocity;
        
        float appliedAccel = (moveInput.y != 0) ? currentAcceleration * moveInput.y : 0;
        if (isBoosting) appliedAccel *= boostMultiplier;
        
        if (isGrounded)
        {
            currentVelocity += transform.forward * appliedAccel * dt;
            currentVelocity -= currentVelocity * currentDrag * dt;

            float distance = hit.distance - 0.5f; 
            float heightError = hoverHeight - distance;
            
            currentVelocity += Vector3.down * gravity * dt;

            float currentVerticalSpeed = Vector3.Dot(currentVelocity, hit.normal);

            if (heightError > 0)
            {
                if (currentVerticalSpeed < 0)
                {
                    currentVelocity -= hit.normal * currentVerticalSpeed;
                    currentVerticalSpeed = 0;
                }

                float targetVerticalSpeed = heightError * heightCorrectionSpeed;

                if (currentVerticalSpeed < targetVerticalSpeed)
                {
                   currentVelocity += hit.normal * (targetVerticalSpeed - currentVerticalSpeed) * dt * 10f; 
                }
            }
            else
            {
                if (currentVerticalSpeed > 0)
                {
                    currentVelocity -= hit.normal * currentVerticalSpeed * dt * 10f;
                }
            }
        }
        else
        {
            currentVelocity += Vector3.down * gravity * dt;
        }

        rb.linearVelocity = currentVelocity;
    }
}