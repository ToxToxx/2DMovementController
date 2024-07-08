using UnityEngine;

namespace PlayerMovementRefactoring
{
    public class WallSlideController
    {
        private PlayerMovement _playerMovement;

        public WallSlideController(PlayerMovement player)
        {
            _playerMovement = player;;
        }
        public void WallSlideCheck()
        {
            if (_playerMovement._isTouchingWall && !_playerMovement._isGrounded && !_playerMovement._isDashing)
            {
                if (_playerMovement.VerticalVelocity < 0f && !_playerMovement._isWallSliding)
                {
                    _playerMovement.ResetJumpValues();
                    _playerMovement.ResetWallJumpValues();
                    _playerMovement.ResetDashValues();

                    if (_playerMovement.MovementStats.ResetDashOnWallSlide)
                    {
                        _playerMovement.ResetDashes();
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
                _playerMovement.StopWallSlide();
            }
            else
            {
                _playerMovement.StopWallSlide();
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