using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class UIElements
{
    [SerializeField] private Button _resetButton;
    [SerializeField] private GameObject _inGameScreen;
    [SerializeField] private GameObject _endGameScreen;
    [SerializeField] private TextMeshProUGUI _pairs;
    [SerializeField] private TextMeshProUGUI _time;
    [SerializeField] private TextMeshProUGUI _errors;
    [SerializeField] private TextMeshProUGUI _totalScore;

    public TextMeshProUGUI Pairs => _pairs;
    public TextMeshProUGUI Time => _time;
    public TextMeshProUGUI Errors => _errors;
    public TextMeshProUGUI TotalScore => _totalScore;

    public Button ResetButton => _resetButton;

    public void InGameScreenActivation(bool val)
    {
        _inGameScreen.SetActive(val);
    }

    public void EndGameScreenActivation(bool val)
    {
        _endGameScreen.SetActive(val);
    }

    public void StartGame()
    {
        GameController.instance.CallStartGame();
    }
}
