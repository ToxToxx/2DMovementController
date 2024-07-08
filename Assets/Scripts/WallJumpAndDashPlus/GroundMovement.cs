using UnityEngine;

namespace PlayerMovementRefactoring
{
    public class GroundMovement
    {
        private PlayerMovement _playerMovement;

        public GroundMovement(PlayerMovement player)
        {
            _playerMovement = player;
        }

        public void Move(float acceleration, float deceleration, Vector2 moveInput)
        {
            if (!_playerMovement.IsDashing)
            {
                if (Mathf.Abs(moveInput.x) >= _playerMovement.MovementStats.MoveTreshold)
                {
                    _playerMovement.TurnCheck(moveInput);

                    float targetVelocity = 0f;
                    if (InputManager.RunIsHeld)
                    {
                        targetVelocity = moveInput.x * _playerMovement.MovementStats.MaxRunSpeed;
                    }
                    else
                    { targetVelocity = moveInput.x * _playerMovement.MovementStats.MaxWalkSpeed; }

                    _playerMovement.HorizontalVelocity = Mathf.Lerp(_playerMovement.HorizontalVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);
                }
                else if (Mathf.Abs(moveInput.x) <= _playerMovement.MovementStats.MoveTreshold)
                {
                    _playerMovement.HorizontalVelocity = Mathf.Lerp(_playerMovement.HorizontalVelocity, 0f, deceleration * Time.fixedDeltaTime);
                }
            }
        }
    }
}

