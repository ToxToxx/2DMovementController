using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlayerMovementRefactoring
{
    public class WallSlideController : MonoBehaviour
    {
        private PlayerMovement _playerMovement;
        private Rigidbody2D _playerRigidbody;
        private PlayerMovementStats _movementStats;
        private JumpHandler _jumpHandler;

        public WallSlideController(PlayerMovement player, Rigidbody2D playerRigidbody, PlayerMovementStats movementStats, JumpHandler jumpHandler)
        {
            _playerMovement = player;
            _playerRigidbody = playerRigidbody;
            _movementStats = movementStats;
            _jumpHandler = jumpHandler;
        }
        public void WallSlideCheck()
        {
            if (_playerMovement._isTouchingWall && !_playerMovement._isGrounded && !_playerMovement._isDashing)
            {
                if (_playerMovement.VerticalVelocity < 0f && !_playerMovement._isWallSliding)
                {
                    _jumpHandler.ResetJumpValues();
                    ResetWallJumpValues();
                    ResetDashValues();

                    if (_playerMovement.MovementStats.ResetDashOnWallSlide)
                    {
                        ResetDashes();
                    }

                    _playerMovement._isWallSlideFalling = false;
                    _playerMovement._isWallSliding = true;

                    if (_playerMovement.MovementStats.ResetJumpOnWallSlide)
                    {
                        _playerMovement._numberOfJumpsUsed = 0;
                    }
                }
            }

            else if (_playerMovement._isWallSliding && !_playerMovement._isTouchingWall && !_playerMovement._isGrounded && !_playerMovement._isWallSlideFalling)
            {
                _playerMovement._isWallSlideFalling = true;
                StopWallSlide();
            }
            else
            {
                StopWallSlide();
            }
        }

        private void StopWallSlide()
        {
            if (_playerMovement._isWallSliding)
            {
                _playerMovement._numberOfJumpsUsed++;

                _playerMovement._isWallSliding = false;
            }
        }

        public void WallSlide()
        {
            if (_playerMovement._isWallSliding)
            {
                _playerMovement.VerticalVelocity = Mathf.Lerp(_playerMovement.VerticalVelocity, -_playerMovement.MovementStats.WallSlideSpeed, _playerMovement.MovementStats.WallSlideDecelerationSpeed * Time.fixedDeltaTime);

            }
        }
    }
}