using UnityEngine;


namespace PlayerMovementRunJumpSeparateClasses
{
    public class PlayerMovementRedo : MonoBehaviour
    {

        [Header("References")]
        public PlayerMovementStats MovementStats;
        [SerializeField] private Collider2D _feetCollider;
        [SerializeField] private Collider2D _bodyCollider;

        private Rigidbody2D _playerRigidbody;

        private GroundMovement _groundMovement;
        private AirMovement _airMovement;
        private JumpHandler _jumpHandler;

        private bool _isFacingRight = true;
        public float VerticalVelocity;

        private void Awake()
        {
            _playerRigidbody = GetComponent<Rigidbody2D>();

            _groundMovement = new GroundMovement(this, _playerRigidbody, MovementStats);
            _airMovement = new AirMovement(this, _playerRigidbody, MovementStats);
            _jumpHandler = new JumpHandler(this, _playerRigidbody, MovementStats, _feetCollider, _bodyCollider);
        }

        private void Update()
        {
            _jumpHandler.CountTimers();
            _jumpHandler.JumpChecks();
        }

        private void FixedUpdate()
        {
            _jumpHandler.CollisionChecks();

            if (_jumpHandler.IsGrounded)
            {
                _groundMovement.Move(InputManager.Movement);
            }
            else
            {
                _airMovement.Move(InputManager.Movement);
            }

            _jumpHandler.ApplyJump();
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
    }
}

