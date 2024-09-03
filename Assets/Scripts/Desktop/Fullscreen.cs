using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Fullscreen : MonoBehaviour, IPointerDownHandler
{

    [SerializeField] private Canvas _canvas;
    [SerializeField] private float _originalScale;
    [SerializeField] private RectTransform _rectTransform;
    [SerializeField] private MouseControl _mouseControl;
    public bool _fullscreen;
    [SerializeField] private Vector2 _fullPos;
    [SerializeField] private Image _backGround;
    private Transform _mouse;
    private Vector2 _scale;

    [SerializeField] private float _maxScale;


    private RectTransform _buttonTransform;
    private bool _growing;


    private void Awake()
    {
        _buttonTransform = GetComponent<RectTransform>();
        _fullscreen = false;
        _originalScale = _rectTransform.localScale.x;
        _backGround.enabled = false;
    }

    
    public void OnPointerDown(PointerEventData eventData)
    {
        _scale = _rectTransform.localScale;

        if (_fullscreen)
        {
            EndFullScreen();
        }
        else
        {
            EnterFullScreen();
        }

        _rectTransform.localScale = _scale;

    }


    public void EndFullScreen()
    {
        Debug.Log("ENDED");
        _scale.x = _originalScale;
        _scale.y = _originalScale;
        _fullscreen = false;
        _backGround.enabled = false;
    }
   
    public void EnterFullScreen()
    {
        _originalScale = _rectTransform.localScale.x;
        _scale.x = _maxScale;
        _scale.y = _maxScale;
        _rectTransform.anchoredPosition = _fullPos;
        _fullscreen = true;
        _rectTransform.transform.GetComponent<GameScreen>().DisableBorder();
        _mouseControl.Disable();
        _mouseControl._fullScreen = this;
        _backGround.enabled = true;
    }


}

