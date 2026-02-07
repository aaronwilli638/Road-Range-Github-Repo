using UnityEngine;
using System.Collections.Generic;

public class ShredderController : MonoBehaviour
{
    [Header("Settings")]
    public Transform target;
    public float minSpeed = 5f;
    public float catchUpMultiplier = 2f;
    public float headStartDistance = 10f;
    
    [Header("Dynamic Distance")]
    public float followDistance = 15f;
    public float distanceDecayRate = 1.0f;
    public float maxFollowDistance = 25f;

    public float rotationSpeed = 10f;

    private List<Vector3> breadcrumbs = new List<Vector3>();
    private float currentSpeed;
    private bool hasStarted;

    private void OnEnable()
    {
        if (target != null)
        {
            breadcrumbs.Clear();
            breadcrumbs.Add(target.position);
        }
        hasStarted = false;
    }

    private void Update()
    {
        followDistance -= distanceDecayRate * Time.deltaTime;
        followDistance = Mathf.Max(0f, followDistance);
    }

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 targetPos = target.position;
        if (breadcrumbs.Count == 0 || Vector3.Distance(breadcrumbs[breadcrumbs.Count - 1], targetPos) > 0.1f)
        {
            breadcrumbs.Add(targetPos);
        }

        float distanceToTarget = 0f;
        if (breadcrumbs.Count > 0)
        {
            distanceToTarget += Vector3.Distance(transform.position, breadcrumbs[0]);
            for (int i = 0; i < breadcrumbs.Count - 1; i++)
            {
                distanceToTarget += Vector3.Distance(breadcrumbs[i], breadcrumbs[i + 1]);
            }
            distanceToTarget += Vector3.Distance(breadcrumbs[breadcrumbs.Count - 1], targetPos);
        }
        else
        {
            distanceToTarget = Vector3.Distance(transform.position, targetPos);
        }

        if (!hasStarted)
        {
            if (distanceToTarget < headStartDistance)
            {
                return;
            }
            hasStarted = true;
        }

        float desiredSpeed;

        float killThreshold = 4f; 
        
        if (distanceToTarget < killThreshold)
        {
             desiredSpeed = minSpeed * catchUpMultiplier; 
        }
        else
        {
            float error = distanceToTarget - followDistance;
            desiredSpeed = minSpeed + (error * catchUpMultiplier);
        }
        
        currentSpeed = Mathf.Max(minSpeed, desiredSpeed);

        MoveAlongPath(currentSpeed * Time.deltaTime);
    }

    public void PushBack(float amount)
    {
        followDistance += amount;
        followDistance = Mathf.Min(followDistance, maxFollowDistance);
    }

    private void MoveAlongPath(float distanceToMove)
    {
        while (distanceToMove > 0 && breadcrumbs.Count > 0)
        {
            Vector3 nextPoint = breadcrumbs[0];
            float distToNextPoint = Vector3.Distance(transform.position, nextPoint);

            Vector3 direction = (nextPoint - transform.position).normalized;
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
            }

            if (distanceToMove >= distToNextPoint)
            {
                transform.position = nextPoint;
                distanceToMove -= distToNextPoint;
                breadcrumbs.RemoveAt(0);
            }
            else
            {
                transform.position = Vector3.MoveTowards(transform.position, nextPoint, distanceToMove);
                distanceToMove = 0;
            }
        }
        
        if (breadcrumbs.Count == 0 && target != null)
        {
             transform.position = target.position;
        }
    }
}