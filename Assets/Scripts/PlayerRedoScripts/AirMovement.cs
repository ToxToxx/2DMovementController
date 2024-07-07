using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirMovement
{
    private PlayerMovementRedo _player;
    private Rigidbody2D _playerRigidbody;
    private PlayerMovementStats _movementStats;

    public AirMovement(PlayerMovementRedo player, Rigidbody2D playerRigidbody, PlayerMovementStats movementStats)
    {
        _player = player;
        _playerRigidbody = playerRigidbody;
        _movementStats = movementStats;
    }

    public void Move(Vector2 moveInput)
    {
        if (moveInput != Vector2.zero)
        {
            _player.TurnCheck(moveInput);

            Vector2 targetVelocity = new Vector2(moveInput.x, 0f) * (InputManager.RunIsHeld ? _movementStats.MaxRunSpeed : _movementStats.MaxWalkSpeed);
            _playerRigidbody.velocity = new Vector2(Mathf.Lerp(_playerRigidbody.velocity.x, targetVelocity.x, _movementStats.AirAcceleration * Time.fixedDeltaTime), _playerRigidbody.velocity.y);
        }
        else
        {
            _playerRigidbody.velocity = new Vector2(Mathf.Lerp(_playerRigidbody.velocity.x, 0, _movementStats.AirDeceleration * Time.fixedDeltaTime), _playerRigidbody.velocity.y);
        }
    }
}
