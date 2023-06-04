using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    /// <summary>
    /// Set this to be the value to flip the card back and show its back.
    /// </summary>
    private const float _valueToStopFlip = 270f;

    public static Action<Card> OnCardFlipped;

    [SerializeField] private bool _activateFlip;
    [SerializeField] private float _flipSpeed;
    [SerializeField] private float _rotationLimit;
    [SerializeField] private float _deadzone;
    [SerializeField] private Sprite _flippedSprite;
    [SerializeField] private Sprite _backSprite;
    [SerializeField] private Button _buttonComp;

    private bool _canFlip = true;
    private bool _changedSide;
    private bool _cardMatched;

    [SerializeField] private Image _cardImg;
    public Image CardImage => _cardImg;

    [SerializeField] private CardState _cardState;
    public CardState CardState => _cardState;

    private void OnEnable()
    {
        if(_cardImg == null)
            _cardImg = GetComponent<Image>();

        if (_buttonComp == null)
            _buttonComp = GetComponent<Button>();

        _buttonComp.onClick.AddListener(() =>
        {
            if (!_canFlip)
                return;

            if(!_activateFlip && _cardState == CardState.Back)
                _activateFlip = true;
        });

        _cardState = CardState.Back;
        _changedSide = false;
    }

    private void Update()
    {
        if(_activateFlip)
            FlipCard();
    }

    public void ActivateFlip()
    {
        _buttonComp.onClick.Invoke();
    }

    public void ForceFlip()
    {
        _activateFlip = true;
    }

    private void FlipCard()
    {
        if (!_changedSide)
            transform.Rotate(Vector3.up, _flipSpeed * Time.deltaTime);
        else
            transform.Rotate(Vector3.up, -_flipSpeed * Time.deltaTime);

        float difference = Mathf.Abs(transform.localRotation.eulerAngles.y - _rotationLimit);

        if (!_changedSide && difference <= _deadzone)
            ChangeCardSideAndImage();

        if (_changedSide && transform.eulerAngles.y > _valueToStopFlip)
        {
            //print($"The card {_cardState} has stopped its flip on {transform.eulerAngles.y}");
            _changedSide = false;
            _activateFlip = false;
            transform.rotation = Quaternion.identity;
            OnCardFlipped?.Invoke(this);
        }
    }

    private void ChangeCardSideAndImage()
    {
        if(_cardState == CardState.Back)
        {
            _cardImg.sprite = _flippedSprite;
            _cardState = CardState.Flipped;
        }
        else if (_cardState == CardState.Flipped)
        {
            _cardImg.sprite = _backSprite;
            _cardState = CardState.Back;
        }

        _changedSide = true;
    }

    public void SetCardImages(Sprite back, Sprite flipped)
    {
        _backSprite = back;
        _flippedSprite = flipped;
    }

    public void SetCanFlip(bool val)
    {
        _canFlip = val;
    }
}
