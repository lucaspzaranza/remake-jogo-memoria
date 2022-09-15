using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class ElapsedTime : MonoBehaviour
{
    public static event Action OnTimeEnded;
    public static event Action<int> OnTimeUpdated;

    [SerializeField] private TextMeshProUGUI _elapsedTimeTMPRO;
    [SerializeField] private int _elapsedTimeVal;

    private float _timeCounter;

    private void Start()
    {
        Invoke(nameof(Setup), 0.5f);
    }

    public void Setup()
    {
        _elapsedTimeVal = GameController.instance.TimeInSeconds;
        _timeCounter = 0f;
        _elapsedTimeTMPRO.text = "01:00";
    }

    private void FixedUpdate()
    {
        if (!GameController.instance.IsInGame)
            return;

        _timeCounter += Time.fixedDeltaTime;

        if(_timeCounter >= 1f) // 1 second
        {
            _timeCounter = 0f;
            _elapsedTimeVal--;

            _elapsedTimeTMPRO.text = (_elapsedTimeVal < 10) ? $"00:0{_elapsedTimeVal}" : $"00:{_elapsedTimeVal}";
            OnTimeUpdated?.Invoke(_elapsedTimeVal);

            if (_elapsedTimeVal == 0)
                OnTimeEnded?.Invoke();
        }
    }
}
