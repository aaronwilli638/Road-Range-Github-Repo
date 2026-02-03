using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ObjectMover : MonoBehaviour
{
    public Vector3 direction = Vector3.forward;
    public float speed = 5f;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.isKinematic = true;
    }

    void FixedUpdate()
    {
        Vector3 moveStep = direction.normalized * speed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + moveStep);
    }
}