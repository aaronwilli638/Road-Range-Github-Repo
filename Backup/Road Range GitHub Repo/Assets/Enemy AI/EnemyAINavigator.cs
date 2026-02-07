using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(EnemyCarController))]
public class EnemyAINavigator : MonoBehaviour
{
    public EnemyMasterTarget masterTarget;

    [Header("Life Cycle")]
    public float maxOffroadDuration = 5f;

    [Header("Formation")]
    public float zOffsetMin = 5f;
    public float zOffsetMax = 15f;

    [Header("Speed Logic")]
    public float catchUpSpeedMultiplier = 1.2f;
    public float regularSpeedMultiplier = 0.7f;

    [Header("Pathfinding")]
    public float lookAheadDistance = 10f;
    public float wallAvoidanceRadius = 1.5f;
    public float wallAvoidanceStrength = 5f;
    public float separationStrength = 5f;
    public float separationRadius = 6f;
    
    [Header("Layers")]
    public LayerMask obstacleMask;
    public LayerMask racerMask;
    public LayerMask lineOfSightMask;
    public LayerMask groundLayer;

    private EnemyCarController car;
    private Rigidbody rb;
    private EnemySpawner spawner;
    private float targetZOffset;
    private float targetXOffset;
    private float offsetTimer;
    private float currentOffroadTimer;

    private List<Transform> trackWaypoints = new List<Transform>();
    private int currentTargetIndex = 0;

    public void Initialize(EnemySpawner spawnerRef)
    {
        spawner = spawnerRef;
    }

    private void OnEnable()
    {
        currentOffroadTimer = 0f;
        RandomizeOffsets();
    }

    private void Start()
    {
        car = GetComponent<EnemyCarController>();
        rb = GetComponent<Rigidbody>();
        
        if (masterTarget != null && masterTarget.waypointParent != null)
        {
            foreach (Transform t in masterTarget.waypointParent)
            {
                trackWaypoints.Add(t);
            }
        }
    }

    private void FixedUpdate()
    {
        if (car.IsOffroad)
        {
            currentOffroadTimer += Time.fixedDeltaTime;
            if (currentOffroadTimer >= maxOffroadDuration)
            {
                if (spawner != null)
                {
                    spawner.ReturnEnemy(this.gameObject);
                }
                else
                {
                    gameObject.SetActive(false);
                }
                return;
            }
        }
        else
        {
            currentOffroadTimer = 0f;
        }

        if (masterTarget == null || trackWaypoints.Count == 0) return;

        Vector3 masterFwd = masterTarget.transform.forward;
        Vector3 masterRight = masterTarget.transform.right;

        float clampedX = Mathf.Clamp(targetXOffset, masterTarget.CurrentSafeLeft, masterTarget.CurrentSafeRight);
        Vector3 formationPos = masterTarget.transform.position - (masterFwd * targetZOffset) + (masterRight * clampedX);

        bool hasLineOfSight = !Physics.Linecast(transform.position + Vector3.up, formationPos + Vector3.up, lineOfSightMask);
        
        Vector3 driveTarget;
        float finalThrottleInput;
        float finalSteerInput;
        float targetSpeed;

        if (hasLineOfSight)
        {
            Vector3 distVector = transform.position - masterTarget.transform.position;
            float distAhead = Vector3.Dot(distVector, masterFwd);

            if (distAhead > 0)
            {
                rb.MovePosition(rb.position - masterFwd * distAhead);
                
                float currentFwdSpeed = Vector3.Dot(rb.linearVelocity, masterFwd);
                float targetFwdSpeed = masterTarget.CurrentSpeed;
                
                if (currentFwdSpeed > targetFwdSpeed)
                {
                    rb.linearVelocity -= masterFwd * (currentFwdSpeed - targetFwdSpeed);
                }
            }
            
            driveTarget = formationPos;

            int closestIndex = -1;
            float closestDist = float.MaxValue;
            for(int i = 0; i < trackWaypoints.Count; i++)
            {
                float d = Vector3.SqrMagnitude(transform.position - trackWaypoints[i].position);
                if(d < closestDist)
                {
                    closestDist = d;
                    closestIndex = i;
                }
            }

            if (closestIndex != -1)
            {
                int nextIndex = (closestIndex + 1) % trackWaypoints.Count;
                int prevIndex = (closestIndex - 1 + trackWaypoints.Count) % trackWaypoints.Count;

                Vector3 toNext = (trackWaypoints[nextIndex].position - trackWaypoints[closestIndex].position).normalized;
                Vector3 toCar = transform.position - trackWaypoints[closestIndex].position;

                float dotNext = Vector3.Dot(toCar, toNext);

                if (dotNext > 0) currentTargetIndex = nextIndex;
                else currentTargetIndex = closestIndex;
            }

            Vector3 toMaster = masterTarget.transform.position - transform.position;
            float distBehind = Vector3.Dot(toMaster, masterFwd);
            
            if (distBehind > zOffsetMax) targetSpeed = car.MaxSpeed * catchUpSpeedMultiplier;
            else targetSpeed = car.MaxSpeed * regularSpeedMultiplier;

            if (Vector3.Distance(transform.position, formationPos) < 3f && distBehind < 0) finalThrottleInput = -0.5f;
            else finalThrottleInput = Mathf.Clamp((targetSpeed - car.CurrentSpeed) * 0.5f, -1f, 1f);
        }
        else
        {
            Transform targetPoint = trackWaypoints[currentTargetIndex];
            driveTarget = targetPoint.position;

            int prevIndex = (currentTargetIndex - 1 + trackWaypoints.Count) % trackWaypoints.Count;
            Vector3 trackSegmentDir = (targetPoint.position - trackWaypoints[prevIndex].position).normalized;
            Vector3 toTarget = targetPoint.position - transform.position;
            
            if (Vector3.Dot(toTarget, trackSegmentDir) < 0)
            {
                currentTargetIndex = (currentTargetIndex + 1) % trackWaypoints.Count;
            }

            targetSpeed = car.MaxSpeed * catchUpSpeedMultiplier;
            finalThrottleInput = Mathf.Clamp((targetSpeed - car.CurrentSpeed) * 0.5f, -1f, 1f);
        }

        Vector3 dirToTarget = (driveTarget - transform.position).normalized;

        Collider[] neighbors = Physics.OverlapSphere(transform.position, separationRadius, racerMask);
        Vector3 separationSum = Vector3.zero;
        int count = 0;
        
        foreach (var n in neighbors)
        {
            if (n.transform.root == transform.root) continue;
            
            Vector3 push = transform.position - n.transform.position;
            float sqrDist = push.sqrMagnitude;
            
            separationSum += push.normalized / (sqrDist + 0.1f);
            count++;

            if (sqrDist < (separationRadius * separationRadius) * 0.5f)
            {
                float dist = Mathf.Sqrt(sqrDist);
                float repelStrength = (separationRadius - dist) * 0.5f;
                rb.linearVelocity += push.normalized * repelStrength * Time.fixedDeltaTime;
            }
        }

        if (count > 0) dirToTarget += separationSum.normalized * separationStrength;

        if (Physics.SphereCast(transform.position, wallAvoidanceRadius, car.Forward, out RaycastHit hit, lookAheadDistance, obstacleMask))
        {
            dirToTarget += Vector3.Reflect(car.Forward, hit.normal) * wallAvoidanceStrength;
        }

        Vector3 terrainCorrection = CheckTerrainBounds();
        if (terrainCorrection != Vector3.zero)
        {
            dirToTarget += terrainCorrection * wallAvoidanceStrength * 2f;
        }

        dirToTarget.Normalize();

        float angleToTarget = Vector3.SignedAngle(car.Forward, dirToTarget, transform.up);
        finalSteerInput = Mathf.Clamp(angleToTarget * 0.03f, -1f, 1f);

        if (Mathf.Abs(angleToTarget) > 25f) finalThrottleInput *= 0.5f;

        car.SetInputs(finalSteerInput, finalThrottleInput);

        if (targetSpeed > car.MaxSpeed && car.CurrentSpeed < targetSpeed && finalThrottleInput > 0.9f)
        {
            float speedDeficit = targetSpeed - car.CurrentSpeed;
            rb.AddForce(car.Forward * speedDeficit, ForceMode.Acceleration);
        }

        offsetTimer += Time.fixedDeltaTime;
        if (offsetTimer > 3f)
        {
            offsetTimer = 0f;
            if (Random.value > 0.7f) RandomizeOffsets();
        }
    }

    private Vector3 CheckTerrainBounds()
    {
        if (masterTarget == null) return Vector3.zero;

        Vector3 leftProbe = transform.position + (transform.forward * lookAheadDistance) - (transform.right * 2f);
        Vector3 rightProbe = transform.position + (transform.forward * lookAheadDistance) + (transform.right * 2f);
        
        bool leftOff = IsPointOffroad(leftProbe);
        bool rightOff = IsPointOffroad(rightProbe);

        if (leftOff && !rightOff) return transform.right;
        if (rightOff && !leftOff) return -transform.right;
        if (leftOff && rightOff) return (masterTarget.transform.position - transform.position).normalized;

        return Vector3.zero;
    }

    private bool IsPointOffroad(Vector3 point)
    {
        RaycastHit hit;
        if (Physics.Raycast(point + Vector3.up * 5f, Vector3.down, out hit, 10f, groundLayer))
        {
            Terrain t = hit.collider.GetComponent<Terrain>();
            if (t != null)
            {
                return IsOffroadTexture(hit.point, t);
            }
        }
        return false;
    }

    private bool IsOffroadTexture(Vector3 worldPos, Terrain terrain)
    {
        TerrainData td = terrain.terrainData;
        float mapX = ((worldPos.x - terrain.transform.position.x) / td.size.x) * td.alphamapWidth;
        float mapZ = ((worldPos.z - terrain.transform.position.z) / td.size.z) * td.alphamapHeight;

        int x = Mathf.FloorToInt(mapX);
        int z = Mathf.FloorToInt(mapZ);

        if (x < 0 || z < 0 || x >= td.alphamapWidth || z >= td.alphamapHeight) return true;

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
        return maxIndex == masterTarget.offroadLayerIndex;
    }

    private void RandomizeOffsets()
    {
        targetZOffset = Random.Range(zOffsetMin, zOffsetMax);
        if (masterTarget != null)
        {
            targetXOffset = Random.Range(masterTarget.CurrentSafeLeft, masterTarget.CurrentSafeRight);
        }
    }
}