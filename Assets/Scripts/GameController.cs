using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Schema;
using Unity.VisualScripting;
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

    private bool _matchBegun;
    private bool _matchRestarted;
    private int _pairCounter;
    private int _errorCounter;
    private int _numOfTries;
    private int _time;
    private float _timeCounter;

    private List<Card> _flippedCards = new List<Card>(); 
    public int FlippedCardsCount => _flippedCards.Count;
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
        _matchRestarted = false;
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
                StartCoroutine(EndGame());
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

    public void RestartMatch()
    {
        if(!_matchRestarted)
        {
            _time = _matchDuration;
            _timeCounter = 0f;
            _matchBegun = false;

            // Reseting the cards which are flipped in game
            _cards.Where(card => card.CardState == CardState.Flipped)
                .ToList()
                .ForEach(card =>
                {
                    card.CardImage.sprite = _cardBack;
                    card.SetCardState(CardState.Back);
                });

            _pairPoints = 0;
            _pairCounter = 0;
            _numOfTries = 0;
            _errorCounter = 0;
            _flippedCards = new List<Card>();
            StartGame();
            _matchRestarted = true;
            _UIController.RestartMatchBtn.interactable = false;
            _UIController.SetNumOfTries(0);
            _UIController.SetScore(0);
        }
    }

    private void ForceFlipAllCards(bool setCanFlip = false, bool flipOnlyBackCards = false)
    {
        foreach (var card in _cards)
        {
            if(flipOnlyBackCards && card.CardState == CardState.Flipped)
                continue;

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
        if (FlippedCardsCount == 2 || TimeCounter <= 0f) //card.CardState == CardState.Back || removed
            return;

        _flippedCards.Add(card);        
        if(FlippedCardsCount == 2)
        {
            NumberOfTries++;

            bool areEqual = _flippedCards[0].FlippedSprite.Equals(_flippedCards[1].FlippedSprite);

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
        ForceFlipAllCards(false, true);
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

        if(TimeCounter > 1f)
        {
            foreach (var card in _flippedCards)
            {
                card.ForceFlip();
            }
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
