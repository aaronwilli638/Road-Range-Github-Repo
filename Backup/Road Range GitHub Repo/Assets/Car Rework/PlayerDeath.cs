using UnityEngine;

public class PlayerDeath : MonoBehaviour
{
    public string deathLayerName = "Death Layer";
    public GameObject deathScreen;

    private void OnCollisionEnter(Collision collision)
    {
        int deathLayerId = LayerMask.NameToLayer(deathLayerName);

        if (collision.gameObject.layer == deathLayerId)
        {
            FreezeTimeIndefinitely();
        }
    }

    private void FreezeTimeIndefinitely()
    {
        if (TimeManager.Instance != null)
        {
            deathScreen.SetActive(true);
            TimeManager.Instance.RequestFreeze(99999f);
        }
        else
        {
            Debug.LogWarning("No TimeManager found!");
        }
    }
}