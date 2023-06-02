using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RankingData", menuName = "ScriptableObjects/Ranking Data", order = 0)]
public class RankingDataSO : ScriptableObject
{
    [SerializeField] private List<RankingRowData> _rankingRowsDataList;
    public List<RankingRowData> RankingRowsDataList => _rankingRowsDataList;
}
