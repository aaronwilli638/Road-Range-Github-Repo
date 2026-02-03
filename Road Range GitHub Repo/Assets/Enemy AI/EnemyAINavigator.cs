using UnityEngine;

[RequireComponent(typeof(EnemyCarController))]
public class EnemyAINavigator : MonoBehaviour
{
    public EnemyMasterTarget masterTarget;

    [Header("Formation")]
    public float zOffsetMin = 5f;
    public float zOffsetMax = 15f;

    [Header("Speed Logic")]
    public float catchUpSpeedMultiplier = 1.1f;
    public float regularSpeedMultiplier = 0.7f;

    [Header("Pathfinding")]
    public float lookAheadDistance = 10f;
    public float separationStrength = 5f;
    
    [Header("Layers")]
    public LayerMask obstacleMask;
    public LayerMask racerMask;

    private EnemyCarController car;
    private float targetZOffset;
    private float targetXOffset;
    private float offsetTimer;

    private void Start()
    {
        car = GetComponent<EnemyCarController>();
        RandomizeOffsets();
    }

    private void FixedUpdate()
    {
        if (masterTarget == null) return;

        Vector3 masterFwd = masterTarget.transform.forward;
        Vector3 masterRight = masterTarget.transform.right;
        
        float clampedX = Mathf.Clamp(targetXOffset, masterTarget.CurrentSafeLeft, masterTarget.CurrentSafeRight);
        Vector3 formationPos = masterTarget.transform.position - (masterFwd * targetZOffset) + (masterRight * clampedX);

        Vector3 dirToTarget = (formationPos - transform.position).normalized;

        Collider[] neighbors = Physics.OverlapSphere(transform.position, 10f, racerMask);
        Vector3 separation = Vector3.zero;
        foreach (var n in neighbors)
        {
            if (n.transform.root == transform.root) continue;
            Vector3 push = transform.position - n.transform.position;
            separation += push.normalized / (push.sqrMagnitude + 0.1f);
        }
        dirToTarget += separation * separationStrength;

        if (Physics.Raycast(transform.position, car.Forward, out RaycastHit hit, lookAheadDistance, obstacleMask))
        {
            dirToTarget += Vector3.Reflect(car.Forward, hit.normal) * 2f;
        }

        dirToTarget.Normalize();

        float angleToTarget = Vector3.SignedAngle(car.Forward, dirToTarget, Vector3.up);
        float steerInput = Mathf.Clamp(angleToTarget * 0.03f, -1f, 1f);

        Vector3 toMaster = masterTarget.transform.position - transform.position;
        float distBehind = Vector3.Dot(toMaster, masterFwd);
        
        float targetSpeed;

        if (distBehind > zOffsetMax)
        {
            targetSpeed = car.MaxSpeed * catchUpSpeedMultiplier;
        }
        else
        {
            targetSpeed = car.MaxSpeed * regularSpeedMultiplier;
        }

        float throttleInput;

        if (Vector3.Distance(transform.position, formationPos) < 3f && distBehind < 0)
        {
            throttleInput = -0.5f;
        }
        else
        {
            throttleInput = Mathf.Clamp((targetSpeed - car.CurrentSpeed) * 0.5f, -1f, 1f);
        }

        if (Mathf.Abs(angleToTarget) > 25f)
        {
            throttleInput *= 0.5f;
        }

        car.SetInputs(steerInput, throttleInput);

        offsetTimer += Time.fixedDeltaTime;
        if (offsetTimer > 3f)
        {
            offsetTimer = 0f;
            if (Random.value > 0.7f) RandomizeOffsets();
        }
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