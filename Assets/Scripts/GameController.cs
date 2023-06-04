using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public static GameController instance;

    public Action<int> OnTimeChanged;

    [Header("Controllers")]
    [SerializeField] private UIController _UIController;

    [Space]
    [Header("Card Customization")]
    [SerializeField] private Sprite _cardBack;
    [SerializeField] private List<Sprite> _cardsSprites;
    [SerializeField] private List<Card> _cards;

    [Space]
    [Header("Game Rules")]
    [SerializeField] private int _score;
    [SerializeField] private int _matchScore;
    [SerializeField] private int _secondsScore;
    [SerializeField] private int _errorPenalty;
    [SerializeField] private int _matchDuration;
    [SerializeField] private float _memorizeTime;
    [SerializeField] private float _timeToUnflip;
    [SerializeField] private float _timeToEndGame;

    private List<Card> _flippedCards = new List<Card>();
    private bool _matchBegun;
    private int _pairCounter;
    private int _errorCounter;
    private int _numOfTries;
    private int _time;
    private float _timeCounter;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        SetCardsSprites();
    }

    private void Update()
    {
        if(_matchBegun && _timeCounter >= 0f)
        {
            _timeCounter -= Time.deltaTime;
            _time = Mathf.RoundToInt(_timeCounter);
            OnTimeChanged?.Invoke(_time);
        }
    }

    public void SetCardsSprites()
    {
        int pairCounter = 0;
        for (int i = 0; i < _cards.Count; i++)
        {
            if (i > 1 && i % 2 == 0)
                pairCounter++;

            _cards[i].SetCardImages(_cardBack, _cardsSprites[pairCounter]);
        }
    }

    public void StartGame()
    {
        ShuffleCards();
        StartCoroutine(FlipAllCards());       
    }

    private IEnumerator FlipAllCards()
    {
        //Flipping...
        foreach (var card in _cards)
        {
            card.ForceFlip();
            card.SetCanFlip(false);
        }

        _UIController.GameHUD.SetActive(false);

        // Waiting the player memorize the positions...
        yield return new WaitForSeconds(_memorizeTime);

        _UIController.GameHUD.SetActive(true);

        // ...and unflipping
        foreach (var card in _cards)
        {
            card.SetCanFlip(true);
            card.ForceFlip();
        }

        Card.OnCardFlipped += HandleOnCardFlipped;
        _timeCounter = _matchDuration;
        _matchBegun = true;
    }

    public void ShuffleCards()
    {
        foreach (var card in _cards)
        {
            int random = UnityEngine.Random.Range(0, _cards.Count);
            card.transform.SetSiblingIndex(random);
        }
    }

    public void HandleOnCardFlipped(Card card)
    {
        if (card.CardState == CardState.Back || _flippedCards.Count > 2)
            return;

        _flippedCards.Add(card);        

        if(_flippedCards.Count == 2)
        {
            _numOfTries++;
            _UIController.SetNumOfTries(_numOfTries);

            bool areEqual = _flippedCards[0].CardImage.sprite.
                Equals(_flippedCards[1].CardImage.sprite);

            if(!areEqual)
            {
                _errorCounter++;
                StartCoroutine(UnflipPairOfFlippedCards(_timeToUnflip));
            }
            else
            {
                _score += _matchScore;
                _UIController.SetScore(_score);
                _pairCounter++;
                _flippedCards.ForEach(card =>
                {
                    card.SetCanFlip(false);
                });
                _flippedCards = new List<Card>();

                if (_pairCounter == _cards.Count / 2)
                    StartCoroutine(EndGame());
            }
        }
    }

    private int GetTotalScore()
    {
        int totalSecondsScore = _time * _secondsScore;
        int totalErrorsScore = _errorCounter * _errorPenalty;
        int total = totalSecondsScore + totalErrorsScore + _score;

        return total;
    }

    public IEnumerator EndGame()
    {
        _matchBegun = false;
        _UIController.SaveRankingData(_UIController.NameInput.text, GetTotalScore());
        _UIController.GameHUD.SetActive(false);

        yield return new WaitForSeconds(_timeToEndGame);

        _UIController.InGameUI.SetActive(false);
        _UIController.EndGameUI.SetActive(true);
    }

    private IEnumerator UnflipPairOfFlippedCards(float timeToWait)
    {
        yield return new WaitForSeconds(timeToWait);
        foreach (var card in _flippedCards)
        {
            card.ForceFlip();
        }
        _flippedCards = new List<Card>();
    }

    public void RestartGame()
    {
        Card.OnCardFlipped -= HandleOnCardFlipped;
    }
}
