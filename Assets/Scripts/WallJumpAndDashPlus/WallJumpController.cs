using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace PlayerMovementRefactoring
{
    public class WallJumpController : MonoBehaviour
    {
        private PlayerMovement _playerMovement;
        private Rigidbody2D _playerRigidbody;
        private PlayerMovementStats _movementStats;

        public WallJumpController(PlayerMovement player, Rigidbody2D playerRigidbody, PlayerMovementStats movementStats)
        {
            _playerMovement = player;
            _playerRigidbody = playerRigidbody;
            _movementStats = movementStats;
        }
        private void WallJumpCheck()
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

            StopWallSlide();
            ResetJumpValues();
            _playerMovement._wallJumpTime = 0f;

            _playerMovement.VerticalVelocity = _playerMovement.MovementStats.InitialWallJumpVelocity;

            int dirMultiplier = 0;
            Vector2 hitPoint = _playerMovement._lastWallHit.collider.ClosestPoint(_playerMovement._bodyCollider.bounds.center);

            if (hitPoint.x > transform.position.x)
            {
                dirMultiplier = -1;
            }
            else dirMultiplier = 1;

            _playerMovement.HorizontalVelocity = Mathf.Abs(_playerMovement.MovementStats.WallJumpDirection.x) * dirMultiplier;
        }

        private void WallJump()
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
                        if (!_isPastWallJumpApexThreshold)
                        {
                            _isPastWallJumpApexThreshold = true;
                            _timePastWallJumpApexThreshold = 0f;
                        }
                        if (_isPastWallJumpApexThreshold)
                        {
                            _timePastWallJumpApexThreshold += Time.fixedDeltaTime;
                            if (_timePastWallJumpApexThreshold < MovementStats.ApexHangTime)
                            {
                                VerticalVelocity = 0f;
                            }
                            else
                            {
                                VerticalVelocity = -0.01f;
                            }
                        }
                    }
                    // Gravity in ascending but not past apex threshold
                    else if (!_isWallJumpFastFalling)
                    {
                        VerticalVelocity += MovementStats.WallJumpGravity * Time.fixedDeltaTime;

                        if (_isPastWallJumpApexThreshold)
                        {
                            _isPastWallJumpApexThreshold = false;
                        }
                    }
                }
                // Gravity in Descending
                else if (!_isWallJumpFastFalling)
                {
                    VerticalVelocity += MovementStats.WallJumpGravity * Time.fixedDeltaTime;
                }

                else if (VerticalVelocity < 0f)
                {
                    if (!_isWallJumpFalling)
                        _isWallJumpFalling = true;
                }
            }

            //handle wall jump cut time
            if (_isWallJumpFastFalling)
            {
                if (_wallJUmpFastFallTime >= MovementStats.TimeForUpwardsCancel)
                {
                    VerticalVelocity += MovementStats.WallJumpGravity * MovementStats.WallJumpGravityOnReleaseMultiplier * Time.fixedDeltaTime;
                }
                else if (_wallJUmpFastFallTime < MovementStats.TimeForUpwardsCancel)
                {
                    VerticalVelocity = Mathf.Lerp(_wallJumpFastFallReleaseSpeed, 0f, (_wallJUmpFastFallTime / MovementStats.TimeForUpwardsCancel));
                }

                _wallJUmpFastFallTime += Time.fixedDeltaTime;
            }
        }
    }
}

