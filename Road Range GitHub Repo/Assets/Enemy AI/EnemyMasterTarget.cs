using UnityEngine;
using System.Collections.Generic;
using System;

public class EnemyMasterTarget : MonoBehaviour
{
    public Transform waypointParent;
    public SmoothCar playerReference;
    public event Action OnWaypointReached;
    
    [Header("Movement")]
    public float targetLeadDistance = 25f;
    public float minSpeed = 10f;
    [Range(0.1f, 1.5f)]
    public float speedMultiplier = 0.6f; 

    [Header("Track Detection")]
    public LayerMask wallLayer;
    public float maxScanDistance = 20f;
    public float wallPadding = 2f;

    private List<Transform> waypoints = new List<Transform>();
    private int currentIndex = 0;
    private float currentSpeed;
    
    public float CurrentSafeRight { get; private set; }
    public float CurrentSafeLeft { get; private set; }
    public float CurrentSpeed => currentSpeed;

    private void Start()
    {
        if (waypointParent == null) return;
        foreach (Transform child in waypointParent) waypoints.Add(child);
    }

    private void Update()
    {
        if (waypoints.Count == 0 || playerReference == null) return;

        float playerBaseSpeed = playerReference.driveAcceleration / playerReference.driveDrag;

        Vector3 toMe = transform.position - playerReference.transform.position;
        float currentLead = Vector3.Dot(toMe, transform.forward);
        float distanceLogic = (targetLeadDistance - currentLead) * 0.1f;

        float desiredSpeed = playerBaseSpeed * (speedMultiplier + distanceLogic);
        currentSpeed = Mathf.Max(minSpeed, desiredSpeed);

        Vector3 dest = waypoints[currentIndex].position;
        Vector3 pathDir = (dest - transform.position).normalized;
        
        if (pathDir != Vector3.zero) 
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(pathDir), 5f * Time.deltaTime);

        transform.position = Vector3.MoveTowards(transform.position, dest, currentSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, dest) < 2f)
        {
            currentIndex = (currentIndex + 1) % waypoints.Count;
            OnWaypointReached?.Invoke();
        }

        ScanTrackWidth();
    }

    private void ScanTrackWidth()
    {
        RaycastHit hit;
        Vector3 origin = transform.position + Vector3.up;

        if (Physics.Raycast(origin, transform.right, out hit, maxScanDistance, wallLayer))
            CurrentSafeRight = Mathf.Max(0f, hit.distance - wallPadding);
        else
            CurrentSafeRight = maxScanDistance;

        if (Physics.Raycast(origin, -transform.right, out hit, maxScanDistance, wallLayer))
            CurrentSafeLeft = -Mathf.Max(0f, hit.distance - wallPadding);
        else
            CurrentSafeLeft = -maxScanDistance;
        
        Debug.DrawRay(origin, transform.right * CurrentSafeRight, Color.green);
        Debug.DrawRay(origin, -transform.right * Mathf.Abs(CurrentSafeLeft), Color.red);
    }
}