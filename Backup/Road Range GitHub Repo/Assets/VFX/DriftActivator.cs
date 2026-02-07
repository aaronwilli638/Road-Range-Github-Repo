using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class DriftActivator : MonoBehaviour
{
    public Car2 carController;

    private ParticleSystem _ps;
    private ParticleSystem.EmissionModule _emission;

    void Start()
    {
        _ps = GetComponent<ParticleSystem>();
        _emission = _ps.emission;
        
        _emission.enabled = false;

        if (carController == null)
        {
            carController = GetComponentInParent<Car2>();
        }
    }

    void Update()
    {
        if (carController == null) return;

        bool shouldEmit = carController.IsDrifting;

        if (_emission.enabled != shouldEmit)
        {
            _emission.enabled = shouldEmit;
        }
    }
}