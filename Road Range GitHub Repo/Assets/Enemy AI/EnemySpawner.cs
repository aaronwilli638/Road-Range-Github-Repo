using UnityEngine;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    public Transform enemyPoolParent;
    public Transform target;
    public Transform playerTransform;
    
    [Header("Spawn Settings")]
    public int enemyQuota = 5;
    public float spawnDistance = 30f;
    public float initialSpeed = 20f;
    public float spawnDelay = 3f;
    public int enemyLayerIndex;

    [Header("Track Detection")]
    public LayerMask wallLayer;
    public float maxScanDistance = 20f;
    public float wallPadding = 2f;
    public float playerZoneSize = 3f;

    private Queue<GameObject> pool = new Queue<GameObject>();
    private float spawnTimer;

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
        int activeCount = 0;
        foreach(Transform child in enemyPoolParent)
        {
            if (child.gameObject.activeInHierarchy) activeCount++;
        }

        if (activeCount < enemyQuota)
        {
            spawnTimer -= Time.deltaTime;
            if (spawnTimer <= 0f)
            {
                SpawnEnemy();
                spawnTimer = spawnDelay;
            }
        }
    }

    public void SpawnEnemy()
    {
        if (playerTransform == null || target == null) return;

        GameObject enemy = null;

        if (pool.Count > 0)
        {
            enemy = pool.Dequeue();
        }

        if (enemy == null || enemy.activeInHierarchy)
        {
            foreach(Transform child in enemyPoolParent)
            {
                if (!child.gameObject.activeInHierarchy)
                {
                    enemy = child.gameObject;
                    break;
                }
            }
        }

        if (enemy == null) return;

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

        if (safeLeft < -playerZoneSize)
            validRanges.Add(new Vector2(safeLeft, -playerZoneSize));
        
        if (safeRight > playerZoneSize)
            validRanges.Add(new Vector2(playerZoneSize, safeRight));

        if (validRanges.Count == 0)
        {
            ReturnEnemy(enemy);
            return;
        }

        Vector2 selectedRange = validRanges[Random.Range(0, validRanges.Count)];
        float finalOffset = Random.Range(selectedRange.x, selectedRange.y);

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

        EnemyAINavigator nav = enemy.GetComponent<EnemyAINavigator>();
        if (nav != null)
        {
            nav.Initialize(this);
        }

        enemy.SetActive(true);
    }

    public void ReturnEnemy(GameObject enemy)
    {
        enemy.SetActive(false);
        if (!pool.Contains(enemy))
        {
            pool.Enqueue(enemy);
        }
    }
}