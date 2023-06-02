using System.Linq;
using UnityEngine;

public class UIController : MonoBehaviour
{
    [SerializeField] private RankingDataSO _rankingData;
    public RankingDataSO RankingData => _rankingData;

    public void ResetRanking()
    {
        for (int i = 0; i < _rankingData.RankingRowsDataList.Count; i++)
        {
            _rankingData.RankingRowsDataList[i].Name = string.Empty;
            _rankingData.RankingRowsDataList[i].Score = 0;
            _rankingData.RankingRowsDataList[i].Number = i + 1;
        }
    }

    public void SaveRankingData(RankingRowData rankingRowData)
    {
        RankingRowData currentRowData = _rankingData.RankingRowsDataList
            .SingleOrDefault(rowData => rowData.Number == rankingRowData.Number);

        currentRowData.Name = rankingRowData.Name;
        currentRowData.Score = rankingRowData.Score;
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}