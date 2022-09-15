using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using System.Linq;
using System;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GameController : MonoBehaviour
{
    public static GameController instance;

    public static event Action OnGameEnded;

    [SerializeField] private int _pairCount;
    [SerializeField] private int _timeInSeconds;
    [SerializeField] private int _timeToShowCards;
    [SerializeField] private Sprite _cardBack;
    [SerializeField] private GameObject _elapsedTime;
    [SerializeField] private List<Card> _cards;
    [SerializeField] private List<Card> _flippedCards;
    [SerializeField] private UIElements _UIElements;

    private bool _isInGame;
    private int _errorCounter;
    private int _elapsedTimeVal;

    public IReadOnlyList<Card> Cards => _cards;
    public Sprite CardBack => _cardBack;
    public IReadOnlyList<Card> FlippedCards => _flippedCards;
    public int PairCount => _pairCount;
    public int TotalPairs => Cards.Count / 2;
    public int TimeInSeconds => _timeInSeconds;
    public bool IsInGame => _isInGame;
    public UIElements UIElements => _UIElements;
    public int ErrorCounter => _errorCounter;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        Card.OnCardFlipped += HandleOnCardFlipped;
        Card.OnCardUnflipped += HandleOnCardUnflipped;
        ElapsedTime.OnTimeUpdated += HandleOnTimeUpdated;
        ElapsedTime.OnTimeEnded += HandleOnTimeEnded;
    }

    public void CallStartGame()
    {
        StartCoroutine(nameof(StartGame));
    }

    private IEnumerator StartGame()
    {
        //ShuffleCards();
        _isInGame = false; 
        _pairCount = 0;
        FlipAllCards(card => !card.CardIsVisible);
        yield return new WaitForSeconds(_timeToShowCards);
        FlipAllCards(card => card.CardIsVisible);
        _isInGame = true;
        _elapsedTime.SetActive(true);
        _UIElements.ResetButton.interactable = true;
    }

    private void HandleOnTimeUpdated(int elapsedTime)
    {
        _elapsedTimeVal = elapsedTime;
    }

    private void HandleOnTimeEnded()
    {
        _UIElements.ResetButton.interactable = false;
        _isInGame = false;
        StartCoroutine(FlipAllCardsCoroutine(1f, card => !card.CardIsVisible));
        StartCoroutine(FlipAllCardsCoroutine(3f, card => card.CardIsVisible));
        UIElements.InGameScreenActivation(false);
        UIElements.EndGameScreenActivation(true);
        CalculateTotalScore();
    }

    private void CalculateTotalScore()
    {
        UIElements.Pairs.text = $"Pares (x100) = {_pairCount * 100}";
        UIElements.Time.text = $"Tempo (x10) = {_elapsedTimeVal * 10}";
        UIElements.Errors.text = $"Erros (-x10) = {_errorCounter * 10}";

        int total = (_pairCount * 100) + (_elapsedTimeVal * 10) - (_errorCounter * 10);

        UIElements.TotalScore.text = $"{total} pontos";
    }

    private void FlipAllCards(Func<Card, bool> predicate)
    {
        _cards.Where(predicate).ToList()
        .ForEach(card =>
        {
            card.TriggerFlip();
        });
    }

    private IEnumerator FlipAllCardsCoroutine(float timeToWait, Func<Card, bool> predicate)
    {
        yield return new WaitForSeconds(timeToWait);

        _cards.Where(predicate).ToList()
        .ForEach(card =>
        {
            card.TriggerFlip();
        });
    }

    private void ResetAllCards()
    {
        _cards
        .ForEach(card =>
        {
            card.ResetCard();
        });
    }

    private void HandleOnCardFlipped(Card flippedCard)
    {
        if (!IsInGame)
            return;

        _flippedCards.Add(flippedCard);

        if (_flippedCards.Count == 2)
            CheckFlippedCards();
    }

    private void HandleOnCardUnflipped(Card unflippedCard)
    {
        _flippedCards.Remove(unflippedCard);
    }

    private void CheckFlippedCards()
    {
        bool cardsAreEqual = _flippedCards[0].ID == _flippedCards[1].ID;

        if(cardsAreEqual)
            MakePair();
        else
            Invoke(nameof(UnflipWrongCards), 0.5f);
    }

    private void UnflipWrongCards()
    {
        _flippedCards[0].TriggerFlip();
        _flippedCards[1].TriggerFlip();
        _errorCounter++;
    }

    private void MakePair()
    {
        _flippedCards = new List<Card>();
        _pairCount++;

        if(_pairCount == TotalPairs)
        {
            print("End game");
            _isInGame = false;
            CalculateTotalScore();
            Invoke(nameof(CallEndGame), 2f);
        }
    }

    private void CallEndGame()
    {
        UIElements.InGameScreenActivation(false);
        UIElements.EndGameScreenActivation(true);
    }

    public void ResetGame()
    {
        ResetAllCards();

        var elapsedComponent = _elapsedTime.GetComponent<ElapsedTime>();
        elapsedComponent.Setup();
        _elapsedTime.SetActive(false);
        _elapsedTimeVal = 0;
        _errorCounter = 0;
        _pairCount = 0;

        StartCoroutine(StartGame());
    }

    public void ShuffleCards()
    {
        for (int i = 0; i < _cards.Count; i++)
        {
            int randomIndex = UnityEngine.Random.Range(0, _cards.Count);
            _cards[i].transform.SetSiblingIndex(randomIndex);
        }
    }

    public void PlayAgain()
    {
        SceneManager.LoadScene(0);
    }
}
