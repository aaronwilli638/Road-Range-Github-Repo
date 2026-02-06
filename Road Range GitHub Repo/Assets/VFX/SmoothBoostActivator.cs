using UnityEngine;

public class SmoothBoostActivator : MonoBehaviour
{
    public SmoothCar carController;

    [Header("Boost Objects")]
    public GameObject[] boostFX;

    private bool _wasBoosting;

    void Start()
    {
        if (carController == null)
        {
            carController = GetComponentInParent<SmoothCar>();
        }

        UpdateActivation(false);
    }

    void Update()
    {
        if (carController == null) return;

        bool isBoosting = carController.IsBoosting;

        if (isBoosting != _wasBoosting)
        {
            UpdateActivation(isBoosting);
            _wasBoosting = isBoosting;
        }
    }

    private void UpdateActivation(bool active)
    {
        if (boostFX == null) return;

        for (int i = 0; i < boostFX.Length; i++)
        {
            if (boostFX[i] != null)
            {
                boostFX[i].SetActive(active);
            }
        }
    }
}