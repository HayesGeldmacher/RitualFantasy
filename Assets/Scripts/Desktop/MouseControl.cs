using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MouseControl : MonoBehaviour
{
   public bool _canMove = true;
   private bool _active = true;
    [SerializeField] private Image _sprite;

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

        if (_canMove && _active)
        {
          transform.position = Input.mousePosition;
        }

        if (!_active)
        {
            if (Input.GetKeyDown(KeyCode.CapsLock))
            {
                Enable();
            }
        }

    }

    public void EndReSize()
    {
        _canMove = true;
        
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
