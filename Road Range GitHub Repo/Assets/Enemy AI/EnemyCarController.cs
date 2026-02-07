using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EnemyCarController : MonoBehaviour
{
    public SmoothCar playerReference;
    public float steerMultiplier = 1.0f;

    [Header("Hover Physics")]
    public float hoverHeight = 2.0f;
    public float hoverDamping = 10f;
    public float rotationSmoothing = 5f;
    public float gravity = 40f; 
    public LayerMask groundLayer;
    
    [Header("Surface")]
    public int offroadLayerIndex = 1;
    public float offroadSpeedMult = 0.5f;

    private Rigidbody rb;
    private float driveAcceleration;
    private float driveTurnSpeed;
    private float inputSteer;
    private float inputThrottle;
    
    private float currentVerticalSpeed;
    private Vector3 airVelocity;
    private Vector3 lastPosition;

    public bool IsOffroad { get; private set; }
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
        lastPosition = transform.position;
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
        float dt = Time.fixedDeltaTime;
        Quaternion currentRotation = rb.rotation;
        Quaternion nextRotation = currentRotation;
        
        Vector3 currentEffectiveVelocity = (transform.position - lastPosition) / dt;
        lastPosition = transform.position;

        Ray ray = new Ray(transform.position, -transform.up);
        bool isGrounded = Physics.Raycast(ray, out RaycastHit hit, hoverHeight + 2.0f, groundLayer);

        float currentAcc = driveAcceleration;

        if (isGrounded)
        {
            Terrain terrain = hit.collider.GetComponent<Terrain>();
            if (terrain != null)
            {
                int texIndex = GetDominantTextureIndex(hit.point, terrain);
                if (texIndex == offroadLayerIndex)
                {
                    currentAcc *= offroadSpeedMult;
                    IsOffroad = true;
                }
                else
                {
                    IsOffroad = false;
                }
            }
            else
            {
                IsOffroad = false;
            }

            currentVerticalSpeed = 0f;
            
            airVelocity = currentEffectiveVelocity;
            airVelocity.y = 0f; 

            Vector3 targetPosition = hit.point + (hit.normal * hoverHeight);
            Vector3 smoothedPosition = Vector3.Lerp(rb.position, targetPosition, dt * hoverDamping);
            rb.MovePosition(smoothedPosition);

            Quaternion targetAlign = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;
            nextRotation = Quaternion.Slerp(currentRotation, targetAlign, dt * rotationSmoothing);

            if (Mathf.Abs(inputThrottle) > 0.01f)
            {
                rb.AddForce(Forward * inputThrottle * currentAcc, ForceMode.Acceleration);
            }
        }
        else
        {
            currentVerticalSpeed -= gravity * dt;
            rb.linearVelocity = Vector3.zero; 
            Vector3 displacement = (airVelocity * dt) + (Vector3.up * currentVerticalSpeed * dt);
            rb.MovePosition(rb.position + displacement);
            Quaternion upright = Quaternion.FromToRotation(transform.up, Vector3.up) * currentRotation;
            nextRotation = Quaternion.Slerp(currentRotation, upright, dt * 2f);
            
            IsOffroad = false;
        }

        if (Mathf.Abs(inputSteer) > 0.01f)
        {
            float turn = inputSteer * driveTurnSpeed * dt;
            nextRotation *= Quaternion.Euler(0f, turn, 0f);
        }

        rb.MoveRotation(nextRotation);
    }

    private int GetDominantTextureIndex(Vector3 worldPos, Terrain terrain)
    {
        TerrainData td = terrain.terrainData;
        float mapX = ((worldPos.x - terrain.transform.position.x) / td.size.x) * td.alphamapWidth;
        float mapZ = ((worldPos.z - terrain.transform.position.z) / td.size.z) * td.alphamapHeight;

        int x = Mathf.FloorToInt(mapX);
        int z = Mathf.FloorToInt(mapZ);
        
        if (x < 0 || z < 0 || x >= td.alphamapWidth || z >= td.alphamapHeight) return 0;

        float[,,] splat = td.GetAlphamaps(x, z, 1, 1);

        float maxMix = 0;
        int maxIndex = 0;

        for (int i = 0; i < td.alphamapLayers; i++)
        {
            if (splat[0, 0, i] > maxMix)
            {
                maxMix = splat[0, 0, i];
                maxIndex = i;
            }
        }

        return maxIndex;
    }
}