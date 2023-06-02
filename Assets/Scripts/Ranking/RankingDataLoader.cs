using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RankingDataLoader : MonoBehaviour
{
    [SerializeField] private UIController _UIController;
    [SerializeField] private List<RankingRow> _rows;

    private void OnEnable()
    {
        LoadRankings();
    }

    private void LoadRankings()
    {
        for (int i = 0; i < _rows.Count; i++)
        {
            if (i >= _UIController.RankingData.RankingRowsDataList.Count)
                return;

            RankingRowData data = _UIController.RankingData.RankingRowsDataList[i];
            _rows[i].NameTMPro.text = $"{data.Number}. {data.Name}";
            _rows[i].ScoreTMPro.text = $"{data.Score} pts";
        }
    }
}
