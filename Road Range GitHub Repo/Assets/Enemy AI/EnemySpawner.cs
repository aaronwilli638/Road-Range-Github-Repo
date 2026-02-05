using UnityEngine;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    public Transform enemyPoolParent;
    public Transform target;
    public Transform playerTransform;
    
    [Header("Spawn Settings")]
    public float spawnDistance = 30f;
    public float initialSpeed = 20f;
    public float spawnInterval = 3f;
    public int enemyLayerIndex;

    [Header("Track Detection")]
    public LayerMask wallLayer;
    public float maxScanDistance = 20f;
    public float wallPadding = 2f;
    public float playerZoneSize = 3f;

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
        if (pool.Count == 0 || playerTransform == null || target == null) return;

        Vector3 scanOrigin = playerTransform.position - (target.forward * spawnDistance);
        scanOrigin.y = target.position.y + 1f;

        float safeRight = maxScanDistance;
        float safeLeft = -maxScanDistance;
        RaycastHit hit;

        if (Physics.Raycast(scanOrigin, target.right, out hit, maxScanDistance, wallLayer))
            safeRight = Mathf.Max(0f, hit.distance - wallPadding);

        if (Physics.Raycast(scanOrigin, -target.right, out hit, maxScanDistance, wallLayer))
            safeLeft = -Mathf.Max(0f, hit.distance - wallPadding);

        List<Vector2> validRanges = new List<Vector2>();

        // Exclude the center area defined by playerZoneSize
        if (safeLeft < -playerZoneSize)
            validRanges.Add(new Vector2(safeLeft, -playerZoneSize));
        
        if (safeRight > playerZoneSize)
            validRanges.Add(new Vector2(playerZoneSize, safeRight));

        if (validRanges.Count == 0) return;

        Vector2 selectedRange = validRanges[Random.Range(0, validRanges.Count)];
        float finalOffset = Random.Range(selectedRange.x, selectedRange.y);

        GameObject enemy = pool.Dequeue();
        enemy.layer = enemyLayerIndex;
        enemy.transform.position = scanOrigin + (target.right * finalOffset);
        enemy.transform.rotation = Quaternion.LookRotation(-target.forward);

        Rigidbody rb = enemy.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = target.forward * initialSpeed;
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