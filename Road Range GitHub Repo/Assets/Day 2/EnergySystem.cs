using UnityEngine;
using TMPro;

public class EnergySystem : MonoBehaviour
{
    public TextMeshProUGUI energyText;
    
    public float maxEnergy = 100f;
    public float currentEnergy;

    public float boostCostPerSec = 30f;
    public float shootCost = 15f;

    public float driftRefillPerMeter = 0.2f;
    public bool passiveRegen = true;
    public float passiveRegenRate = 10f;
    public float regenDelay = 2f;

    private float lastActionTime;
    private int lastDisplayEnergy = -1;

    void Start()
    {
        currentEnergy = maxEnergy;
    }

    void Update()
    {
        if (passiveRegen && Time.time > lastActionTime + regenDelay)
        {
            Refill(passiveRegenRate * Time.deltaTime);
        }

        if (energyText != null)
        {
            int displayValue = Mathf.FloorToInt(currentEnergy);
            if (displayValue != lastDisplayEnergy)
            {
                energyText.text = displayValue.ToString();
                lastDisplayEnergy = displayValue;
            }
        }
    }

    public bool TryConsume(float amount)
    {
        if (currentEnergy >= amount)
        {
            currentEnergy -= amount;
            lastActionTime = Time.time;
            return true;
        }
        return false;
    }

    public void Refill(float amount)
    {
        currentEnergy = Mathf.Clamp(currentEnergy + amount, 0, maxEnergy);
    }
}