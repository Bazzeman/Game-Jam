using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private PlayerControllerSettingsSO _settings;

    private Rigidbody2D rb;
    private CapsuleCollider2D col;
    private FrameInput frameInput;
    private Vector2 frameVelocity;
    private bool cachedQueryStartInColliders;
    private float time;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<CapsuleCollider2D>();

        cachedQueryStartInColliders = Physics2D.queriesStartInColliders;
    }

    private void Update()
    {
        time += Time.deltaTime;
        GatherInput();
    }

    private void GatherInput()
    {
        frameInput = new FrameInput
        {
            IsJumpingDown = Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.C),
            IsJumpHeld = Input.GetButton("Jump") || Input.GetKey(KeyCode.C),
            WASD = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"))
        };

        if (_settings.SnapInput)
        {
            frameInput.WASD.x = Mathf.Abs(frameInput.WASD.x) < _settings.HorizontalDeadZoneThreshold ? 0 : Mathf.Sign(frameInput.WASD.x);
            frameInput.WASD.y = Mathf.Abs(frameInput.WASD.y) < _settings.VerticalDeadZoneThreshold ? 0 : Mathf.Sign(frameInput.WASD.y);
        }

        if (frameInput.IsJumpingDown)
        {
            _jumpToConsume = true;
            _timeJumpWasPressed = time;
        }
    }

    private void FixedUpdate()
    {
        CheckCollisions();

        HandleJump();
        HandleDirection();
        HandleGravity();
            
        ApplyMovement();
    }

    #region Collisions
        
    private float _frameLeftGrounded = float.MinValue;
    private bool _grounded;

    private void CheckCollisions()
    {
        Physics2D.queriesStartInColliders = false;

        // Ground and Ceiling
        bool groundHit = Physics2D.CapsuleCast(col.bounds.center, col.size, col.direction, 0, Vector2.down, _settings.GrounderDistance, ~_settings.PlayerLayer);
        bool ceilingHit = Physics2D.CapsuleCast(col.bounds.center, col.size, col.direction, 0, Vector2.up, _settings.GrounderDistance, ~_settings.PlayerLayer);

        // Hit a Ceiling
        if (ceilingHit) frameVelocity.y = Mathf.Min(0, frameVelocity.y);

        // Landed on the Ground
        if (!_grounded && groundHit)
        {
            _grounded = true;
            _coyoteUsable = true;
            _bufferedJumpUsable = true;
            _endedJumpEarly = false;
        }
        // Left the Ground
        else if (_grounded && !groundHit)
        {
            _grounded = false;
            _frameLeftGrounded = time;
        }

        Physics2D.queriesStartInColliders = cachedQueryStartInColliders;
    }

    #endregion

    #region Jumping

    private bool _jumpToConsume;
    private bool _bufferedJumpUsable;
    private bool _endedJumpEarly;
    private bool _coyoteUsable;
    private float _timeJumpWasPressed;

    private bool HasBufferedJump => _bufferedJumpUsable && time < _timeJumpWasPressed + _settings.JumpBuffer;
    private bool CanUseCoyote => _coyoteUsable && !_grounded && time < _frameLeftGrounded + _settings.CoyoteTime;

    private void HandleJump()
    {
        if (!_endedJumpEarly && !_grounded && !frameInput.IsJumpHeld && rb.linearVelocity.y > 0) _endedJumpEarly = true;

        if (!_jumpToConsume && !HasBufferedJump) return;

        if (_grounded || CanUseCoyote) ExecuteJump();

        _jumpToConsume = false;
    }

    private void ExecuteJump()
    {
        _endedJumpEarly = false;
        _timeJumpWasPressed = 0;
        _bufferedJumpUsable = false;
        _coyoteUsable = false;
        frameVelocity.y = _settings.JumpPower;
    }

    #endregion

    #region Horizontal

    private void HandleDirection()
    {
        if (frameInput.WASD.x == 0)
        {
            var deceleration = _grounded ? _settings.GroundDeceleration : _settings.AirDeceleration;
            frameVelocity.x = Mathf.MoveTowards(frameVelocity.x, 0, deceleration * Time.fixedDeltaTime);
        }
        else
        {
            frameVelocity.x = Mathf.MoveTowards(frameVelocity.x, frameInput.WASD.x * _settings.MaxSpeed, _settings.Acceleration * Time.fixedDeltaTime);
        }
    }

    #endregion

    #region Gravity

    private void HandleGravity()
    {
        if (_grounded && frameVelocity.y <= 0f)
        {
            frameVelocity.y = _settings.GroundingForce;
        }
        else
        {
            var inAirGravity = _settings.FallAcceleration;
            if (_endedJumpEarly && frameVelocity.y > 0) inAirGravity *= _settings.JumpEndEarlyGravityModifier;
            frameVelocity.y = Mathf.MoveTowards(frameVelocity.y, -_settings.MaxFallSpeed, inAirGravity * Time.fixedDeltaTime);
        }
    }

    #endregion

    private void ApplyMovement() => rb.linearVelocity = frameVelocity;
}

public struct FrameInput
{
    public bool IsJumpingDown;
    public bool IsJumpHeld;
    public Vector2 WASD;
}