using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Schema;
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
    [SerializeField] private int _pairPoints;
    [SerializeField] private int _matchScore;
    [SerializeField] private int _secondsScore;
    [SerializeField] private int _errorPenalty;
    [Tooltip("The duration of the match in seconds.")]
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

    public int PairPoints
    {
        get => _pairPoints;
        set
        {
            _pairPoints = value;            
            _UIController.SetScore(_pairPoints);
            _pairCounter++;
        }
    }

    public int NumberOfTries
    {
        get => _numOfTries;
        set
        {
            _numOfTries = value;
            _UIController.SetNumOfTries(_numOfTries);
        }
    }

    public int TotalSecondsScore => _time * _secondsScore;
    public int TotalErrorsScore => _errorCounter * _errorPenalty;
    public int TotalScore => TotalErrorsScore + TotalSecondsScore + _pairPoints;
    public int TimeCounter => _time;

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
        _time = _matchDuration;
    }

    private void Update()
    {
        if(_matchBegun && _timeCounter >= 0f)
        {
            _timeCounter -= Time.deltaTime;
            int roundedTime = Mathf.RoundToInt(_timeCounter);

            if(roundedTime != _time) // This will serve to call the event only for each second, not for each frame
            {
                _time = roundedTime;
                OnTimeChanged?.Invoke(_time);
            }

            if(roundedTime == 0)
            {
                print("Time's up!");
                StartCoroutine(EndGame());
            }
        }
    }

    public void SetCardsSprites()
    {
        int pairCounter = 0;
        for (int i = 0; i < _cards.Count; i++)
        {
            if (i > 1 && i % 2 == 0)
                pairCounter++;

            if (pairCounter < _cardsSprites.Count)
                _cards[i].SetCardImages(_cardBack, _cardsSprites[pairCounter]);
            else
            {
                Debug.LogWarning("The number of card sprites doesn't match the number of cards, " +
                    "this may cause some cards be without any sprite.");
                break;
            }
        }
    }

    public void StartGame()
    {
        ShuffleCards();
        StartCoroutine(FlipAllCardsIntro());       
    }

    private void ForceFlipAllCards(bool setCanFlip = false)
    {
        foreach (var card in _cards)
        {
            card.ForceFlip();
            card.SetCanFlip(setCanFlip);
        }
    }

    private void FlipAllCards()
    {
        foreach (var card in _cards)
        {
            card.ActivateFlip();
        }
    }

    private IEnumerator FlipAllCardsIntro()
    {
        //Flipping...
        ForceFlipAllCards(false);

        _UIController.GameHUD.SetActive(false);

        // Waiting the player memorize the positions...
        yield return new WaitForSeconds(_memorizeTime);

        _UIController.GameHUD.SetActive(true);

        // ...and unflipping
        ForceFlipAllCards(true);

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
        if (card.CardState == CardState.Back || _flippedCards.Count > 2 || _timeCounter <= 0f)
            return;

        _flippedCards.Add(card);        

        if(_flippedCards.Count == 2)
        {
            NumberOfTries++;
            bool areEqual = _flippedCards[0].CardImage.sprite.Equals(_flippedCards[1].CardImage.sprite);

            if(!areEqual)
            {
                _errorCounter++;
                StartCoroutine(UnflipPairOfFlippedCards(_timeToUnflip));
            }
            else
            {
                PairPoints += _matchScore;
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

    public IEnumerator EndGame()
    {
        FlipAllCards();
        _matchBegun = false;
        _UIController.SaveRankingData(_UIController.NameInput.text, TotalScore);
        _UIController.GameHUD.SetActive(false);

        yield return new WaitForSeconds(_timeToEndGame);

        _UIController.InGameUI.SetActive(false);
        _UIController.EndGameUI.SetActive(true);
        _UIController.UpdateEndGamePontuation(PairPoints, TotalSecondsScore, TotalErrorsScore, TotalScore);
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

    private void OnDestroy()
    {
        Card.OnCardFlipped -= HandleOnCardFlipped;
    }
}
