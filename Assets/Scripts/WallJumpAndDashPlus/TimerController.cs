using UnityEngine;

namespace PlayerMovementRefactoring
{
    public class TimerController 
    {
        private readonly PlayerMovement _playerMovement;
        private WallJumpController _wallJumpController;

        public TimerController(PlayerMovement player, WallJumpController wallJumpController)
        {
            _playerMovement = player;
            _wallJumpController = wallJumpController;
        }
        public void CountTimers()
        {
            _playerMovement._jumpBufferTimer -= Time.deltaTime;


            if (!_playerMovement._isGrounded)
            {
                _playerMovement._coyoteTimer -= Time.deltaTime;
            }
            else _playerMovement._coyoteTimer = _playerMovement.MovementStats.JumpCoyoteTime;

            if (!_wallJumpController.ShouldApplyPostWallJumpBuffer())
            {
                _playerMovement._wallJumpPostBufferTimer -= Time.deltaTime;
            }

            //dash timer
            if (_playerMovement._isGrounded)
            {
                _playerMovement._dashOnGroundTimer -= Time.deltaTime;
            }
        }

    }
}
 
