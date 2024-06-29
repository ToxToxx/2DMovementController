using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    public PlayerMovementStats MovementStats;
    [SerializeField] private Collision2D _feetCollision;
    [SerializeField] private Collision2D _bodyCollision;

    private Rigidbody2D _playerRigidbody;

    //movementvars
    private Vector2 _moveVelocity;
    private bool _isFacingRight;

    //collision check vars
    private RaycastHit2D _groundHit;
    private RaycastHit2D _headHit;
    private bool _isGrounded;
    private bool _bumpedHead;


}
