using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Apple.ReplayKit;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    public PlayerMovementStats MovementStats;
    [SerializeField] private Collider2D _feetCollider;
    [SerializeField] private Collider2D _bodyCollider;

    private Rigidbody2D _playerRigidbody;

    // movement variables
    private Vector2 _moveVelocity;
    private bool _isFacingRight;

    //collision check variables
    private RaycastHit2D _groundHit;
    private RaycastHit2D _headHit;
    private bool _isGrounded;
    private bool _bumpedHead;

    //jump variables
    public float VerticalVelocity { get; private set; }
    private bool _isJumping;
    private bool _isFastFalling;
    private bool _isFalling;
    private float _fastFallTime;
    private float _fastFallReleaseSpeed;
    private int _numberOfJumpsUsed;

    //apex variables
    private float _apexPoint;
    private float _timePastApexThreshold;
    private bool _isPastApexThreshold;

    //jump buffer vars
    private float _jumpBufferTimer;
    private bool _jumpReleasedDuringBuffer;

    //coyote time vars
    private float _coyoteTimer;

    private void Awake()
    {
        _isFacingRight = true;

        _playerRigidbody = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        CountTimers();
        JumpChecks();
    }

    private void FixedUpdate()
    {
        CollisionChecks();
        Jump();

        if (_isGrounded)
        {
            Move(MovementStats.GroundAcceleration, MovementStats.GroundDeceleration, InputManager.Movement);

        } else
        {
            Move(MovementStats.AirAcceleration, MovementStats.AirDeceleration, InputManager.Movement);
        }
    }

    #region Movement

    private void Move(float acceleration, float deceleration, Vector2 moveInput)
    {
        if (moveInput != Vector2.zero)
        {
            TurnCheck(moveInput);

            Vector2 targetVelocity = Vector2.zero;
            if (InputManager.RunIsHeld)
            {
                targetVelocity = new Vector2(moveInput.x, 0f) * MovementStats.MaxRunSpeed;
            }
            else
            { targetVelocity = new Vector2(moveInput.x, 0f) * MovementStats.MaxWalkSpeed; }

            _moveVelocity = Vector2.Lerp(_moveVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);
            _playerRigidbody.velocity = new Vector2(_moveVelocity.x, _playerRigidbody.velocity.y);
        }
        else if (moveInput == Vector2.zero)
        {
            _moveVelocity = Vector2.Lerp(_moveVelocity, Vector2.zero, deceleration * Time.fixedDeltaTime);
            _playerRigidbody.velocity = new Vector2(_moveVelocity.x, _playerRigidbody.velocity.y);
        }
    }

    private void TurnCheck(Vector2 moveInput)
    {
        if (_isFacingRight && moveInput.x < 0)
        {
            Turn(false);
        }
        else if (!_isFacingRight && moveInput.x > 0)
        {
            Turn(true);
        }
    }

    private void Turn(bool turnRight)
    {
        if (turnRight)
        {
            _isFacingRight = true;
            transform.Rotate(0f, 180f, 0f);
        }
        else
        {
            _isFacingRight = false;
            transform.Rotate(0f, -180f, 0f);
        }
    }
    #endregion

    #region Jump
    private void JumpChecks()
    {
        if (InputManager.JumpWasPressed)
        {
            _jumpBufferTimer = MovementStats.JumpBufferTime;
            _jumpReleasedDuringBuffer = false;
        }

        if(InputManager.JumpWasReleased)
        {
            if(_jumpBufferTimer > 0)
            {
                _jumpReleasedDuringBuffer = true;
            }

            if(_isJumping && VerticalVelocity > 0f)
            {
                if(_isPastApexThreshold)
                {
                    _isPastApexThreshold = false;
                    _isFastFalling = true;
                    _fastFallTime = MovementStats.TimeForUpwardsCancel;
                    VerticalVelocity = 0f;
                }
                else
                {
                    _isFastFalling = true;
                    _fastFallReleaseSpeed = VerticalVelocity;
                }
            }
        }

        //Initiating Jump
        if (_jumpBufferTimer > 0f && !_isJumping && (_isGrounded || _coyoteTimer > 0f))
        {
            InitiateJump(1);

            if (_jumpReleasedDuringBuffer)
            {
                _isFastFalling = true;
                _fastFallReleaseSpeed = VerticalVelocity;
            }
        }

        //Double jump logic
        else if (_jumpBufferTimer > 0f && _isJumping && _numberOfJumpsUsed < MovementStats.NumberOfJumpsAllowed)
        {
            _isFastFalling = false;
            InitiateJump(1);
        }

        //air jump after coyote time lapsed logic
        else if (_jumpBufferTimer > 0f && _isFalling && _numberOfJumpsUsed < MovementStats.NumberOfJumpsAllowed)
        {
            InitiateJump(2); //because we are falling and it's air jump
            _isFastFalling = false;
        }

        //landed logic
        if((_isJumping || _isFalling) && _isGrounded && VerticalVelocity <= 0f)
        {
            _isJumping = false;
            _isFalling = false;
            _isFastFalling = false;
            _isPastApexThreshold = false;
            _fastFallTime = 0f;         
            _numberOfJumpsUsed = 0;
            VerticalVelocity = Physics2D.gravity.y;
        }
    }

    private void InitiateJump(int numberOfJumpsUsed)
    {
        if (!_isJumping)
        {
            _isJumping = true;
        }

        _jumpBufferTimer = 0f;
        _numberOfJumpsUsed += numberOfJumpsUsed;
        VerticalVelocity = MovementStats.InitialJumpVelocity;
    }
    private void Jump()
    {

    }
    #endregion

    #region Collision Checks

    private void IsGrounded()
    {
        Vector2 boxCastOrigin = new Vector2(_feetCollider.bounds.center.x, _feetCollider.bounds.min.y);
        Vector2 boxCastSize = new Vector2(_feetCollider.bounds.size.x, MovementStats.GroundDetectionRayLength);

        _groundHit = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, Vector2.down, MovementStats.GroundDetectionRayLength, MovementStats.GroundLayer);
        if (_groundHit.collider != null)
        {
            _isGrounded = true;
        }
        else _isGrounded = false;

    }

    private void CollisionChecks()
    {
        IsGrounded();
    }

    #endregion

    #region Timers

    private void CountTimers()
    {
        _jumpBufferTimer -= Time.deltaTime;

        if (!_isGrounded)
        {
            _coyoteTimer -= Time.deltaTime;
        }
        else _coyoteTimer = MovementStats.JumpCoyoteTime;

    }

    #endregion
}
