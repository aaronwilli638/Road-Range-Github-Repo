using UnityEngine;

public class CarCamera : MonoBehaviour
{
    [Header("Targets")]
    // I set the camera target as the car right now (I'm lazy) but we could play with the camera focal point later if we want
    public Transform target; 
    public Rigidbody targetRb;

    [Header("Settings")]
    public Vector3 offset = new Vector3(0, 5, -10);
    public float recenteringMultiplier = 2f;
    
    // You can set the side to side and forward/back follow speeds independently. Gives the car room to move laterally but not forward and back.
    [Header("Follow Intensity")]
    public float lateralSpeed = 5f; 
    public float longitudinalSpeed = 20f;
    public float verticalSpeed = 10f;

    [Header("Rotation")]
    public float rotationSpeed = 5f;

    private Car carController;

    void Start()
    {
        if (target != null)
        {
            carController = target.GetComponent<Car>();
        }
    }

    void FixedUpdate()
    {
        if (!target) return;

        float currentLateralSpeed = lateralSpeed;
        float currentRotationSpeed = rotationSpeed;

        if (carController != null && Mathf.Abs(carController.SteerInput) <= 0.1f) // Recenter faster when not steering. Super janky
        {
            currentLateralSpeed *= recenteringMultiplier;
            currentRotationSpeed *= recenteringMultiplier;
        }

        Vector3 currentLocalPos = target.InverseTransformPoint(transform.position); // Find camera position relative to car
        Vector3 targetLocalPos = offset;
        
        // Ease into each axis independently
        float newX = Mathf.Lerp(currentLocalPos.x, targetLocalPos.x, currentLateralSpeed * Time.fixedDeltaTime);
        float newY = Mathf.Lerp(currentLocalPos.y, targetLocalPos.y, verticalSpeed * Time.fixedDeltaTime);
        float newZ = Mathf.Lerp(currentLocalPos.z, targetLocalPos.z, longitudinalSpeed * Time.fixedDeltaTime);

        transform.position = target.TransformPoint(new Vector3(newX, newY, newZ));

        Vector3 lookDirection;

        if (targetRb != null && targetRb.linearVelocity.magnitude > 2f)
        {
            Vector3 velocityHeading = targetRb.linearVelocity.normalized;
            Vector3 facingHeading = target.forward; // Set camera based on car's velocity instead of car's orientation. This is the secret sauce
            lookDirection = Vector3.Lerp(facingHeading, velocityHeading, 0.3f); 
        }
        else
        {
            lookDirection = target.forward;
        }

        Quaternion targetRotation = Quaternion.LookRotation(lookDirection, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, currentRotationSpeed * Time.fixedDeltaTime);
    }
}