using UnityEngine;


namespace PlayerMovementRefactoring
{
    public class CollisionChecksController
    {
        private readonly PlayerMovement _playerMovement;

        public CollisionChecksController(PlayerMovement player)
        {
            _playerMovement = player;
        }
        private void IsGrounded()
        {
            Vector2 boxCastOrigin = new(_playerMovement._feetCollider.bounds.center.x, _playerMovement._feetCollider.bounds.min.y);
            Vector2 boxCastSize = new(_playerMovement._feetCollider.bounds.size.x, _playerMovement.MovementStats.GroundDetectionRayLength);

            _playerMovement._groundHit = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, Vector2.down, _playerMovement.MovementStats.GroundDetectionRayLength, _playerMovement.MovementStats.GroundLayer);
            if (_playerMovement._groundHit.collider != null)
            {
                _playerMovement._isGrounded = true;
            }
            else _playerMovement._isGrounded = false;

        }

        private void BumpedHead()
        {
            Vector2 boxCastOrigin = new(_playerMovement._feetCollider.bounds.center.x, _playerMovement._bodyCollider.bounds.max.y);
            Vector2 boxCastSize = new(_playerMovement._feetCollider.bounds.size.x * _playerMovement.MovementStats.HeadWidth, _playerMovement.MovementStats.HeadDetectionRayLength);

            _playerMovement._headHit = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, Vector2.up, _playerMovement.MovementStats.HeadDetectionRayLength, _playerMovement.MovementStats.GroundLayer);
            if (_playerMovement._headHit.collider != null)
            {
                _playerMovement._bumpedHead = true;
            }
            else
            {
                _playerMovement._bumpedHead = false;
            }
        }

        private void IsTouchingWall()
        {
            float originEndPoint = 0f;
            if (_playerMovement._isFacingRight)
            {
                originEndPoint = _playerMovement._bodyCollider.bounds.max.x;
            }
            else originEndPoint = _playerMovement._bodyCollider.bounds.min.x;

            float adjustedHeight = _playerMovement._bodyCollider.bounds.size.y * _playerMovement.MovementStats.WallDetectionRayHeightMultiplier;

            Vector2 boxCastOrigin = new(originEndPoint, _playerMovement._bodyCollider.bounds.center.y);
            Vector2 boxCastSize = new(_playerMovement.MovementStats.WallDetectionRayLength, adjustedHeight);

            _playerMovement._wallHit = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, _playerMovement.transform.right, _playerMovement.MovementStats.WallDetectionRayLength, _playerMovement.MovementStats.GroundLayer);
            if (_playerMovement._wallHit.collider != null)
            {
                _playerMovement._lastWallHit = _playerMovement._wallHit;
                _playerMovement._isTouchingWall = true;
            }
            else _playerMovement._isTouchingWall = false;


        }
        public void CollisionChecks()
        {
            IsGrounded();
            BumpedHead();
            IsTouchingWall();
        }
    }
}
