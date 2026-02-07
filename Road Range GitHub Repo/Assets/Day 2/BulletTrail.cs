using UnityEngine;
using System.Collections;
using System;

[RequireComponent(typeof(LineRenderer))]
public class BulletTrail : MonoBehaviour
{
    private LineRenderer lineRenderer;
    private float fadeDuration;
    private Action<BulletTrail> returnToPoolAction;
    private Transform startTransform;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
    }

    public void Init(Transform startPointRef, Vector3 endPoint, float duration, Action<BulletTrail> returnAction)
    {
        startTransform = startPointRef;
        
        lineRenderer.SetPosition(0, startTransform.position);
        lineRenderer.SetPosition(1, endPoint);
        
        // Reset alpha immediately
        Color c = lineRenderer.startColor; c.a = 1f; lineRenderer.startColor = c;
        c = lineRenderer.endColor; c.a = 1f; lineRenderer.endColor = c;

        fadeDuration = duration;
        returnToPoolAction = returnAction;
        
        gameObject.SetActive(true);
        StartCoroutine(FadeTrail());
    }

    private IEnumerator FadeTrail()
    {
        float timer = 0;
        Color startColor = lineRenderer.startColor;
        Color endColor = lineRenderer.endColor;

        while (timer < fadeDuration)
        {
            if (startTransform != null)
            {
                lineRenderer.SetPosition(0, startTransform.position);
            }

            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);
            
            startColor.a = alpha;
            endColor.a = alpha;
            
            lineRenderer.startColor = startColor;
            lineRenderer.endColor = endColor;
            
            yield return null;
        }

        startColor.a = 1f;
        endColor.a = 1f;
        lineRenderer.startColor = startColor;
        lineRenderer.endColor = endColor;
        
        returnToPoolAction?.Invoke(this);
    }
}