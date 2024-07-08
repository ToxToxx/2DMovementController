using UnityEngine;

namespace PlayerMovementRefactoring
{
    public class DashController
    {
        private readonly PlayerMovement _playerMovement;

        public DashController(PlayerMovement player)
        {
            _playerMovement = player;
        }
        public void DashCheck()
        {
            if (InputManager.DashWasPressed)
            {
                //ground dash
                if (_playerMovement._isGrounded && _playerMovement._dashOnGroundTimer < 0 && !_playerMovement._isDashing)
                {
                    InitiateDash();
                }

                //air dash
                else if (!_playerMovement._isGrounded && !_playerMovement._isDashing && _playerMovement._numberOfDashesUsed < _playerMovement.MovementStats.NumberOfDashes)
                {
                    _playerMovement._isAirDashing = true;
                    InitiateDash();

                    //you left a wallslide but dashed within the wall jump post buffer timer
                    if (_playerMovement._wallJumpPostBufferTimer > 0f)
                    {
                        _playerMovement._numberOfJumpsUsed--;
                        if (_playerMovement._numberOfJumpsUsed < 0f)
                        {
                            _playerMovement._numberOfJumpsUsed = 0;
                        }
                    }
                }
            }
        }

        private void InitiateDash()
        {
            _playerMovement._dashDirection = InputManager.Movement;

            Vector2 closestDirection = Vector2.zero;
            float minDistance = Vector2.Distance(_playerMovement._dashDirection, _playerMovement.MovementStats.DashDirections[0]);

            for (int i = 0; i < _playerMovement.MovementStats.DashDirections.Length; i++)
            {
                //skip if we hit it bang on
                if (_playerMovement._dashDirection == _playerMovement.MovementStats.DashDirections[i])
                {
                    closestDirection = _playerMovement._dashDirection;
                    break;
                }

                float distance = Vector2.Distance(_playerMovement._dashDirection, _playerMovement.MovementStats.DashDirections[i]);

                //if diagonal direction
                bool isDiagonal = (Mathf.Abs(_playerMovement.MovementStats.DashDirections[i].x) == 1 && Mathf.Abs(_playerMovement.MovementStats.DashDirections[i].y) == 1);
                if (isDiagonal)
                {
                    distance -= _playerMovement.MovementStats.DashDiagonallyBias;
                }
                else if (distance < minDistance)
                {
                    minDistance = distance;
                    closestDirection = _playerMovement.MovementStats.DashDirections[i];
                }
            }

            //handle direction with no input
            if (closestDirection == Vector2.zero)
            {
                if (_playerMovement._isFacingRight)
                {
                    closestDirection = Vector2.right;
                }
                else closestDirection = Vector2.left;
            }

            _playerMovement._dashDirection = closestDirection;
            _playerMovement._numberOfDashesUsed++;
            _playerMovement._isDashing = true;
            _playerMovement._dashTimer = 0f;
            _playerMovement._dashOnGroundTimer = _playerMovement.MovementStats.TimeBtwDashesOnGround;

            _playerMovement.ResetJumpValues();
            _playerMovement.ResetWallJumpValues();
            _playerMovement.StopWallSlide();
        }
        public void Dash()
        {
            if (_playerMovement._isDashing)
            {
                //stop the dash after the timer
                _playerMovement._dashTimer += Time.fixedDeltaTime;
                if (_playerMovement._dashTimer >= _playerMovement.MovementStats.DashTime)
                {
                    if (_playerMovement._isGrounded)
                    {
                        _playerMovement.ResetDashes();
                    }

                    _playerMovement._isAirDashing = false;
                    _playerMovement._isDashing = false;

                    if (!_playerMovement._isJumping && !_playerMovement._isWallJumping)
                    {
                        _playerMovement._dashFastFallTime = 0f;
                        _playerMovement._dashFastFallReleaseSpeed = _playerMovement.VerticalVelocity;

                        if (!_playerMovement._isGrounded)
                        {
                            _playerMovement._isDashFastFalling = true;
                        }
                    }

                    return;
                }

                _playerMovement.HorizontalVelocity = _playerMovement.MovementStats.DashSpeed * _playerMovement._dashDirection.x;

                if (_playerMovement._dashDirection.y != 0f || _playerMovement._isAirDashing)
                {
                    _playerMovement.VerticalVelocity = _playerMovement.MovementStats.DashSpeed * _playerMovement._dashDirection.y;
                }
            }

            //Handle dash cut time
            else if (_playerMovement._isDashFastFalling)
            {
                if (_playerMovement.VerticalVelocity > 0f)
                {
                    if (_playerMovement._dashFastFallTime < _playerMovement.MovementStats.DashTimeForUpwardsCancel)
                    {
                        _playerMovement.VerticalVelocity = Mathf.Lerp(_playerMovement._dashFastFallReleaseSpeed, 0f, (_playerMovement._dashFastFallTime / _playerMovement.MovementStats.DashTimeForUpwardsCancel));
                    }
                    else if (_playerMovement._dashFastFallTime >= _playerMovement.MovementStats.DashTimeForUpwardsCancel)
                    {
                        _playerMovement.VerticalVelocity += _playerMovement.MovementStats.Gravity * _playerMovement.MovementStats.DashGravityOnReleaseMultiplier * Time.fixedDeltaTime;
                    }

                    _playerMovement._dashFastFallTime += Time.fixedDeltaTime;
                }
                else
                {
                    _playerMovement.VerticalVelocity += _playerMovement.MovementStats.Gravity * _playerMovement.MovementStats.DashGravityOnReleaseMultiplier * Time.fixedDeltaTime;
                }
            }
        }


    }

}