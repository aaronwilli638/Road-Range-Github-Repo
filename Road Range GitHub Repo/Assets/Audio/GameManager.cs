using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private Car2 playerCar;

    private int lapNumber;
    private bool inPitstop;
    private float playerHealth;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        playerCar = FindFirstObjectByType<Car2>();

        lapNumber = 1;
        inPitstop = false;
        playerHealth = 100f;
    }

    public float PlayerAcceleration
    {
        get { return playerCar != null ? playerCar.driveAcceleration : 0f; }
    }

    public Vector3 PlayerVelocity
    {
        get { return playerCar != null ? playerCar.GetComponent<Rigidbody>().linearVelocity : Vector3.zero; }
    }

    public bool PlayerDriftStatus
    {
        get { return playerCar != null && playerCar.IsDrifting; }
    }

    public int LapNumber
    {
        get { return lapNumber; }
    }

    public bool InPitstop
    {
        get { return inPitstop; }
    }

    public float PlayerHealth
    {
        get { return playerHealth; }
    }
}