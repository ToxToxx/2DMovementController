using UnityEngine;

namespace PlayerMovementRefactoring
{
    public class JumpHandler
    {
        private readonly PlayerMovement _playerMovement;

        public JumpHandler(PlayerMovement player)
        {
            _playerMovement = player;
        }

        public void JumpChecks()
        {
            if (InputManager.JumpWasPressed)
            {
                if (_playerMovement._isWallSlideFalling && _playerMovement._wallJumpPostBufferTimer >= 0f)
                {
                    return;
                }

                else if (_playerMovement._isWallSliding || (_playerMovement._isTouchingWall && _playerMovement._isGrounded)) { return; }

                _playerMovement._jumpBufferTimer = _playerMovement.MovementStats.JumpBufferTime;
                _playerMovement._jumpReleasedDuringBuffer = false;
            }

            if (InputManager.JumpWasReleased)
            {
                if (_playerMovement._jumpBufferTimer > 0)
                {
                    _playerMovement._jumpReleasedDuringBuffer = true;
                }

                if (_playerMovement._isJumping && _playerMovement.VerticalVelocity > 0f)
                {
                    if (_playerMovement._isPastApexThreshold)
                    {
                        _playerMovement._isPastApexThreshold = false;
                        _playerMovement._isFastFalling = true;
                        _playerMovement._fastFallTime = _playerMovement.MovementStats.TimeForUpwardsCancel;
                        _playerMovement.VerticalVelocity = 0f;
                    }
                    else
                    {
                        _playerMovement._isFastFalling = true;
                        _playerMovement._fastFallReleaseSpeed = _playerMovement.VerticalVelocity;
                    }
                }
            }

            //Initiating Jump
            if (_playerMovement._jumpBufferTimer > 0f && !_playerMovement._isJumping && (_playerMovement._isGrounded || _playerMovement._coyoteTimer > 0f))
            {
                InitiateJump(1);

                if (_playerMovement._jumpReleasedDuringBuffer)
                {
                    _playerMovement._isFastFalling = true;
                    _playerMovement._fastFallReleaseSpeed = _playerMovement.VerticalVelocity;
                }
            }

            //Double jump logic
            else if (_playerMovement._jumpBufferTimer > 0f && (_playerMovement._isJumping || _playerMovement._isWallJumping || _playerMovement._isWallSlideFalling || _playerMovement._isAirDashing || _playerMovement._isDashFastFalling) && !_playerMovement._isTouchingWall && _playerMovement._numberOfJumpsUsed < _playerMovement.MovementStats.NumberOfJumpsAllowed)
            {
                _playerMovement._isFastFalling = false;
                InitiateJump(1);

                if (_playerMovement._isDashFastFalling)
                {
                    _playerMovement._isDashFastFalling = false;
                }
            }

            //air jump after coyote time lapsed logic
            else if (_playerMovement._jumpBufferTimer > 0f && _playerMovement._isFalling && !_playerMovement._isWallSlideFalling && _playerMovement._numberOfJumpsUsed < _playerMovement.MovementStats.NumberOfJumpsAllowed - 1)//fixed bugs of uneccesary double jump in air
            {
                InitiateJump(2); //because we are falling and it's air jump
                _playerMovement._isFastFalling = false;
            }

        }

        private void InitiateJump(int numberOfJumpsUsed)
        {
            if (!_playerMovement._isJumping)
            {
                _playerMovement._isJumping = true;
            }

            _playerMovement.ResetWallJumpValues();

            _playerMovement._jumpBufferTimer = 0f;
            _playerMovement._numberOfJumpsUsed += numberOfJumpsUsed;
            _playerMovement.VerticalVelocity = _playerMovement.MovementStats.InitialJumpVelocity;
        }
        public void Jump()
        {
            //apply gravity
            if (_playerMovement._isJumping)
            {
                //check head bump
                if (_playerMovement._bumpedHead)
                {
                    _playerMovement._isFastFalling = true;

                }

                //gravity of ascending
                if (_playerMovement.VerticalVelocity >= 0f)
                {
                    //apex controls
                    _playerMovement._apexPoint = Mathf.InverseLerp(_playerMovement.MovementStats.InitialJumpVelocity, 0f, _playerMovement.VerticalVelocity);

                    if (_playerMovement._apexPoint > _playerMovement.MovementStats.ApexThreshhold)
                    {
                        if (!_playerMovement._isPastApexThreshold)
                        {
                            _playerMovement._isPastApexThreshold = true;
                            _playerMovement._timePastApexThreshold = 0f;
                        }
                        if (_playerMovement._isPastApexThreshold)
                        {
                            _playerMovement._timePastApexThreshold += Time.deltaTime;
                            if (_playerMovement._timePastApexThreshold < _playerMovement.MovementStats.ApexHangTime)
                            {
                                _playerMovement.VerticalVelocity = 0f;
                            }
                            else
                            {
                                _playerMovement.VerticalVelocity = -0.01f;
                            }
                        }
                    }

                    //gravity of ascending not past apex threshold
                    else if (!_playerMovement._isFastFalling)
                    {
                        _playerMovement.VerticalVelocity += _playerMovement.MovementStats.Gravity * Time.fixedDeltaTime;
                        if (_playerMovement._isPastApexThreshold)
                        {
                            _playerMovement._isPastApexThreshold = false;
                        }
                    }
                }

                //gravity of descending
                else if (!_playerMovement._isFastFalling)
                {
                    _playerMovement.VerticalVelocity += _playerMovement.MovementStats.Gravity * _playerMovement.MovementStats.GravityReleaseMultiplier * Time.fixedDeltaTime;
                }
                else if (_playerMovement.VerticalVelocity < 0f)
                {
                    if (!_playerMovement._isFalling)
                    {
                        _playerMovement._isFalling = true;
                    }
                }
            }

            //jump cut
            if (_playerMovement._isFastFalling)
            {
                if (_playerMovement._fastFallTime >= _playerMovement.MovementStats.TimeForUpwardsCancel)
                {
                    _playerMovement.VerticalVelocity += _playerMovement.MovementStats.Gravity * _playerMovement.MovementStats.GravityReleaseMultiplier * Time.fixedDeltaTime;
                }
                else if (_playerMovement._fastFallTime < _playerMovement.MovementStats.TimeForUpwardsCancel)
                {
                    _playerMovement.VerticalVelocity = Mathf.Lerp(_playerMovement._fastFallReleaseSpeed, 0f, (_playerMovement._fastFallTime / _playerMovement.MovementStats.TimeForUpwardsCancel));
                }
                _playerMovement._fastFallTime += Time.fixedDeltaTime;
            }
        }
    }
}

