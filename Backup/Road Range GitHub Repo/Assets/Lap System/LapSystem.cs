using UnityEngine;
using TMPro;

public class LapSystem : MonoBehaviour
{
    public int totalCheckpoints = 4;
    
    public TMP_Text totalTimerText;
    public TMP_Text lap1Text;
    public TMP_Text lap2Text;
    public TMP_Text lap3Text;

    private int _nextIndex = 1;
    public int _currentLap = 1;
    private float _currentLapTime;
    private float _totalRaceTime;

    private void Update()
    {
        float dt = Time.deltaTime;
        _currentLapTime += dt;
        _totalRaceTime += dt;

        if (totalTimerText != null)
        {
            totalTimerText.text = _totalRaceTime.ToString("F2");
        }

        if (_currentLap == 1 && lap1Text != null)
        {
            lap1Text.text = _currentLapTime.ToString("F2");
        }
        else if (_currentLap == 2 && lap2Text != null)
        {
            lap2Text.text = _currentLapTime.ToString("F2");
        }
        else if (_currentLap == 3 && lap3Text != null)
        {
            lap3Text.text = _currentLapTime.ToString("F2");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Checkpoint checkpoint = other.GetComponent<Checkpoint>();

        if (checkpoint != null)
        {
            if (checkpoint.index == _nextIndex)
            {
                if (_nextIndex == 0)
                {
                    MatchManager.Instance.RecordLap(_currentLapTime);
                    
                    _currentLap++;
                    _currentLapTime = 0f;
                    _nextIndex = 1;
                }
                else
                {
                    _nextIndex++;
                    if (_nextIndex >= totalCheckpoints)
                    {
                        _nextIndex = 0;
                    }
                }
            }
        }
    }
}