using UnityEngine;

namespace PlayerMovementRefactoring
{
    public class LandFallController
    {
        private readonly PlayerMovement _playerMovement;

        public LandFallController(PlayerMovement player)
        {
            _playerMovement = player;
        }
        public void LandCheck()
        {
            //landed logic
            if ((_playerMovement._isJumping || _playerMovement._isFalling || _playerMovement._isWallJumping || _playerMovement._isWallJumpFalling || _playerMovement._isWallSlideFalling || _playerMovement._isWallSliding || _playerMovement._isDashFastFalling) && _playerMovement._isGrounded && _playerMovement.VerticalVelocity <= 0f)
            {
                _playerMovement.ResetJumpValues();
                _playerMovement.StopWallSlide();
                _playerMovement.ResetWallJumpValues();
                _playerMovement.ResetDashes();
                _playerMovement._numberOfJumpsUsed = 0;
                _playerMovement.VerticalVelocity = Physics2D.gravity.y;

                if (_playerMovement._isDashFastFalling && _playerMovement._isGrounded)
                {
                    _playerMovement.ResetDashValues();
                    return;
                }

                _playerMovement.ResetDashValues();
            }
        }

        public void Fall()
        {
            //normal gravity while falling
            if (!_playerMovement._isGrounded && !_playerMovement._isJumping && !_playerMovement._isWallSliding && !_playerMovement._isWallJumping && !_playerMovement._isDashing && !_playerMovement._isDashFastFalling)
            {
                if (!_playerMovement._isFalling)
                {
                    _playerMovement._isFalling = true;
                }
                _playerMovement.VerticalVelocity += _playerMovement.MovementStats.Gravity * Time.fixedDeltaTime;
            }
        }
    }


}