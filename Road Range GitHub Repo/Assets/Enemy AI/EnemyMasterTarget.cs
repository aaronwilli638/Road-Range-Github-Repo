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
    public LayerMask groundLayer;
    public int offroadLayerIndex = 1;
    public float maxScanDistance = 20f;
    public float scanStep = 1.0f;
    public float edgePadding = 2f;

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

        ScanTerrainWidth();
    }

    private void ScanTerrainWidth()
    {
        CurrentSafeRight = maxScanDistance;
        CurrentSafeLeft = -maxScanDistance;

        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up * 5f, Vector3.down, out hit, 20f, groundLayer))
        {
            Terrain terrain = hit.collider.GetComponent<Terrain>();
            if (terrain != null)
            {
                for (float d = 0; d < maxScanDistance; d += scanStep)
                {
                    Vector3 probe = transform.position + (transform.right * d);
                    if (IsOffroad(probe, terrain))
                    {
                        CurrentSafeRight = Mathf.Max(0, d - edgePadding);
                        break;
                    }
                }

                for (float d = 0; d < maxScanDistance; d += scanStep)
                {
                    Vector3 probe = transform.position - (transform.right * d);
                    if (IsOffroad(probe, terrain))
                    {
                        CurrentSafeLeft = -Mathf.Max(0, d - edgePadding);
                        break;
                    }
                }
            }
        }
    }

    private bool IsOffroad(Vector3 worldPos, Terrain terrain)
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

        return maxIndex == offroadLayerIndex;
    }
}