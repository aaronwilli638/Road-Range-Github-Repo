using UnityEngine;
using System.Collections.Generic;

public class ShredderController : MonoBehaviour
{
    public EnemyMasterTarget target;
    public float followDistance = 15f;
    public float catchUpSpeedMultiplier = 1.5f;

    private List<Vector3> breadcrumbs = new List<Vector3>();
    private Vector3 lastRecordedPosition;

    private void Start()
    {
        if (target != null)
        {
            lastRecordedPosition = target.transform.position;
            breadcrumbs.Add(lastRecordedPosition);
        }
    }

    private void Update()
    {
        if (target == null) return;

        float distToTarget = Vector3.Distance(lastRecordedPosition, target.transform.position);
        if (distToTarget > 0.1f)
        {
            breadcrumbs.Add(target.transform.position);
            lastRecordedPosition = target.transform.position;
        }

        float currentPathLength = 0f;
        
        if (breadcrumbs.Count > 0)
        {
            currentPathLength += Vector3.Distance(transform.position, breadcrumbs[0]);
        }
        
        for (int i = 0; i < breadcrumbs.Count - 1; i++)
        {
            currentPathLength += Vector3.Distance(breadcrumbs[i], breadcrumbs[i + 1]);
        }
        
        if (breadcrumbs.Count > 0)
        {
             currentPathLength += Vector3.Distance(breadcrumbs[breadcrumbs.Count - 1], target.transform.position);
        }
        else
        {
             currentPathLength += Vector3.Distance(transform.position, target.transform.position);
        }

        if (currentPathLength > followDistance && breadcrumbs.Count > 0)
        {
            Vector3 destination = breadcrumbs[0];
            Vector3 direction = (destination - transform.position).normalized;

            if (direction != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(direction);

            float speed = target.CurrentSpeed;
            if (currentPathLength > followDistance * 1.2f) speed *= catchUpSpeedMultiplier;

            transform.position = Vector3.MoveTowards(transform.position, destination, speed * Time.deltaTime);

            if (Vector3.Distance(transform.position, destination) < 0.1f)
            {
                breadcrumbs.RemoveAt(0);
            }
        }
    }
}