using UnityEngine;
using System.Collections.Generic;

public class WaypointVisualizer : MonoBehaviour
{
    public Transform waypointParent;
    public Color lineColor = Color.yellow;
    public bool closeLoop = true;

    private void OnDrawGizmos()
    {
        Transform root = waypointParent != null ? waypointParent : transform;
        if (root.childCount < 2) return;

        Gizmos.color = lineColor;
        List<Transform> points = new List<Transform>();
        
        foreach (Transform child in root)
        {
            points.Add(child);
        }

        for (int i = 0; i < points.Count - 1; i++)
        {
            Gizmos.DrawLine(points[i].position, points[i+1].position);
        }

        if (closeLoop && points.Count > 1)
        {
            Gizmos.DrawLine(points[points.Count - 1].position, points[0].position);
        }
    }
}