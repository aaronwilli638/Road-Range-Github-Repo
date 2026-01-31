using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class Car2 : MonoBehaviour
{
    public float driveAcceleration = 50f;
    public float driveTurnSpeed = 100f;
    public float driveDrag = 3.5f;

    public float driftAcceleration = 50f;
    public float driftTurnSpeed = 180f;
    public float driftDrag = 1f;

    public float shooterAcceleration = 40f; 
    public float shooterTurnSpeed = 80f;    
    public float shooterDrag = 4f;

    public float boostMultiplier = 2.0f;
    public float transitionSpeed = 5f;

    private Rigidbody rb;
    private CarShooter carShooter; 
    private EnergySystem energySystem;
    
    private Vector2 moveInput;
    private bool isBoosting;
    private bool isDrifting;

    public float SteerInput => moveInput.x; 
    public bool IsDrifting => isDrifting;

    private float currentAcceleration;
    private float currentTurnSpeed;
    private float currentDrag;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        carShooter = GetComponent<CarShooter>(); 
        energySystem = GetComponent<EnergySystem>();

        rb.linearDamping = 0; 
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        currentAcceleration = driveAcceleration;
        currentTurnSpeed = driveTurnSpeed;
        currentDrag = driveDrag;
    }

    void Update()
    {
        moveInput = Vector2.zero;
        isBoosting = false;
        isDrifting = false;

        if (Keyboard.current != null)
        {
            var kb = Keyboard.current;
            moveInput.y = kb.wKey.isPressed ? 1 : 0;
            moveInput.x = (kb.dKey.isPressed ? 1 : 0) - (kb.aKey.isPressed ? 1 : 0);
            isDrifting = kb.spaceKey.isPressed;
        }

        bool aimActive = carShooter != null && carShooter.IsAiming;

        if (Mouse.current != null && !aimActive && !isDrifting && Mouse.current.leftButton.isPressed)
        {
            if (energySystem == null || energySystem.TryConsume(energySystem.boostCostPerSec * Time.deltaTime))
            {
                isBoosting = true;
            }
        }
    }

    void FixedUpdate()
    {
        rb.angularVelocity = Vector3.zero;

        bool aimActive = carShooter != null && carShooter.IsAiming;
        
        float targetAccel = driveAcceleration;
        float targetTurn = driveTurnSpeed;
        float targetDrag = driveDrag;

        if (isDrifting)
        {
            targetAccel = driftAcceleration;
            targetTurn = driftTurnSpeed;
            targetDrag = driftDrag;

            if (energySystem != null)
            {
                energySystem.Refill(rb.linearVelocity.magnitude * Time.fixedDeltaTime * energySystem.driftRefillPerMeter);
            }
        }
        else if (aimActive)
        {
            targetAccel = shooterAcceleration;
            targetTurn = shooterTurnSpeed;
            targetDrag = shooterDrag;
        }

        float dt = Time.fixedDeltaTime;
        currentAcceleration = Mathf.Lerp(currentAcceleration, targetAccel, transitionSpeed * dt);
        currentTurnSpeed = Mathf.Lerp(currentTurnSpeed, targetTurn, transitionSpeed * dt);
        currentDrag = Mathf.Lerp(currentDrag, targetDrag, transitionSpeed * dt);

        if (moveInput.x != 0 && (Mathf.Abs(moveInput.y) > 0.05f || rb.linearVelocity.magnitude > 1f))
        {
            float turnAmount = moveInput.x * currentTurnSpeed * dt;
            transform.Rotate(0, turnAmount, 0);
        }

        Vector3 currentVelocity = rb.linearVelocity;
        float appliedAccel = (moveInput.y != 0) ? currentAcceleration * moveInput.y : 0;

        if (isBoosting) appliedAccel *= boostMultiplier;

        currentVelocity += transform.forward * appliedAccel * dt;
        currentVelocity -= currentVelocity * currentDrag * dt;

        rb.linearVelocity = currentVelocity;
    }
}