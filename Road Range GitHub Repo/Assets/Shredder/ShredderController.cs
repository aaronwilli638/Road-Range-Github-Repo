using UnityEngine;
using System.Collections.Generic;

public class ShredderController : MonoBehaviour
{
    public EnemyMasterTarget target;
    public float followDistance = 15f;
    public float rotationSpeed = 10f;

    private List<Vector3> breadcrumbs = new List<Vector3>();
    private Vector3 startPosition;
    private Quaternion startRotation;

    private void OnEnable()
    {
        startPosition = transform.position;
        startRotation = transform.rotation;
        
        if (target != null)
        {
            breadcrumbs.Clear();
            breadcrumbs.Add(target.transform.position);
        }
    }

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 targetPos = target.transform.position;
        if (breadcrumbs.Count == 0 || Vector3.Distance(breadcrumbs[breadcrumbs.Count - 1], targetPos) > 0.1f)
        {
            breadcrumbs.Add(targetPos);
        }

        float distanceTraveled = 0f;

        for (int i = breadcrumbs.Count - 1; i > 0; i--)
        {
            Vector3 p1 = breadcrumbs[i];
            Vector3 p2 = breadcrumbs[i - 1];
            float segmentLength = Vector3.Distance(p1, p2);

            if (distanceTraveled + segmentLength >= followDistance)
            {
                float remaining = followDistance - distanceTraveled;
                float t = remaining / segmentLength;

                Vector3 desiredPosition = Vector3.Lerp(p1, p2, t);
                transform.position = desiredPosition;

                Vector3 direction = (p1 - p2).normalized;
                if (direction != Vector3.zero)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * rotationSpeed);
                }

                if (i > 1)
                {
                    breadcrumbs.RemoveRange(0, i - 1);
                }
                return;
            }

            distanceTraveled += segmentLength;
        }

        transform.position = startPosition;
        transform.rotation = startRotation;
    }
}