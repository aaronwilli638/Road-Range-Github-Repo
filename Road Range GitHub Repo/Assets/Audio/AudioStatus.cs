using UnityEngine;

public class AudioStatus : MonoBehaviour
{
    [SerializeField] private SmoothCar smoothCar;
    public int shredderDistance = 50;
    public bool slickMode = false;
    public int lap = 1;
    private Rigidbody carRigidbody;

    private void Awake()
    {
        if (smoothCar != null)
        {
            carRigidbody = smoothCar.GetComponent<Rigidbody>();
        }
    }

    public float GetSpeed()
    {
        if (carRigidbody != null)
        {
            return carRigidbody.linearVelocity.magnitude;
        }
        return 0f;
    }

    public bool GetDriftingStatus()
    {
        if (smoothCar != null)
        {
            return smoothCar.IsDrifting;
        }
        return false;
    }

    public bool GetBoostingStatus()
    {
        if (smoothCar != null)
        {
            return smoothCar.IsBoosting;
        }
        return false;
    }

    public int GetDistanceToShredder()
    {
        return shredderDistance;
    }

    public bool GetSlickMode()
    {
        return slickMode;
    }

    public int GetLap()
    {
        return lap;
    }
}