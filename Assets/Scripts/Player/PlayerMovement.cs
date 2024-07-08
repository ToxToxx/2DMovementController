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
    public float HorizontalVelocity {get; private set;}
    private bool _isFacingRight;

    //collision check variables
    private RaycastHit2D _groundHit;
    private RaycastHit2D _headHit;
    private bool _isGrounded;
    private bool _bumpedHead;

    //wall collision check variables
    private RaycastHit2D _wallHit;
    private RaycastHit2D _lastWallHit;
    private bool _isTouchingWall;

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

    //wall slide
    private bool _isWallSliding;
    private bool _isWallSlideFalling;

    //wall jump
    private bool _useWallJumpMoveStats;
    private bool _isWallJumping;
    private float _wallJumpTime;
    private bool _isWallJumpFastFalling;
    private bool _isWallJUmpFalling;
    private float _wallJUmpFastFallTime;
    private float _wallJumpFastFallReleaseSpeed;

    private float _wallJumpPostBufferTimer;

    private float _wallJumpApexPoint;
    private float _timePastWallJumpApexThreshold;
    private bool _isPastWallJumpApexThreshold;

    //dash vars
    private bool _isDashing;
    private bool _isAirDashing;
    private float _dashTimer;
    private float _dashOnGroundTimer;
    private int _numberOfDashesUsed;
    private Vector2 _dashDirection;
    private bool _isDashFastFalling;
    private float _dashFastFallTime;
    private float _dashFastFallReleaseSpeed;

    private void Awake()
    {
        _isFacingRight = true;

        _playerRigidbody = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        CountTimers();
        JumpChecks();
        LandCheck();
        WallSlideCheck();
        WallJumpCheck();
    }

    private void FixedUpdate()
    {
        CollisionChecks();
        Jump();
        Fall();
        WallSlide();
        WallJump();

        if (_isGrounded)
        {
            Move(MovementStats.GroundAcceleration, MovementStats.GroundDeceleration, InputManager.Movement);

        }
        else
        {
            //wall jumping
            if (_useWallJumpMoveStats)
            {
                Move(MovementStats.WallJumpMoveAceleration, MovementStats.WallJumpMoveDeceleration, InputManager.Movement);
            }

            //airborne
            else
            {
                Move(MovementStats.AirAcceleration, MovementStats.AirDeceleration, InputManager.Movement);
            }

        }

        ApplyVelocity();
    }

    private void ApplyVelocity()
    {
        //clamp fall speed
        if (!_isDashing)
        {
            VerticalVelocity = Mathf.Clamp(VerticalVelocity, -MovementStats.MaxFallSpeed, 50f); //changed if need to clamp faster
        }
        else
        {
            VerticalVelocity = Mathf.Clamp(VerticalVelocity, -50f, 50f);
        }

        _playerRigidbody.velocity = new Vector2(HorizontalVelocity, VerticalVelocity);
    }

    #region Movement

    private void Move(float acceleration, float deceleration, Vector2 moveInput)
    {
        if (!_isDashing)
        {
            if (Mathf.Abs(moveInput.x) >= MovementStats.MoveTreshold)
            {
                TurnCheck(moveInput);

                float targetVelocity = 0f;
                if (InputManager.RunIsHeld)
                {
                    targetVelocity = moveInput.x * MovementStats.MaxRunSpeed;
                }
                else
                { targetVelocity = moveInput.x * MovementStats.MaxWalkSpeed; }

                HorizontalVelocity = Mathf.Lerp(HorizontalVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);
            }
            else if (Mathf.Abs(moveInput.x) <= MovementStats.MoveTreshold)
            {
                HorizontalVelocity = Mathf.Lerp(HorizontalVelocity, 0f, deceleration * Time.fixedDeltaTime);
            }
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

    #region Land/Fall

    private void LandCheck()
    {
        //landed logic
        if ((_isJumping || _isFalling || _isWallJumping || _isWallJUmpFalling || _isWallSlideFalling || _isWallSliding || _isDashFastFalling) && _isGrounded && VerticalVelocity <= 0f)
        {
            ResetJumpValues();
            StopWallSlide();
            ResetWallJumpValues();
            ResetDashes();
            _numberOfJumpsUsed = 0;
            VerticalVelocity = Physics2D.gravity.y;

            if(_isDashFastFalling && _isGrounded)
            {
                ResetDashValues();
                return;
            }

            ResetDashValues();
        }
    }

    private void Fall()
    {
        //normal gravity while falling
        if (!_isGrounded && !_isJumping && !_isWallSliding && !_isWallJumping && !_isDashing && !_isDashFastFalling)
        {
            if (!_isFalling)
            {
                _isFalling = true;
            }
            VerticalVelocity += MovementStats.Gravity * Time.fixedDeltaTime;
        }
    }

    #endregion

    #region Jump

    private void ResetJumpValues()
    {
        _isJumping = false;
        _isFalling = false;
        _isFastFalling = false;
        _fastFallTime = 0f;
        _isPastApexThreshold = false;
    }
    private void JumpChecks()
    {
        if (InputManager.JumpWasPressed)
        {
            if(_isWallSlideFalling && _wallJumpPostBufferTimer >= 0f)
            {
                return;
            }

            else if (_isWallSliding || (_isTouchingWall && _isGrounded)) { return; }

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
        else if (_jumpBufferTimer > 0f && (_isJumping || _isWallJumping || _isWallSlideFalling || _isAirDashing || _isDashFastFalling) && !_isTouchingWall && _numberOfJumpsUsed < MovementStats.NumberOfJumpsAllowed)
        {
            _isFastFalling = false;
            InitiateJump(1);

            if (_isDashFastFalling)
            {
                _isDashFastFalling = false;
            }
        }

        //air jump after coyote time lapsed logic
        else if (_jumpBufferTimer > 0f && _isFalling && !_isWallSlideFalling && _numberOfJumpsUsed < MovementStats.NumberOfJumpsAllowed - 1)//fixed bugs of uneccesary double jump in air
        {
            InitiateJump(2); //because we are falling and it's air jump
            _isFastFalling = false;
        }

    }

    private void InitiateJump(int numberOfJumpsUsed)
    {
        if (!_isJumping)
        {
            _isJumping = true;
        }

        ResetWallJumpValues();

        _jumpBufferTimer = 0f;
        _numberOfJumpsUsed += numberOfJumpsUsed;
        VerticalVelocity = MovementStats.InitialJumpVelocity;
    }
    private void Jump()
    {
        //apply gravity
        if (_isJumping)
        {   
            //check head bump
            if(_bumpedHead)
            {
                _isFastFalling = true;

            }

            //gravity of ascending
            if(VerticalVelocity >= 0f)
            {
                //apex controls
                _apexPoint = Mathf.InverseLerp(MovementStats.InitialJumpVelocity, 0f, VerticalVelocity);

                if(_apexPoint > MovementStats.ApexThreshhold)
                {
                    if (!_isPastApexThreshold)
                    {
                        _isPastApexThreshold = true;
                        _timePastApexThreshold = 0f;
                    }
                    if (_isPastApexThreshold)
                    {
                        _timePastApexThreshold += Time.deltaTime;
                        if(_timePastApexThreshold < MovementStats.ApexHangTime)
                        {
                            VerticalVelocity = 0f;
                        }
                        else
                        {
                            VerticalVelocity = -0.01f;
                        }
                    }
                }

                //gravity of ascending not past apex threshold
                else if (!_isFastFalling)
                {
                    VerticalVelocity += MovementStats.Gravity * Time.fixedDeltaTime;
                    if (_isPastApexThreshold)
                    {
                        _isPastApexThreshold = false;
                    }
                }
            }

            //gravity of descending
            else if (!_isFastFalling)
            {
                VerticalVelocity += MovementStats.Gravity * MovementStats.GravityReleaseMultiplier * Time.fixedDeltaTime;
            }
            else if(VerticalVelocity< 0f)
            {
                if(!_isFalling)
                {
                    _isFalling = true;
                }
            }
        }

        //jump cut
        if (_isFastFalling)
        {
            if(_fastFallTime >= MovementStats.TimeForUpwardsCancel)
            {
                VerticalVelocity += MovementStats.Gravity * MovementStats.GravityReleaseMultiplier * Time.fixedDeltaTime;
            } 
            else if (_fastFallTime < MovementStats.TimeForUpwardsCancel)
            {
                VerticalVelocity = Mathf.Lerp(_fastFallReleaseSpeed, 0f, (_fastFallTime / MovementStats.TimeForUpwardsCancel));
            }
            _fastFallTime += Time.fixedDeltaTime;
        }
    }
    #endregion

    #region Wall Slide

    private void WallSlideCheck()
    {
        if(_isTouchingWall && !_isGrounded && !_isDashing)
        {
            if(VerticalVelocity < 0f && !_isWallSliding)
            {
                ResetJumpValues();
                ResetWallJumpValues();
                ResetDashValues();

                if (MovementStats.ResetDashOnWallSlide)
                {
                    ResetDashes();
                }

                _isWallSlideFalling = false;
                _isWallSliding = true;

                if(MovementStats.ResetJumpOnWallSlide)
                {
                    _numberOfJumpsUsed = 0;
                }
            }
        }

        else if (_isWallSliding && !_isTouchingWall && !_isGrounded && !_isWallSlideFalling) 
        {
            _isWallSlideFalling = true;
            StopWallSlide();
        }
        else
        {
            StopWallSlide();
        }
    }

    private void StopWallSlide()
    {
        if (_isWallSliding)
        {
            _numberOfJumpsUsed++;

            _isWallSliding = false;
        }
    }

    private void WallSlide()
    {
        if(_isWallSliding)
        {
            VerticalVelocity = Mathf.Lerp(VerticalVelocity, -MovementStats.WallSlideSpeed, MovementStats.WallSlideDecelerationSpeed * Time.fixedDeltaTime);

        }
    }

    #endregion

    #region Wall Jump

    private void WallJumpCheck()
    {
        if(ShouldApplyPostWallJumpBuffer())
        {
            _wallJumpPostBufferTimer = MovementStats.WallJumpPostBufferTime;
        }

        //wall jump fast falling
    }

    private void WallJump()
    {

    }

    private bool ShouldApplyPostWallJumpBuffer()
    {
        if(!_isGrounded && (_isTouchingWall || _isWallSliding))
        {
            return true;
        } else
        {
            return false;
        }
    }
    private void ResetWallJumpValues()
    {
        _isWallSlideFalling = false;
        _useWallJumpMoveStats = false;
        _isWallJumping = false;
        _isWallJumpFastFalling = false;
        _isWallJUmpFalling = false;
        _isPastWallJumpApexThreshold = false;

        _wallJumpFastFallReleaseSpeed = 0f;
        _wallJumpTime = 0f;
    }

    #endregion


    #region Dash

    private void ResetDashValues()
    {
        _isDashFastFalling = false;
        _dashOnGroundTimer = -0.01f;
    }
    
    private void ResetDashes()
    {
        _numberOfDashesUsed = 0;
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

    private void BumpedHead()
    {
        Vector2 boxCastOrigin = new Vector2(_feetCollider.bounds.center.x, _bodyCollider.bounds.max.y);
        Vector2 boxCastSize = new Vector2(_feetCollider.bounds.size.x * MovementStats.HeadWidth, MovementStats.HeadDetectionRayLength);

        _headHit = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, Vector2.up, MovementStats.HeadDetectionRayLength, MovementStats.GroundLayer);
        if(_headHit.collider != null)
        {
            _bumpedHead = true;
        }
        else
        {
            _bumpedHead = false;
        }
    }

    private void IsTouchingWall()
    {
        float originEndPoint = 0f;
        if (_isFacingRight)
        {
            originEndPoint = _bodyCollider.bounds.max.x;
        }
        else originEndPoint = _bodyCollider.bounds.min.x;

        float adjustedHeight = _bodyCollider.bounds.size.y * MovementStats.WallDetectionRayHeightMultiplier;

        Vector2 boxCastOrigin = new Vector2(originEndPoint, _bodyCollider.bounds.center.y);
        Vector2 boxCastSize = new Vector2(MovementStats.WallDetectionRayLength, adjustedHeight);

        _wallHit = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, transform.right, MovementStats.WallDetectionRayLength, MovementStats.GroundLayer);
        if(_wallHit.collider != null)
        {
            _lastWallHit = _wallHit;
            _isTouchingWall = true;
        }
        else _isTouchingWall= false;


    }
    private void CollisionChecks()
    {
        IsGrounded();
        BumpedHead();
        IsTouchingWall();
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

        if(!ShouldApplyPostWallJumpBuffer())
        {
            _wallJumpPostBufferTimer -= Time.deltaTime;
        }
    }

    #endregion
}
