using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RankingRow : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _nameTMPro; 
    public TextMeshProUGUI NameTMPro => _nameTMPro;

    [SerializeField] private TextMeshProUGUI _scoreTMPro;
    public TextMeshProUGUI ScoreTMPro => _scoreTMPro;
}