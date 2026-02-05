using UnityEngine;

public class CarBodyTilt : MonoBehaviour
{
    public SmoothCar carController;
    public Transform visualBody;
    public Rigidbody carRb;

    public float tiltFactor = 1.25f;
    public float smoothTime = 0.1f;
    public float maxTiltAngle = 15f;
    public float deadZoneAngle = 2.0f;

    private float currentTilt;
    private float tiltVelocity;
    private Quaternion initialRotation;

    void Start()
    {
        if (!carController) carController = GetComponent<SmoothCar>();
        if (!carRb) carRb = GetComponent<Rigidbody>();
        
        if (visualBody) initialRotation = visualBody.localRotation;
    }

    void Update()
    {
        if (!carController || !visualBody || !carRb) return;

        float steer = carController.SteerInput;
        float speed = carRb.linearVelocity.magnitude;

        float targetTilt = -steer * speed * tiltFactor;

        if (Mathf.Abs(targetTilt) < deadZoneAngle)
        {
            targetTilt = 0f;
        }

        targetTilt = Mathf.Clamp(targetTilt, -maxTiltAngle, maxTiltAngle);

        currentTilt = Mathf.SmoothDamp(currentTilt, targetTilt, ref tiltVelocity, smoothTime);

        visualBody.localRotation = Quaternion.Euler(0f, 0f, currentTilt) * initialRotation;
    }
}