using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class SmoothDriftActivator : MonoBehaviour
{
    public SmoothCar carController;

    [Header("Emission Settings")]
    public float particlesPerMeter = 5.0f;
    public float minDriftFactor = 0.05f;

    private ParticleSystem _ps;
    private ParticleSystem.EmissionModule _emission;

    void Start()
    {
        _ps = GetComponent<ParticleSystem>();
        _emission = _ps.emission;

        if (carController == null)
        {
            carController = GetComponentInParent<SmoothCar>();
        }

        _emission.rateOverTime = 0f;
        _emission.enabled = true;
    }

    void Update()
    {
        if (carController == null) return;

        bool shouldEmit = carController.IsDrifting && 
                          carController.IsGrounded && 
                          carController.DriftFactor > minDriftFactor;

        if (shouldEmit)
        {
            _emission.rateOverDistance = particlesPerMeter * carController.DriftFactor;
        }
        else
        {
            _emission.rateOverDistance = 0f;
        }
    }
}