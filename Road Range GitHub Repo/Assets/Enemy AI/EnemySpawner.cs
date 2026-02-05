using UnityEngine;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    public Transform enemyPoolParent;
    public Transform target;
    public float spawnDistance = 30f;
    public float initialSpeed = 20f;
    public float spawnInterval = 3f;
    public int enemyLayerIndex;

    private Queue<GameObject> pool = new Queue<GameObject>();
    private float timer;

    private void Start()
    {
        foreach (Transform child in enemyPoolParent)
        {
            child.gameObject.SetActive(false);
            pool.Enqueue(child.gameObject);
        }
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            SpawnEnemy();
            timer = 0f;
        }
    }

    public void SpawnEnemy()
    {
        if (pool.Count == 0) return;

        GameObject enemy = pool.Dequeue();
        
        enemy.layer = enemyLayerIndex;

        Vector3 spawnPos = target.position - (target.forward * spawnDistance);
        spawnPos += target.right * Random.Range(-4f, 4f);
        spawnPos.y = target.position.y + 1f;

        enemy.transform.position = spawnPos;

        enemy.transform.rotation = Quaternion.LookRotation(spawnPos - target.position);

        Rigidbody rb = enemy.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = (target.position - spawnPos).normalized * initialSpeed;
            rb.angularVelocity = Vector3.zero;
        }

        EnemyCarController car = enemy.GetComponent<EnemyCarController>();
        if (car != null)
        {
            car.SetInputs(0f, 1f);
        }

        enemy.SetActive(true);
        pool.Enqueue(enemy);
    }
}