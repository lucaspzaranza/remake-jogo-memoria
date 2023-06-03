using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    [Header("Controllers")]
    [SerializeField] private UIController _UIController;

    [Space]
    [Header("Card Customization")]
    [SerializeField] private Sprite _cardBack;
    [SerializeField] private List<Sprite> _cardsSprites;

    [Space]
    [Header("In Game Data")]
    [SerializeField] private List<Card> _cards;
    [SerializeField] private int _score;
    [SerializeField] private float _memorizeTime;

    private void Start()
    {
        SetCardsSprites();
    }

    public void SetCardsSprites()
    {
        int pairCounter = 0;
        for (int i = 0; i < _cards.Count; i++)
        {
            print($"i: {i}, {i} % 2 = {i % 2}.");
            _cards[i].SetCardImages(_cardBack, _cardsSprites[pairCounter]);

            if(i > 0 && i % 2 == 0)
                pairCounter++;
        }
    }

    public void CallFlipAllCards()
    {
        StartCoroutine(FlipAllCards());
    }

    private IEnumerator FlipAllCards()
    {
        //Flipping

        foreach (var card in _cards)
        {
            card.ActivateFlip();
        }

        // Waiting
        yield return new WaitForSeconds(_memorizeTime);

        // And unflipping
        foreach (var card in _cards)
        {
            card.ActivateFlip();
        }
    }
}
