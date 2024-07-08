using UnityEngine;

namespace PlayerMovementRunJumpSeparateClasses
{
    public class JumpHandler
    {

        private PlayerMovementRedo _playerMovementRedo;
        private Rigidbody2D _playerRigidbody;
        private PlayerMovementStats _movementStats;
        private Collider2D _feetCollider;
        private Collider2D _bodyCollider;

        private bool _jumpReleasedDuringBuffer;
        private bool _isJumping;
        private bool _isFalling;
        private bool _isFastFalling;
        private bool _isGrounded;
        private bool _bumpedHead;

        private float _jumpBufferTimer;
        private float _fastFallTime;
        private float _fastFallReleaseSpeed;

        private int _numberOfJumpsUsed;

        private float _apexPoint;
        private float _timePastApexThreshold;
        private bool _isPastApexThreshold;
        private float _coyoteTimer;

        public bool IsGrounded => _isGrounded;

        public JumpHandler(PlayerMovementRedo player, Rigidbody2D playerRigidbody, PlayerMovementStats movementStats, Collider2D feetCollider, Collider2D bodyCollider)
        {
            _playerMovementRedo = player;
            _playerRigidbody = playerRigidbody;
            _movementStats = movementStats;
            _feetCollider = feetCollider;
            _bodyCollider = bodyCollider;
        }

        public void CountTimers()
        {
            _jumpBufferTimer -= Time.deltaTime;

            if (!_isGrounded)
            {
                _coyoteTimer -= Time.deltaTime;
            }
            else
            {
                _coyoteTimer = _movementStats.JumpCoyoteTime;
            }
        }

        public void JumpChecks()
        {
            if (InputManager.JumpWasPressed)
            {
                _jumpBufferTimer = _movementStats.JumpBufferTime;
                _jumpReleasedDuringBuffer = false;
            }

            if (InputManager.JumpWasReleased)
            {
                if (_jumpBufferTimer > 0)
                {
                    _jumpReleasedDuringBuffer = true;
                }

                if (_isJumping && _playerMovementRedo.VerticalVelocity > 0f)
                {
                    if (_isPastApexThreshold)
                    {
                        _isPastApexThreshold = false;
                        _isFastFalling = true;
                        _fastFallTime = _movementStats.TimeForUpwardsCancel;
                        _playerMovementRedo.VerticalVelocity = 0f;
                    }
                    else
                    {
                        _isFastFalling = true;
                        _fastFallReleaseSpeed = _playerMovementRedo.VerticalVelocity;
                    }
                }
            }

            if (_jumpBufferTimer > 0f && !_isJumping && (_isGrounded || _coyoteTimer > 0f))
            {
                InitiateJump(1);

                if (_jumpReleasedDuringBuffer)
                {
                    _isFastFalling = true;
                    _fastFallReleaseSpeed = _playerMovementRedo.VerticalVelocity;
                }
            }
            else if (_jumpBufferTimer > 0f && _isJumping && _numberOfJumpsUsed < _movementStats.NumberOfJumpsAllowed)
            {
                _isFastFalling = false;
                InitiateJump(1);
            }
            else if (_jumpBufferTimer > 0f && _isFalling && _numberOfJumpsUsed < _movementStats.NumberOfJumpsAllowed - 1)
            {
                InitiateJump(2);
                _isFastFalling = false;
            }

            if ((_isJumping || _isFalling) && _isGrounded && _playerMovementRedo.VerticalVelocity <= 0f)
            {
                _isJumping = false;
                _isFalling = false;
                _isFastFalling = false;
                _isPastApexThreshold = false;
                _fastFallTime = 0f;
                _numberOfJumpsUsed = 0;
                _playerMovementRedo.VerticalVelocity = Physics2D.gravity.y;
            }
        }

        public void ApplyJump()
        {
            if (_isJumping)
            {
                if (_bumpedHead)
                {
                    _isFastFalling = true;
                }

                if (_playerMovementRedo.VerticalVelocity >= 0f)
                {
                    _apexPoint = Mathf.InverseLerp(_movementStats.InitialJumpVelocity, 0f, _playerMovementRedo.VerticalVelocity);

                    if (_apexPoint > _movementStats.ApexThreshhold)
                    {
                        if (!_isPastApexThreshold)
                        {
                            _isPastApexThreshold = true;
                            _timePastApexThreshold = 0f;
                        }
                        if (_isPastApexThreshold)
                        {
                            _timePastApexThreshold += Time.deltaTime;
                            if (_timePastApexThreshold < _movementStats.ApexHangTime)
                            {
                                _playerMovementRedo.VerticalVelocity = 0f;
                            }
                            else
                            {
                                _playerMovementRedo.VerticalVelocity = -0.01f;
                            }
                        }
                    }
                    else
                    {
                        _playerMovementRedo.VerticalVelocity += _movementStats.Gravity * Time.fixedDeltaTime;
                        if (_isPastApexThreshold)
                        {
                            _isPastApexThreshold = false;
                        }
                    }
                }
                else if (!_isFastFalling)
                {
                    _playerMovementRedo.VerticalVelocity += _movementStats.Gravity * _movementStats.GravityReleaseMultiplier * Time.fixedDeltaTime;
                }
                else if (_playerMovementRedo.VerticalVelocity < 0f)
                {
                    if (!_isFalling)
                    {
                        _isFalling = true;
                    }
                }
            }

            if (_isFastFalling)
            {
                if (_fastFallTime >= _movementStats.TimeForUpwardsCancel)
                {
                    _playerMovementRedo.VerticalVelocity += _movementStats.Gravity * _movementStats.GravityReleaseMultiplier * Time.fixedDeltaTime;
                }
                else if (_fastFallTime < _movementStats.TimeForUpwardsCancel)
                {
                    _playerMovementRedo.VerticalVelocity = Mathf.Lerp(_fastFallReleaseSpeed, 0f, (_fastFallTime / _movementStats.TimeForUpwardsCancel));
                }
                _fastFallTime += Time.fixedDeltaTime;
            }

            if (!_isGrounded && !_isJumping)
            {
                if (!_isFalling)
                {
                    _isFalling = true;
                }
                _playerMovementRedo.VerticalVelocity += _movementStats.Gravity * Time.fixedDeltaTime;
            }

            _playerMovementRedo.VerticalVelocity = Mathf.Clamp(_playerMovementRedo.VerticalVelocity, -_movementStats.MaxFallSpeed, 50f);

            _playerRigidbody.velocity = new Vector2(_playerRigidbody.velocity.x, _playerMovementRedo.VerticalVelocity);
        }

        private void InitiateJump(int numberOfJumpsUsed)
        {
            if (!_isJumping)
            {
                _isJumping = true;
            }

            _jumpBufferTimer = 0f;
            _numberOfJumpsUsed += numberOfJumpsUsed;
            _playerMovementRedo.VerticalVelocity = _movementStats.InitialJumpVelocity;
        }

        public void CollisionChecks()
        {
            IsOnGround();
            BumpedHead();
        }

        private void IsOnGround()
        {
            Vector2 boxCastOrigin = new Vector2(_feetCollider.bounds.center.x, _feetCollider.bounds.min.y);
            Vector2 boxCastSize = new Vector2(_feetCollider.bounds.size.x, _movementStats.GroundDetectionRayLength);

            RaycastHit2D _groundHit = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, Vector2.down, _movementStats.GroundDetectionRayLength, _movementStats.GroundLayer);
            _isGrounded = _groundHit.collider != null;
        }

        private void BumpedHead()
        {
            Vector2 boxCastOrigin = new Vector2(_feetCollider.bounds.center.x, _bodyCollider.bounds.max.y);
            Vector2 boxCastSize = new Vector2(_feetCollider.bounds.size.x * _movementStats.HeadWidth, _movementStats.HeadDetectionRayLength);

            RaycastHit2D _headHit = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, Vector2.up, _movementStats.HeadDetectionRayLength, _movementStats.GroundLayer);
            _bumpedHead = _headHit.collider != null;
        }
    }

}
