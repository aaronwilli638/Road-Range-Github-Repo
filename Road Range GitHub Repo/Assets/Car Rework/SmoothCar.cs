using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class SmoothCar : MonoBehaviour
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

    [Header("Boost")]
    public float boostLeniency = 0.2f;
    public float boostMultiplier = 2.0f;
    public float boostDecayRate = 1.0f;
    public float driftBoostDecayMult = 3.0f;

    [Header("Height")]
    public float hoverHeight = 1.5f;
    public float heightCorrectionSpeed = 10f;
    public float slopeAlignSpeed = 15f;

    [Header("Gravity")]
    public float gravity = 40f;
    public float groundCheckDistance = 3.0f;
    public float airDrag = 0.1f; 
    public LayerMask groundLayer;

    [Header("Surface Detection")]
    public int offroadTerrainLayerIndex = 1;
    public float offroadSpeedMultiplier = 0.5f;
    public float offroadDragMultiplier = 2.0f;

    private Rigidbody rb;
    private EnergySystem energySystem;
    private float boostLeniencyTimer;
    public bool IsBoostingBuffered => isBoosting || boostLeniencyTimer > 0f;
    private Vector2 moveInput;
    private bool isBoosting;
    private bool isDriftInput;
    private bool isGrounded;
    private bool isOffroad;
    
    private float driftWeight;
    private float boostState;

    public float SteerInput => moveInput.x; 
    public bool IsDrifting => driftWeight > 0f;
    public bool IsGrounded => isGrounded;
    public float DriftFactor => Mathf.SmoothStep(0f, 1f, driftWeight);
    public bool IsBoosting => isBoosting;
    private float currentAcceleration;
    private float currentTurnSpeed;
    private float currentDrag;

    private float baseAcceleration;
    private float baseTurnSpeed;
    private float baseDrag;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        energySystem = GetComponent<EnergySystem>();

        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.useGravity = false;
        rb.linearDamping = 0; 
        rb.angularDamping = 0; 
        rb.constraints = RigidbodyConstraints.None;
        rb.maxAngularVelocity = 100f; 

        baseAcceleration = driveAcceleration;
        baseTurnSpeed = driveTurnSpeed;
        baseDrag = driveDrag;
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
            if (kb.sKey.isPressed) moveInput.y = -1;
            moveInput.x = (kb.dKey.isPressed ? 1 : 0) - (kb.aKey.isPressed ? 1 : 0);
            isDriftInput = kb.spaceKey.isPressed;
        }

        if (Gamepad.current != null)
        {
            var gp = Gamepad.current;
            Vector2 stick = gp.leftStick.ReadValue();
            if (stick.sqrMagnitude > 0.01f)
            {
                moveInput.x = stick.x;
                moveInput.y = stick.y;
            }
            if (gp.leftTrigger.isPressed) isDriftInput = true;
            
            if (gp.rightTrigger.isPressed) isBoosting = true;
        }

        if (Mouse.current != null && Mouse.current.leftButton.isPressed) isBoosting = true;

        if (isOffroad)
        {
            isBoosting = false;
        }

        if (isBoosting && !isDriftInput && isGrounded)
        {
             if (energySystem != null && !energySystem.TryConsume(energySystem.boostCostPerSec * Time.deltaTime))
             {
                 isBoosting = false;
             }
        }
        else
        {
            isBoosting = false;
        }
        if (isBoosting)
        {
            boostLeniencyTimer = boostLeniency;
        }
        else
        {
            boostLeniencyTimer -= Time.deltaTime;
        }
    }

    void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;
        if (dt <= 0) return;

        RaycastHit hit;
        Vector3 rayOrigin = transform.position + transform.up * 0.5f; 
        isGrounded = Physics.Raycast(rayOrigin, -transform.up, out hit, groundCheckDistance, groundLayer, QueryTriggerInteraction.Ignore);

        if (isGrounded && (hit.collider.transform.root == transform.root || hit.distance < 0.05f))
        {
            isGrounded = false;
        }

        Quaternion currentRotation = rb.rotation;
        Quaternion targetRotation = currentRotation;

        if (isGrounded)
        {
            Quaternion slopeRot = Quaternion.FromToRotation(transform.up, hit.normal) * currentRotation;
            targetRotation = Quaternion.Slerp(currentRotation, slopeRot, slopeAlignSpeed * dt);
        }
        else
        {
            Quaternion upright = Quaternion.FromToRotation(transform.up, Vector3.up) * currentRotation;
            targetRotation = Quaternion.Slerp(currentRotation, upright, 2f * dt);
        }

        if (isGrounded && Mathf.Abs(moveInput.x) > 0.01f)
        {
            float turnAmount = moveInput.x * currentTurnSpeed * dt;
            Quaternion turnRot = Quaternion.Euler(0, turnAmount, 0);
            targetRotation = targetRotation * turnRot;
        }

        SetAngularVelocityToReachRotation(targetRotation, dt);

        float targetDriftWeight = isDriftInput ? 1f : 0f;
        float driftSpeed = isDriftInput ? driftEaseInSpeed : driftEaseOutSpeed;
        driftWeight = Mathf.MoveTowards(driftWeight, targetDriftWeight, driftSpeed * dt);
        float driftBlend = Mathf.SmoothStep(0f, 1f, driftWeight);

        if (driftWeight > 0 && energySystem != null)
        {
            energySystem.Refill(rb.linearVelocity.magnitude * dt * energySystem.driftRefillPerMeter * driftBlend);
        }

        float transition = 5f; 
        baseAcceleration = Mathf.Lerp(baseAcceleration, driveAcceleration, transition * dt);
        baseTurnSpeed = Mathf.Lerp(baseTurnSpeed, driveTurnSpeed, transition * dt);
        baseDrag = Mathf.Lerp(baseDrag, driveDrag, transition * dt);

        currentAcceleration = Mathf.Lerp(baseAcceleration, driftAcceleration, driftBlend);
        currentTurnSpeed = Mathf.Lerp(baseTurnSpeed, driftTurnSpeed, driftBlend);
        currentDrag = Mathf.Lerp(baseDrag, driftDrag, driftBlend);

        float currentSurfaceMultiplier = 1f;
        float currentSurfaceDragMultiplier = 1f;
        isOffroad = false;

        if (isGrounded)
        {
            Terrain terrain = hit.collider.GetComponent<Terrain>();
            if (terrain != null)
            {
                int domIndex = GetDominantTextureIndex(hit.point, terrain);
                if (domIndex == offroadTerrainLayerIndex)
                {
                    isOffroad = true;
                    currentSurfaceMultiplier = offroadSpeedMultiplier;
                    currentSurfaceDragMultiplier = offroadDragMultiplier;
                }
            }
        }

        currentAcceleration *= currentSurfaceMultiplier;
        currentDrag *= currentSurfaceDragMultiplier;

        if (isBoosting) boostState = Mathf.MoveTowards(boostState, 1f, dt * 5f);
        else
        {
            float decay = boostDecayRate * (driftWeight > 0.1f ? driftBoostDecayMult : 1f);
            boostState = Mathf.Lerp(boostState, 0f, decay * dt);
        }
        float currentBoostMult = Mathf.Lerp(1f, boostMultiplier, Mathf.SmoothStep(0f, 1f, boostState));

        Vector3 velocity = rb.linearVelocity;
        Vector3 forwardDir = transform.forward;
        Vector3 rightDir = transform.right;
        Vector3 upDir = transform.up;

        float vForward = Vector3.Dot(velocity, forwardDir);
        float vRight = Vector3.Dot(velocity, rightDir);
        float vUp = Vector3.Dot(velocity, upDir);

        float actualDrag = isGrounded ? currentDrag : airDrag;
        vForward /= (1f + actualDrag * dt);

        float sideDrag = isGrounded ? Mathf.Lerp(5f, 0.5f, driftBlend) : 0.05f; 
        vRight /= (1f + sideDrag * dt);

        if (isGrounded)
        {
            float appliedAccel = (moveInput.y != 0) ? currentAcceleration * moveInput.y : 0;
            appliedAccel *= currentBoostMult;
            vForward += appliedAccel * dt;
        }

        Vector3 targetVelocity = forwardDir * vForward + rightDir * vRight + upDir * vUp;

        if (isGrounded)
        {
            float distance = hit.distance - 0.5f; 
            float heightError = hoverHeight - distance;
            
            targetVelocity += Vector3.down * gravity * dt;

            float currentVerticalSpeed = Vector3.Dot(targetVelocity, hit.normal);

            if (heightError > 0)
            {
                if (currentVerticalSpeed < 0)
                {
                    targetVelocity -= hit.normal * currentVerticalSpeed;
                    currentVerticalSpeed = 0;
                }
                float targetVerticalSpeed = heightError * heightCorrectionSpeed;
                if (currentVerticalSpeed < targetVerticalSpeed)
                {
                   targetVelocity += hit.normal * (targetVerticalSpeed - currentVerticalSpeed) * dt * 10f; 
                }
            }
            else if (currentVerticalSpeed > 0)
            {
                targetVelocity -= hit.normal * currentVerticalSpeed * dt * 10f;
            }
        }
        else
        {
            targetVelocity += Vector3.down * gravity * dt;
        }

        rb.AddForce(targetVelocity - rb.linearVelocity, ForceMode.VelocityChange);
    }

    private void SetAngularVelocityToReachRotation(Quaternion targetRot, float dt)
    {
        Quaternion diff = targetRot * Quaternion.Inverse(rb.rotation);
        diff.ToAngleAxis(out float angle, out Vector3 axis);

        if (angle > 180f) angle -= 360f;

        if (Mathf.Abs(angle) < 0.01f)
        {
            rb.angularVelocity = Vector3.zero;
            return;
        }

        Vector3 angularVel = axis * (angle * Mathf.Deg2Rad / dt);
        if (angularVel.magnitude > 100f) angularVel = angularVel.normalized * 100f;
        rb.angularVelocity = angularVel;
    }

    private int GetDominantTextureIndex(Vector3 worldPos, Terrain terrain)
    {
        TerrainData terrainData = terrain.terrainData;
        float mapX = ((worldPos.x - terrain.transform.position.x) / terrainData.size.x) * terrainData.alphamapWidth;
        float mapZ = ((worldPos.z - terrain.transform.position.z) / terrainData.size.z) * terrainData.alphamapHeight;

        int x = Mathf.FloorToInt(mapX);
        int z = Mathf.FloorToInt(mapZ);

        float[,,] splatmapData = terrainData.GetAlphamaps(x, z, 1, 1);

        float maxMix = 0;
        int maxIndex = 0;

        for (int i = 0; i < terrainData.alphamapLayers; i++)
        {
            if (splatmapData[0, 0, i] > maxMix)
            {
                maxMix = splatmapData[0, 0, i];
                maxIndex = i;
            }
        }

        return maxIndex;
    }
}