using UnityEngine;
using System.IO;
using System.Text;
using System.Collections.Generic;

public class JitterLogger : MonoBehaviour
{
    public Transform carTransform;
    public Transform cameraTransform;
    public Transform visualMeshTransform;
    public int maxRows = 10000;

    private StringBuilder csvData = new StringBuilder();
    private int rowCount = 0;

    void Start()
    {
        csvData.AppendLine("Time,Frame,CarX,CarY,CarZ,CamX,CamY,CamZ,MeshX,MeshY,MeshZ");
    }

    void LateUpdate()
    {
        if (rowCount >= maxRows) return;

        float t = Time.time;
        int f = Time.frameCount;

        Vector3 carP = carTransform.position;
        Vector3 camP = cameraTransform.position;
        Vector3 meshP = visualMeshTransform != null ? visualMeshTransform.position : Vector3.zero;

        string line = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10}",
            t, f,
            carP.x, carP.y, carP.z,
            camP.x, camP.y, camP.z,
            meshP.x, meshP.y, meshP.z
        );

        csvData.AppendLine(line);
        rowCount++;
    }

    void OnApplicationQuit()
    {
        string filePath = Path.Combine(Application.persistentDataPath, "JitterLog.csv");
        File.WriteAllText(filePath, csvData.ToString());
        Debug.Log($"CSV saved to: {filePath}");
    }
}