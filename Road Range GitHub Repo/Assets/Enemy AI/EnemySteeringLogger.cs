using UnityEngine;
using System.IO;
using System.Reflection;
using System.Text;

public class EnemySteeringLogger : MonoBehaviour
{
    private EnemyCarController carController;
    private EnemyAINavigator navigator;
    private FieldInfo steerField;
    private string filePath;
    private StringBuilder sb = new StringBuilder();

    private void Start()
    {
        carController = GetComponent<EnemyCarController>();
        navigator = GetComponent<EnemyAINavigator>();
        
        steerField = typeof(EnemyCarController).GetField("currentSteer", BindingFlags.NonPublic | BindingFlags.Instance);

        string fileName = $"SteeringLog_TargetDebug_{gameObject.name}_{System.DateTime.Now:HH-mm-ss}.csv";
        filePath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop), fileName);

        sb.AppendLine("Time,PosX,PosZ,CarFwdX,CarFwdZ,TargetX,TargetZ,DirToTargetX,DirToTargetZ,AlignmentDot,SteerInput,Speed");
        File.WriteAllText(filePath, sb.ToString());
        sb.Clear();
    }

    private void FixedUpdate()
    {
        if (carController == null || navigator == null) return;

        float steerValue = 0f;
        if (steerField != null)
        {
            steerValue = (float)steerField.GetValue(carController);
        }

        Vector3 targetPos = Vector3.zero;
        Vector3 dirToTarget = Vector3.zero;
        float alignment = 0f;

        if (navigator.masterTarget != null)
        {
            targetPos = navigator.masterTarget.transform.position;
            dirToTarget = (targetPos - transform.position).normalized;
            alignment = Vector3.Dot(transform.forward, dirToTarget);
        }

        string line = string.Format("{0:F4},{1:F4},{2:F4},{3:F4},{4:F4},{5:F4},{6:F4},{7:F4},{8:F4},{9:F4},{10:F4},{11:F4}",
            Time.time,
            transform.position.x,
            transform.position.z,
            transform.forward.x,
            transform.forward.z,
            targetPos.x,
            targetPos.z,
            dirToTarget.x,
            dirToTarget.z,
            alignment,
            steerValue,
            carController.CurrentSpeed
        );

        sb.AppendLine(line);

        if (Time.frameCount % 60 == 0)
        {
            File.AppendAllText(filePath, sb.ToString());
            sb.Clear();
        }
    }

    private void OnDisable()
    {
        if (sb.Length > 0)
        {
            File.AppendAllText(filePath, sb.ToString());
            sb.Clear();
        }
        Debug.Log($"Log saved to: {filePath}");
    }
}