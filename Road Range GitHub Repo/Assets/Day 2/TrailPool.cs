using UnityEngine;
using System.Collections.Generic;

public class TrailPool : MonoBehaviour
{
    [SerializeField] private BulletTrail trailPrefab;
    [SerializeField] private int defaultPoolSize = 10;
    private Queue<BulletTrail> pool = new Queue<BulletTrail>();

    private void Start()
    {
        for (int i = 0; i < defaultPoolSize; i++)
        {
            CreateNewTrail();
        }
    }

    private BulletTrail CreateNewTrail()
    {
        BulletTrail instance = Instantiate(trailPrefab, transform);
        instance.gameObject.SetActive(false);
        pool.Enqueue(instance);
        return instance;
    }

    public BulletTrail GetTrail()
    {
        if (pool.Count == 0)
        {
            CreateNewTrail();
        }
        return pool.Dequeue();
    }

    public void ReturnTrailToPool(BulletTrail trail)
    {
        trail.gameObject.SetActive(false);
        pool.Enqueue(trail);
    }
}