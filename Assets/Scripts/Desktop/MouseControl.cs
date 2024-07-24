using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;


public class MouseControl : MonoBehaviour
{
   public bool _canMove = true;
   private bool _active = true;
   private bool _resizing = false;
    [SerializeField] private Image _sprite;
    public Transform _followPos;

    //The below region just creates a reference of this specific controller that we can call from other scripts quickly
    #region Singleton

    public static MouseControl instance;

    void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("More than one instance of mouseController present!! NOT GOOD!");
            return;
        }

        instance = this;
    }

    #endregion

    // Start is called before the first frame update
    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined; // keep confined in the game window
        _canMove = true;
        _active = true;
    }

    // Update is called once per frame
    void Update()
    {

        if (!_active)
        {
            if (Input.GetKeyDown(KeyCode.CapsLock))
            {
                Enable();
            }
        }
        if(!_active) return;


        if (_canMove && _active)
        {
          transform.position = Input.mousePosition;
        }
        else if( _resizing == true)
        {
            transform.position = _followPos.position;
        }

       

    }

    public void StartResize(Transform followpPoint)
    {

        _followPos = followpPoint;
        _canMove = false;
        _resizing = true;

    }

    public void EndResize()
    {

        Vector2 currentPos = transform.position;
        Mouse.current.WarpCursorPosition(currentPos);
        _canMove = true;

        _resizing = false;
        
    }

    public void Disable()
    {
         Cursor.lockState = CursorLockMode.Locked;
        _sprite.enabled = false;
        _active = false;

    }

    public void Enable()
    {
        Cursor.lockState = CursorLockMode.Confined;
        _active = true;
        _sprite.enabled = true;
    }
}
