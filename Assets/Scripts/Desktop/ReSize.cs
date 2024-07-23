using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;

public class ReSize : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IEndDragHandler, IDragHandler
{

    [SerializeField] private Canvas _canvas;
    [SerializeField] private float _scaleX;
    [SerializeField] private float _scaleY; 
    [SerializeField] private RectTransform _rectTransform;
    [SerializeField] private MouseControl _mouseControl;
    private Transform _mouse;

    [SerializeField] private float _minScale;
    [SerializeField] private float _maxScale;

    private RectTransform _buttonTransform;
    private bool _growing;
   

    private void Awake()
    {
       // _mouseControl = MouseControl.instance;
        _buttonTransform = GetComponent<RectTransform>();

        _scaleX = _rectTransform.localScale.x;
        _scaleY = _rectTransform.localScale.y;

    }
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        _mouseControl.StartResize(transform);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _mouseControl.EndResize();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
       
    }

    public void OnDrag(PointerEventData eventData)
    {
        var scale = _rectTransform.localScale;

        float mouseMovementY = Input.GetAxis("MouseInputY");
        float mouseMovementX = Input.GetAxis("MouseInputX");


        if(mouseMovementY > 0 )
        {
            _growing = true;
        }
        else
        {
            _growing = false;
        }

        if (_growing)
        {
            if(_rectTransform.localScale.x <= _maxScale && _rectTransform.localScale.y <= _maxScale)
            {
                scale.x += 1f * Time.deltaTime;
                scale.y += 1f * Time.deltaTime;

            }
        }
        else
        {
            if (_rectTransform.localScale.x >= _minScale && _rectTransform.localScale.y >= _minScale)
            scale.x -= 1f * Time.deltaTime;
            scale.y -= 1f * Time.deltaTime;
        }

        scale.x = Mathf.Clamp(scale.x, _minScale, _maxScale);
        scale.y = Mathf.Clamp(scale.y, _minScale, _maxScale);

        _rectTransform.localScale = scale;
    }


}
