using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoyController : MonoBehaviour
{
    [Header("Movement Variables")]
    [SerializeField] private CharacterController _controller;
    [SerializeField] private Vector3 _moveDir = Vector3.zero;
    [SerializeField] private float _walkSpeed = 4f;
    [SerializeField] private float _runSpeed = 6f;
    [SerializeField] private float _turnSmoothTime = 0.1f;
    [SerializeField] private Transform _cam;
    private float _turnSmoothVelocity;
    [SerializeField] private Vector3 _playerVelocity;
    [SerializeField] private float _gravityValue = -9.81f;
    [SerializeField] private bool _isMoving = false;
    [SerializeField] private bool _isRunning = false;
    [SerializeField] private float _currentSpeed = 0;
    [SerializeField] private float _walkAccel = 1f;
    [SerializeField] private float _runAccel = 1.3f;
    [SerializeField] private float _stopAccel = 2f;
    [SerializeField] private float _compareHorizontal = 0;
    [SerializeField] private float _compareVertical = 0;
    private Vector3 _compareDirection = Vector3.zero;
    private float _runTimer = 0;
    [SerializeField] private bool _changingDirection = false;
    private bool _justJumped = false;
    [SerializeField] private bool _stopped = false;
    [SerializeField] private bool _canChangeDirection = false;

    [Header ("Jump Variables")]
    [SerializeField] private Transform _groundCheckPos;
    [SerializeField] private LayerMask _groundMask;
    [SerializeField] private float _groundedDistance;
    [SerializeField] private float _jumpHeightNeutral;
    [SerializeField] private float _jumpHeightRun;
    [SerializeField] private bool _isGrounded = false;
    [SerializeField] private float _jumpForward = 10;
    [SerializeField] private float _currentJumpForward = 0;
    [SerializeField] private float _jumpForwardDecel = 5;
    [SerializeField] private bool _isJumping = false;
    private bool _canJump = true;


    // Start is called before the first frame update
    void Start()
    {
        _currentSpeed = 0f;
        _canChangeDirection = false;
    }

    // Update is called once per frame
    void Update()
    {
        GroundedUpdate();
        MoveUpdate();
        
        //This is the last function that should run each frame!
        FinalVelocityUpdate();
    }

    private void MoveUpdate()
    {

        
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;
        Vector3 magDirection = new Vector3(horizontal, 0f, vertical);

        if (_stopped)
        {
            _runTimer = 0f;
            _currentSpeed = 0;
            _canChangeDirection = false;

        }
        if (_stopped) return;

        if (_changingDirection)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + _cam.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref _turnSmoothVelocity, _turnSmoothTime/4);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);
            _runTimer = 0.5f;
            _currentSpeed = _runSpeed;
        }
        if (_changingDirection) return;
        

        if (_isGrounded)
        {
            if (magDirection.magnitude > 0.8f)
            {
                _isMoving = true;

                _runTimer += Time.deltaTime;
                if (_runTimer > 0.5f)
                {
                    _currentSpeed = _runSpeed;
                    _isRunning = true;
                    _canChangeDirection = true;
                }
                else
                {
                    _currentSpeed = _walkSpeed;
                    _isRunning = false;
                    _canChangeDirection = false;
                }

                float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + _cam.eulerAngles.y;
                float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref _turnSmoothVelocity, _turnSmoothTime);
                transform.rotation = Quaternion.Euler(0f, angle, 0f);
                //if sudden change in direction while running, we turn them around!

                float directionDifferenceHorizontal = Mathf.Abs(_compareHorizontal - horizontal);
                float directionDifferenceVertical = Mathf.Abs(_compareVertical - vertical);
                if (directionDifferenceHorizontal > 0.5f || directionDifferenceVertical > 0.5f)
                {

                    if (_canChangeDirection)
                    {
                     StartCoroutine(ChangeDirection(direction.x, direction.z));
                    }
                }
                
                _compareHorizontal = Input.GetAxisRaw("Horizontal");
                _compareVertical = Input.GetAxisRaw("Vertical");

                _compareDirection = new Vector3(_compareHorizontal, 0f, _compareVertical);
                


                _moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            }
            else if (magDirection.magnitude > 0.1f)
            {
                _isRunning = false;
                _isMoving = true;
                _canChangeDirection = false;
                _runTimer += Time.deltaTime;

                _currentSpeed = Mathf.Lerp(_currentSpeed, _walkSpeed, _walkAccel * 5 * Time.deltaTime);
                //_currentSpeed += Time.deltaTime * 5 * _walkAccel;
                _currentSpeed = Mathf.Clamp(_currentSpeed, 0f, _walkSpeed);

                float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + _cam.eulerAngles.y;
                float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref _turnSmoothVelocity, _turnSmoothTime);
                transform.rotation = Quaternion.Euler(0f, angle, 0f);

                _moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            }
            else
            {
                _currentSpeed = Mathf.Lerp(_currentSpeed, 0, _stopAccel * 5 * Time.deltaTime);
                _isRunning = false;
                _isMoving = false;
                _runTimer = 0;
                _canChangeDirection = false;
            }

        }
        else
        {
            _currentSpeed = Mathf.Lerp(_currentSpeed, 0, _stopAccel * 5 * Time.deltaTime);
            _isRunning = false;
            _isMoving = false;
            _runTimer = 0;
        }



        _controller.Move(_moveDir.normalized * _currentSpeed * Time.deltaTime);

    }

    private void GroundedUpdate()
    {
        RaycastHit hit;

        if((Physics.Raycast(_groundCheckPos.position, Vector3.down, out hit, _groundedDistance, _groundMask)))
        {
            _isGrounded = true;

            if (_isJumping) {
                if (!_justJumped)
                {
                    _isJumping = false;
                    StartCoroutine(Stop(0.3f));
                }
            }
        }
        else
        {
            _isGrounded = false;
        }
  


        if (_isGrounded && _canJump && !_isJumping)
        {
            if (Input.GetButtonDown("Jump"))
            {
                if (_isMoving)
                {
                    StartCoroutine(Jump(true));
                }
                else
                {
                    StartCoroutine(Jump(false));
                }
            }
        }

        //this is where the player falls 
        _playerVelocity.y += _gravityValue * Time.deltaTime;
        if(_playerVelocity.y <= -10f)
        {
            _playerVelocity.y = -10f;
        }

        //Where forward jump velocity trails off
        if(_currentJumpForward > 0)
        {
            if (_isGrounded && !_isJumping)
            {
                _currentJumpForward = 0;
            }
            else
            {
                _currentJumpForward -= _jumpForwardDecel * Time.deltaTime;
            }
        }

        _controller.Move(_playerVelocity * Time.deltaTime);
        _controller.Move(transform.forward * _currentJumpForward * Time.deltaTime);


    }

    private IEnumerator Jump(bool _moving)
    {
        _isJumping = true;
        _currentSpeed = 0;
        _justJumped = true;

        if (_moving)
        {
            yield return new WaitForSeconds(0.1f);
            _playerVelocity.y = 0;
            _playerVelocity.y += Mathf.Sqrt(_jumpHeightRun * -3.0f * _gravityValue);
            _currentJumpForward = _jumpForward;

        }
        else
        {
            yield return new WaitForSeconds(0.3f);
            _playerVelocity.y = 0;
            _playerVelocity.y += Mathf.Sqrt(_jumpHeightNeutral * -3.0f * _gravityValue);
        }

        yield return new WaitForSeconds(0.4f);
        _justJumped = false;
        
    }

    private IEnumerator ChangeDirection(float directionX, float directionZ)
    {
        _changingDirection = true;
        yield return new WaitForSeconds(0.1f);
        _changingDirection = false;

    }

    private IEnumerator Stop(float time)
    {
        _stopped = true;
        yield return new WaitForSeconds(time);
        _stopped = false;
    }

    private void FinalVelocityUpdate()
    {
        
    }

}
