using UnityEngine;


namespace PlayerMovementRefactoring
{
    public class WallJumpController
    {
        private PlayerMovement _playerMovement;

        public WallJumpController(PlayerMovement player)
        {
            _playerMovement = player;
        }
        public void WallJumpCheck()
        {
            if (ShouldApplyPostWallJumpBuffer())
            {
                _playerMovement._wallJumpPostBufferTimer = _playerMovement.MovementStats.WallJumpPostBufferTime;
            }

            //wall jump fast falling
            if (InputManager.JumpWasReleased && !_playerMovement._isWallSliding && !_playerMovement._isTouchingWall && _playerMovement._isWallJumping)
            {
                if (_playerMovement.VerticalVelocity > 0f)
                {
                    if (_playerMovement._isPastWallJumpApexThreshold)
                    {
                        _playerMovement._isPastWallJumpApexThreshold = false;
                        _playerMovement._isWallJumpFastFalling = true;
                        _playerMovement._wallJUmpFastFallTime = _playerMovement.MovementStats.TimeForUpwardsCancel;

                        _playerMovement.VerticalVelocity = 0F;
                    }
                    else
                    {
                        _playerMovement._isWallJumpFastFalling = true;
                        _playerMovement._wallJumpFastFallReleaseSpeed = _playerMovement.VerticalVelocity;
                    }
                }
            }

            //actual jump with post wall jump buffer time
            if (InputManager.JumpWasPressed && _playerMovement._wallJumpPostBufferTimer > 0f)
            {
                InitialWallJump();
            }
        }

        private void InitialWallJump()
        {
            if (!_playerMovement._isWallJumping)
            {
                _playerMovement._isWallJumping = true;
                _playerMovement._useWallJumpMoveStats = true;
            }

            _playerMovement.StopWallSlide();
            _playerMovement.ResetJumpValues();
            _playerMovement._wallJumpTime = 0f;

            _playerMovement.VerticalVelocity = _playerMovement.MovementStats.InitialWallJumpVelocity;

            int dirMultiplier = 0;
            Vector2 hitPoint = _playerMovement._lastWallHit.collider.ClosestPoint(_playerMovement._bodyCollider.bounds.center);

            if (hitPoint.x > _playerMovement.transform.position.x)
            {
                dirMultiplier = -1;
            }
            else dirMultiplier = 1;

            _playerMovement.HorizontalVelocity = Mathf.Abs(_playerMovement.MovementStats.WallJumpDirection.x) * dirMultiplier;
        }

        public void WallJump()
        {
            //Apply Wall Jump Gravity
            if (_playerMovement._isWallJumping)
            {
                _playerMovement._wallJumpTime += Time.fixedDeltaTime;
                if (_playerMovement._wallJumpTime >= _playerMovement.MovementStats.TimeTillJumpApex)
                {
                    _playerMovement._useWallJumpMoveStats = false;
                }

                //Hit head
                if (_playerMovement._bumpedHead)
                {
                    _playerMovement._isWallJumpFastFalling = true;
                    _playerMovement._useWallJumpMoveStats = false;
                }

                //Gravity In Ascending
                if (_playerMovement.VerticalVelocity >= 0f)
                {
                    //Apex controls
                    _playerMovement._wallJumpApexPoint = Mathf.InverseLerp(_playerMovement.MovementStats.WallJumpDirection.y, 0f, _playerMovement.VerticalVelocity);

                    if (_playerMovement._wallJumpApexPoint > _playerMovement.MovementStats.ApexThreshhold)
                    {
                        if (!_playerMovement._isPastWallJumpApexThreshold)
                        {
                            _playerMovement._isPastWallJumpApexThreshold = true;
                            _playerMovement._timePastWallJumpApexThreshold = 0f;
                        }
                        if (_playerMovement._isPastWallJumpApexThreshold)
                        {
                            _playerMovement._timePastWallJumpApexThreshold += Time.fixedDeltaTime;
                            if (_playerMovement._timePastWallJumpApexThreshold < _playerMovement.MovementStats.ApexHangTime)
                            {
                                _playerMovement.VerticalVelocity = 0f;
                            }
                            else
                            {
                                _playerMovement.VerticalVelocity = -0.01f;
                            }
                        }
                    }
                    // Gravity in ascending but not past apex threshold
                    else if (!_playerMovement._isWallJumpFastFalling)
                    {
                        _playerMovement.VerticalVelocity += _playerMovement.MovementStats.WallJumpGravity * Time.fixedDeltaTime;

                        if (_playerMovement._isPastWallJumpApexThreshold)
                        {
                            _playerMovement._isPastWallJumpApexThreshold = false;
                        }
                    }
                }
                // Gravity in Descending
                else if (!_playerMovement._isWallJumpFastFalling)
                {
                    _playerMovement.VerticalVelocity += _playerMovement.MovementStats.WallJumpGravity * Time.fixedDeltaTime;
                }

                else if (_playerMovement.VerticalVelocity < 0f)
                {
                    if (!_playerMovement._isWallJumpFalling)
                        _playerMovement._isWallJumpFalling = true;
                }
            }

            //handle wall jump cut time
            if (_playerMovement._isWallJumpFastFalling)
            {
                if (_playerMovement._wallJUmpFastFallTime >= _playerMovement.MovementStats.TimeForUpwardsCancel)
                {
                    _playerMovement.VerticalVelocity += _playerMovement.MovementStats.WallJumpGravity * _playerMovement.MovementStats.WallJumpGravityOnReleaseMultiplier * Time.fixedDeltaTime;
                }
                else if (_playerMovement._wallJUmpFastFallTime < _playerMovement.MovementStats.TimeForUpwardsCancel)
                {
                    _playerMovement.VerticalVelocity = Mathf.Lerp(_playerMovement._wallJumpFastFallReleaseSpeed, 0f, (_playerMovement._wallJUmpFastFallTime / _playerMovement.MovementStats.TimeForUpwardsCancel));
                }

                _playerMovement._wallJUmpFastFallTime += Time.fixedDeltaTime;
            }
        }

        public bool ShouldApplyPostWallJumpBuffer()
        {
            if (!_playerMovement._isGrounded && (_playerMovement._isTouchingWall || _playerMovement._isWallSliding))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

    }
}

