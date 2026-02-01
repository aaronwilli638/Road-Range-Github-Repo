using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(ParticleSystem))]
public class DriftActivator : MonoBehaviour
{
    private ParticleSystem _ps;
    private ParticleSystem.EmissionModule _emission;

    void Start()
    {
        _ps = GetComponent<ParticleSystem>();
        _emission = _ps.emission;
        _emission.enabled = false;
    }

    void Update()
    {
        bool isSpaceHeld = false;

        if (Keyboard.current != null)
        {
            isSpaceHeld = Keyboard.current.spaceKey.isPressed;
        }

        if (_emission.enabled != isSpaceHeld)
        {
            _emission.enabled = isSpaceHeld;
        }
    }
}