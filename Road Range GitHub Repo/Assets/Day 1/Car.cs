using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class Car : MonoBehaviour
{
    [Header("Settings")]
    public float acceleration = 50f;
    public float turnSpeed = 180f;
    public float boostMultiplier = 2.0f;
    public float brakeDrag = 5f;
    public float driftDrag = 1f; // Really just normal turning drag but thinking about it all as drifting made most sense

    private Rigidbody rb;
    
    // Store input info
    private Vector2 moveInput;
    private bool isBoosting;
    private bool isBraking;
    public float SteerInput => moveInput.x; // Lets other scripts access the steer input

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.linearDamping = 0; 
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    void Update()
    {
        // Clear input
        moveInput = Vector2.zero;
        isBoosting = false;
        isBraking = false;

        // Set input vector from keys
        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed) moveInput.y += 1;
            if (Keyboard.current.sKey.isPressed) moveInput.y -= 1;
            if (Keyboard.current.aKey.isPressed) moveInput.x -= 1;
            if (Keyboard.current.dKey.isPressed) moveInput.x += 1;
            isBoosting = Keyboard.current.leftShiftKey.isPressed;
            isBraking = Keyboard.current.spaceKey.isPressed;
        }
    }

    void FixedUpdate()
    {
        // Set turn
        if (moveInput.x != 0)
        {
            float turnAmount = moveInput.x * turnSpeed * Time.fixedDeltaTime;
            if (moveInput.y < -0.1f) turnAmount *= -1f; // Invert steering when in reverse. Feels weird otherwise
            transform.Rotate(0, turnAmount, 0);
        }

        Vector3 direction = transform.forward;

        // Ripped straight from the Racecat controller
        Vector3 currentVelocity = rb.linearVelocity;
        float currentAccel = 0;

        if (moveInput.y != 0)
        {
            currentAccel = acceleration * moveInput.y;
        }

        float currentDrag = driftDrag;

        if (isBoosting) currentAccel *= boostMultiplier;
        if (isBraking)
        {
            currentAccel = 0;
            currentDrag = brakeDrag;
        }

        currentVelocity += direction * currentAccel * Time.fixedDeltaTime;
        currentVelocity -= currentVelocity * currentDrag * Time.fixedDeltaTime;

        rb.linearVelocity = currentVelocity;
    }
}