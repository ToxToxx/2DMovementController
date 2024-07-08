using PlayerMovementRunJumpSeparateClasses;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlayerMovementRefactoring
{
    public class PlayerMovement : MonoBehaviour
    {
        [Header("References")]
        public PlayerMovementStats MovementStats;
        public Collider2D _feetCollider;
        public Collider2D _bodyCollider;

        private Rigidbody2D _playerRigidbody;

        private GroundMovement _groundMovement;
        private JumpHandler _jumpHandler;
        private LandFallController _landFallController;
        private WallSlideController _wallSlideController;

        // movement variables
        public float HorizontalVelocity;
        public bool _isFacingRight;

        //collision check variables
        public RaycastHit2D _groundHit;
        public RaycastHit2D _headHit;
        public bool _isGrounded;
        public bool _bumpedHead;

        //wall collision check variables
        public RaycastHit2D _wallHit;
        public RaycastHit2D _lastWallHit;
        public bool _isTouchingWall;

        //jump variables
        public float VerticalVelocity;
        public bool _isJumping;
        public bool _isFastFalling;
        public bool _isFalling;
        public float _fastFallTime;
        public float _fastFallReleaseSpeed;
        public int _numberOfJumpsUsed;

        //apex variables
        public float _apexPoint;
        public float _timePastApexThreshold;
        public bool _isPastApexThreshold;

        //jump buffer vars
        public float _jumpBufferTimer;
        public bool _jumpReleasedDuringBuffer;

        //coyote time vars
        public float _coyoteTimer;

        //wall slide
        public bool _isWallSliding;
        public bool _isWallSlideFalling;

        //wall jump
        public bool _useWallJumpMoveStats;
        public bool _isWallJumping;
        public float _wallJumpTime;
        public bool _isWallJumpFastFalling;
        public bool _isWallJumpFalling;
        public float _wallJUmpFastFallTime;
        public float _wallJumpFastFallReleaseSpeed;

        public float _wallJumpPostBufferTimer;

        public float _wallJumpApexPoint;
        public float _timePastWallJumpApexThreshold;
        public bool _isPastWallJumpApexThreshold;

        //dash vars
        public bool _isDashing;
        public bool _isAirDashing;
        public float _dashTimer;
        public float _dashOnGroundTimer;
        public int _numberOfDashesUsed;
        public Vector2 _dashDirection;
        public bool _isDashFastFalling;
        public float _dashFastFallTime;
        public float _dashFastFallReleaseSpeed;

        private void Awake()
        {
            _playerRigidbody = GetComponent<Rigidbody2D>();

            _groundMovement = new GroundMovement(this);
            _jumpHandler = new JumpHandler(this, _playerRigidbody, MovementStats);
            _landFallController = new LandFallController(this, _playerRigidbody, MovementStats, _jumpHandler);
            _wallSlideController = new WallSlideController(this, _playerRigidbody, MovementStats, _jumpHandler);
        }

        private void Update()
        {
            CountTimers();
            _jumpHandler.JumpChecks();
            _landFallController.LandCheck();
            _wallSlideController.WallSlideCheck();
            WallJumpCheck();
            DashCheck();
        }

        private void FixedUpdate()
        {
            CollisionChecks();
            _jumpHandler.Jump();
            _landFallController.Fall();
            _wallSlideController.WallSlide();
            WallJump();
            Dash();

            if (_isGrounded)
            {
                _groundMovement.Move(MovementStats.GroundAcceleration, MovementStats.GroundDeceleration, InputManager.Movement);

            }
            else
            {
                //wall jumping
                if (_useWallJumpMoveStats)
                {
                    _groundMovement.Move(MovementStats.WallJumpMoveAceleration, MovementStats.WallJumpMoveDeceleration, InputManager.Movement);
                }

                //airborne
                else
                {
                    _groundMovement.Move(MovementStats.AirAcceleration, MovementStats.AirDeceleration, InputManager.Movement);
                }

            }

            ApplyVelocity();
        }


        public void TurnCheck(Vector2 moveInput)
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

        public void ApplyVelocity()
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
    }
}


