using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;

public class Drag : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IEndDragHandler, IDragHandler
{

    [SerializeField] private Canvas _canvas;
    [SerializeField] private RectTransform _rectTransform;

    [SerializeField] private RawImage _screenImage;
    private Color _screenColor;
    private Vector3 _colors;

    private void Awake()
    {
        _rectTransform = transform.parent.GetComponent<RectTransform>();
        _canvas = transform.root.GetComponent<Canvas>();
        _screenImage = transform.parent.GetComponent<RawImage>();

        _screenColor.r = _screenImage.color.r;
        _screenColor.g = _screenImage.color.g;
        _screenColor.b = _screenImage.color.b;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _screenColor.a = 0.6f;
        _screenImage.color = _screenColor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _screenColor.a = 1f;
        
        _screenImage.color = _screenColor;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _rectTransform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        _rectTransform.anchoredPosition += eventData.delta / _canvas.scaleFactor;
    }
}

