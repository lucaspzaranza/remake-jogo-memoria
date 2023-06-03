using System;
using UnityEngine;

[Serializable]
public class RankingRowData
{
    [SerializeField] private int _number;
    public int Number
    {
        get => _number;
        set => _number = value;
    }

    [SerializeField] private string _name;
    public string Name
    {
        get => _name;
        set => _name = value;
    }

    [SerializeField] private int _score;
    public int Score
    {
        get => _score;
        set => _score = value;
    }

    public RankingRowData(string newName, int newScore)
    {
        _name = newName;
        _score = newScore;
    }
}