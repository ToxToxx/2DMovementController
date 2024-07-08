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
        private WallJumpController _wallJumpController;
        private DashController _dashController;
        private CollisionChecksController _collisionChecksController;
        private TimerController _timerController;

        [Header ("Variables")]
        // movement variables
        public float HorizontalVelocity;
        public bool _isFacingRight = true;

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
            _jumpHandler = new JumpHandler(this);
            _landFallController = new LandFallController(this);
            _wallSlideController = new WallSlideController(this);
            _wallJumpController = new WallJumpController(this);
            _dashController = new DashController(this);
            _collisionChecksController = new CollisionChecksController(this);
            _timerController = new TimerController(this, _wallJumpController);

            _isFacingRight = true;
        }

        private void Update()
        {
            _timerController.CountTimers();
            _jumpHandler.JumpChecks();
            _landFallController.LandCheck();
            _wallSlideController.WallSlideCheck();
            _wallJumpController.WallJumpCheck();
            _dashController.DashCheck();
        }

        private void FixedUpdate()
        {
            _collisionChecksController.CollisionChecks();
            _jumpHandler.Jump();
            _landFallController.Fall();
            _wallSlideController.WallSlide();
            _wallJumpController.WallJump();
            _dashController.Dash();

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

        public void ResetDashValues()
        {
            _isDashFastFalling = false;
            _dashOnGroundTimer = -0.01f;
        }

        public void ResetDashes()
        {
            _numberOfDashesUsed = 0;
        }

        public void ResetWallJumpValues()
        {
            _isWallSlideFalling = false;
            _useWallJumpMoveStats = false;
            _isWallJumping = false;
            _isWallJumpFastFalling = false;
            _isWallJumpFalling = false;
            _isPastWallJumpApexThreshold = false;

            _wallJumpFastFallReleaseSpeed = 0f;
            _wallJumpTime = 0f;
        }

        public void ResetJumpValues()
        {
            _isJumping = false;
            _isFalling = false;
            _isFastFalling = false;
            _fastFallTime = 0f;
            _isPastApexThreshold = false;
        }

        public void StopWallSlide()
        {
            if (_isWallSliding)
            {
                _numberOfJumpsUsed++;

                _isWallSliding = false;
            }
        }
    }
}


