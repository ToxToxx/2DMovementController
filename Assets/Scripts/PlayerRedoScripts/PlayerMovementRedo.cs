using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementRedo : MonoBehaviour
{

    [Header("References")]
    public PlayerMovementStats MovementStats;
    [SerializeField] private Collider2D _feetCollider;
    [SerializeField] private Collider2D _bodyCollider;

    private Rigidbody2D _playerRigidbody;

    private GroundMovement _groundMovement;
    private AirMovement _airMovement;
    private JumpHandler _jumpHandler;

    private void Awake()
    {
        _playerRigidbody = GetComponent<Rigidbody2D>();

        _groundMovement = new GroundMovement(this, _playerRigidbody, MovementStats);
        _airMovement = new AirMovement(this, _playerRigidbody, MovementStats);
        _jumpHandler = new JumpHandler(this, _playerRigidbody, MovementStats, _feetCollider, _bodyCollider);
    }

    private void Update()
    {
        _jumpHandler.CountTimers();
        _jumpHandler.JumpChecks();
    }

    private void FixedUpdate()
    {
        _jumpHandler.CollisionChecks();

        if (_jumpHandler.IsGrounded)
        {
            _groundMovement.Move(InputManager.Movement);
        }
        else
        {
            _airMovement.Move(InputManager.Movement);
        }

        _jumpHandler.ApplyJump();
    }
}
