using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;

public class GameScreen : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private GameObject _windowBorder;
    
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        MouseControl.instance.Disable();
        transform.GetComponent<RectTransform>().SetAsLastSibling();
        DisableBorder();
    }

    public void DisableBorder()
    {
        MouseControl.instance._currentGame = this;
        _windowBorder.SetActive(false);
    }

    public void EnableBorder()
    {
        _windowBorder.SetActive(true);
    }
}
