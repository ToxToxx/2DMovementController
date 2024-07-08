using UnityEngine;

namespace PlayerMovementRefactoring
{
    public class PlayerMovement : MonoBehaviour
    {
        [Header("References")]
        public PlayerMovementStats MovementStats;
        public Collider2D FeetCollider;
        public Collider2D BodyCollider;

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
        public bool IsFacingRight = true;

        //collision check variables
        public RaycastHit2D GroundHit;
        public RaycastHit2D HeadHit;
        public bool IsGrounded;
        public bool BumpedHead;

        //wall collision check variables
        public RaycastHit2D WallHit;
        public RaycastHit2D LastWallHit;
        public bool IsTouchingWall;

        //jump variables
        public float VerticalVelocity;
        public bool IsJumping;
        public bool IsFastFalling;
        public bool IsFalling;
        public float FastFallTime;
        public float FastFallReleaseSpeed;
        public int NumberOfJumpsUsed;

        //apex variables
        public float ApexPoint;
        public float TimePastApexThreshold;
        public bool IsPastApexThreshold;

        //jump buffer vars
        public float JumpBufferTimer;
        public bool JumpReleasedDuringBufferTimer;

        //coyote time vars
        public float CoyoteTimer;

        //wall slide
        public bool IsWallSliding;
        public bool IsWallSlideFalling;

        //wall jump
        public bool UseWallJumpMoveStats;
        public bool IsWallJumping;
        public float WallJumpTime;
        public bool IsWallJumpFastFalling;
        public bool IsWallJumpFalling;
        public float WallJUmpFastFallTime;
        public float WallJumpFastFallReleaseSpeed;

        public float WallJumpPostBufferTimer;

        public float WallJumpApexPoint;
        public float TimePastWallJumpApexThreshold;
        public bool IsPastWallJumpApexThreshold;

        //dash vars
        public bool IsDashing;
        public bool IsAirDashing;
        public float DashTimer;
        public float DashOnGroundTimer;
        public int NumberOfDashesUsed;
        public Vector2 DashDirection;
        public bool IsDashFastFalling;
        public float DashFastFallTime;
        public float DashFastFallReleaseSpeed;

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

            IsFacingRight = true;
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

            if (IsGrounded)
            {
                _groundMovement.Move(MovementStats.GroundAcceleration, MovementStats.GroundDeceleration, InputManager.Movement);

            }
            else
            {
                //wall jumping
                if (UseWallJumpMoveStats)
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
            if (IsFacingRight && moveInput.x < 0)
            {
                Turn(false);
            }
            else if (!IsFacingRight && moveInput.x > 0)
            {
                Turn(true);
            }
        }

        private void Turn(bool turnRight)
        {
            if (turnRight)
            {
                IsFacingRight = true;
                transform.Rotate(0f, 180f, 0f);
            }
            else
            {
                IsFacingRight = false;
                transform.Rotate(0f, -180f, 0f);
            }
        }

        public void ApplyVelocity()
        {
            //clamp fall speed
            if (!IsDashing)
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
            IsDashFastFalling = false;
            DashOnGroundTimer = -0.01f;
        }

        public void ResetDashes()
        {
            NumberOfDashesUsed = 0;
        }

        public void ResetWallJumpValues()
        {
            IsWallSlideFalling = false;
            UseWallJumpMoveStats = false;
            IsWallJumping = false;
            IsWallJumpFastFalling = false;
            IsWallJumpFalling = false;
            IsPastWallJumpApexThreshold = false;

            WallJumpFastFallReleaseSpeed = 0f;
            WallJumpTime = 0f;
        }

        public void ResetJumpValues()
        {
            IsJumping = false;
            IsFalling = false;
            IsFastFalling = false;
            FastFallTime = 0f;
            IsPastApexThreshold = false;
        }

        public void StopWallSlide()
        {
            if (IsWallSliding)
            {
                NumberOfJumpsUsed++;

                IsWallSliding = false;
            }
        }
    }
}


