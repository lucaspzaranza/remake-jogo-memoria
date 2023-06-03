using System.Linq;
using TMPro;
using UnityEngine;

public class UIController : MonoBehaviour
{
    [SerializeField] private TMP_InputField _nameInput;
    public TMP_InputField NameInput => _nameInput;

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

    public void SaveRankingData(string name, int score)
    {
        //var newData = new RankingRowData(name, Random.Range(0, 10000));
        var newData = new RankingRowData(name, score);
        newData.Number = -1;
        bool rankingInserted = false;

        for (int i = 0; i < _rankingData.RankingRowsDataList.Count; i++)
        {
            var data = _rankingData.RankingRowsDataList[i];

            if (newData.Score >= data.Score && !rankingInserted)
            {
                newData.Number = data.Number;
                int index = _rankingData.RankingRowsDataList.IndexOf(data);
                _rankingData.RankingRowsDataList.Insert(index, newData);
                _rankingData.RankingRowsDataList.RemoveAt(_rankingData.RankingRowsDataList.Count - 1);
                rankingInserted = true;

                print($"Added the score {newData.Score} at the position {newData.Number}.");
            }
            else if(rankingInserted)
                _rankingData.RankingRowsDataList[i].Number = i + 1;
        }

        if(newData.Number == -1)
            Debug.LogWarning($"The score {newData.Score} isn't at the Top {_rankingData.RankingRowsDataList.Count}.");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}