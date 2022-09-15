using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Card : MonoBehaviour
{
    public static event Action<Card> OnCardFlipped;
    public static event Action<Card> OnCardUnflipped;

    [SerializeField] private int _id;
    [SerializeField] private Sprite _cardSprite;
    [SerializeField] private Image _cardImg;
    [SerializeField] private float _flipRate;
    [SerializeField] private bool _cardIsVisible;
    [SerializeField] private EventTrigger _eventTrigger;

    private bool _flip;
    private bool _changedSprite;

    public bool CardIsVisible => _cardIsVisible;
    public int ID => _id;
    public EventTrigger EventTrigger => _eventTrigger;

    private void Awake()
    {
        if (_eventTrigger == null)
            _eventTrigger = GetComponent<EventTrigger>();
    }

    void Start()
    {
        if (_cardImg == null)
            _cardImg = GetComponent<Image>();
    }

    void FixedUpdate()
    {
        if (!_flip)
            return;

        if(!_cardIsVisible)
        {
            FlipCard(_cardSprite, () =>
            {
                transform.rotation = Quaternion.identity;
                _flip = false;
                _cardIsVisible = true;
                OnCardFlipped?.Invoke(this);
            });
        }
        else
        {
            FlipCard(GameController.instance.CardBack, () =>
            {
                transform.rotation = Quaternion.identity;
                _flip = false;
                _cardIsVisible = false;
                OnCardUnflipped?.Invoke(this);
            });
        }
    }

    public void FlipCard(Sprite spriteToSet, Action action)
    {
        if (!_changedSprite)
            transform.Rotate(Vector2.up * _flipRate * Time.fixedDeltaTime * -1f);
        else
            transform.Rotate(Vector2.up * _flipRate * Time.fixedDeltaTime);

        if (transform.eulerAngles.y <= 270f && !_changedSprite)
        {
            _cardImg.sprite = spriteToSet;
            _changedSprite = true;
        }

        if (_changedSprite && transform.eulerAngles.y >= 350f)
            action.Invoke();
    }

    public void TriggerFlip()
    {
        _flip = true;
        _changedSprite = false;

        EventTrigger.enabled = !EventTrigger.enabled;
    }

    public void SetSprite(Sprite newSprite)
    {
        _cardImg.sprite = newSprite;
    }

    public void ResetCard()
    {
        if(_cardIsVisible)
        {
            _cardIsVisible = false;
            SetSprite(GameController.instance.CardBack);
            transform.rotation = Quaternion.identity;
        }
    }
}
