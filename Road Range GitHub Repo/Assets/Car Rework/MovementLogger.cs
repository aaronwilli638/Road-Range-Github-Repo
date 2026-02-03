using UnityEngine;
using System.IO;
using System.Text;
using System.Collections.Generic;

public class MovementLogger : MonoBehaviour
{
    private List<string> logData = new List<string>();
    private Vector3 lastPosition;
    private float startTime;

    void Start()
    {
        lastPosition = transform.position;
        startTime = Time.time;
        logData.Add("Time,Frame,DeltaTime,PositionX,PositionY,PositionZ,DistTraveled");
    }

    void Update()
    {
        float dist = Vector3.Distance(transform.position, lastPosition);
        string line = $"{Time.time - startTime},{Time.frameCount},{Time.deltaTime},{transform.position.x},{transform.position.y},{transform.position.z},{dist}";
        logData.Add(line);
        lastPosition = transform.position;
    }

    void OnDisable()
    {
        string path = Path.Combine(Application.persistentDataPath, "MovementLog.csv");
        File.WriteAllLines(path, logData);
        Debug.Log($"Log saved to: {path}");
    }
}