using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlayerMovementRefactoring
{
    public class LandFallController : MonoBehaviour
    {
        private PlayerMovement _playerMovement;
        private Rigidbody2D _playerRigidbody;
        private PlayerMovementStats _movementStats;
        private JumpHandler _jumpHandler;

        public LandFallController(PlayerMovement player, Rigidbody2D playerRigidbody, PlayerMovementStats movementStats, JumpHandler jumpHandler)
        {
            _playerMovement = player;
            _playerRigidbody = playerRigidbody;
            _movementStats = movementStats;
            _jumpHandler = jumpHandler;
        }
        public void LandCheck()
        {
            //landed logic
            if ((_playerMovement._isJumping || _playerMovement._isFalling || _playerMovement._isWallJumping || _playerMovement._isWallJumpFalling || _playerMovement._isWallSlideFalling || _playerMovement._isWallSliding || _playerMovement._isDashFastFalling) && _playerMovement._isGrounded && _playerMovement.VerticalVelocity <= 0f)
            {
                _jumpHandler.ResetJumpValues();
                StopWallSlide();
                ResetWallJumpValues();
                ResetDashes();
                _playerMovement._numberOfJumpsUsed = 0;
                _playerMovement.VerticalVelocity = Physics2D.gravity.y;

                if (_playerMovement._isDashFastFalling && _playerMovement._isGrounded)
                {
                    ResetDashValues();
                    return;
                }

                ResetDashValues();
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