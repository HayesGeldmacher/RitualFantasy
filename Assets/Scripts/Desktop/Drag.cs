using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;

public class Drag : MonoBehaviour
{

    [SerializeField] private Canvas _canvas;
    [SerializeField] private bool _isDragging = false;
    [SerializeField] private Vector2 _offSet;

    public void OnBeginDrag(BaseEventData data)
    {
        _offSet = Vector2.zero;
        Vector2 pointerPosition;
        PointerEventData pointerData = (PointerEventData)data;
        RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)_canvas.transform, pointerData.position, _canvas.worldCamera, out pointerPosition);
        Vector2 _currentPos = transform.position;
        _offSet = pointerPosition - _currentPos;
    }
    
    public void DragHandler(BaseEventData data)
    {

 
        PointerEventData pointerData = (PointerEventData)data;

        Vector2 cursorPosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)_canvas.transform, pointerData.position, _canvas.worldCamera, out cursorPosition);

        transform.position = _canvas.transform.TransformPoint(cursorPosition + _offSet);

    }

    public void EndDrag()
    {
        _isDragging = false;
    }

}
