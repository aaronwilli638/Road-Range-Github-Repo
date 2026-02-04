using UnityEngine;

public class DestroyOnContact : MonoBehaviour
{
    [SerializeField] private LayerMask targetLayer;
    [SerializeField] private GameObject deathFxPrefab;
    [SerializeField] private float fxLifetime = 2f;

    private void OnCollisionEnter(Collision collision)
    {
        if (((1 << collision.gameObject.layer) & targetLayer) != 0)
        {
            if (deathFxPrefab != null)
            {
                GameObject fx = Instantiate(deathFxPrefab, transform.position, Quaternion.identity);
                Destroy(fx, fxLifetime);
            }

            Destroy(gameObject);
        }
    }
}