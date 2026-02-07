using UnityEngine;

public class SpeedGetter : MonoBehaviour
{
    public AudioStatus audioStatus; // You need to point your code to audioStatus
    private float message;
    void Update()
    {
        if (audioStatus != null)
        {
            message = audioStatus.GetSpeed(); // You can call any of the audioStatus methods with audioStatus.MethodName(). This line stores the speed in message.
            Debug.Log("Current Speed: " + message); 
        }
    }
}