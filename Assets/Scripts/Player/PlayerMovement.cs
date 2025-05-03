using System;
using UnityEngine;
using static UnityEngine.InputSystem.InputAction;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private PlayerControllerSettingsSO _settings;

    private Rigidbody2D rb;
    private CapsuleCollider2D col;
    private InputSystem_Actions controls;
    private InputSystem_Actions.PlayerActions playerActions;
    private Action<CallbackContext> onJumpPerformed;
    private Action<CallbackContext> onJumpCanceled;
    private Action<CallbackContext> onMovePerformed;
    private Action<CallbackContext> onMoveCanceled;
    private FrameInput currentFrameInput;
    private FrameInput previousFrameInput;
    private Vector2 frameVelocity;
    private Vector2 inputMovementVector;
    private bool cachedQueryStartInColliders;
    private bool jumpToConsume;
    private bool bufferedJumpUsable;
    private bool endedJumpEarly;
    private bool coyoteUsable;
    private bool grounded;
    private bool isJumpBeingHeld = false;
    private float time;
    private float timeJumpWasPressed;
    private float frameLeftGrounded = float.MinValue;

    private bool HasBufferedJump => bufferedJumpUsable && time < timeJumpWasPressed + _settings.JumpBuffer;
    private bool CanUseCoyote => coyoteUsable && !grounded && time < frameLeftGrounded + _settings.CoyoteTime;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<CapsuleCollider2D>();

        cachedQueryStartInColliders = Physics2D.queriesStartInColliders;

        controls = new();
        playerActions = controls.Player;

        onJumpPerformed = ctx => isJumpBeingHeld = true;
        onJumpCanceled = ctx => isJumpBeingHeld = false;

        onMovePerformed = ctx => inputMovementVector = ctx.ReadValue<Vector2>();
        onMoveCanceled = ctx => inputMovementVector = Vector2.zero;

        RegisterInputCallbacks();
    }

    private void OnEnable() => playerActions.Enable();

    private void OnDisable() => playerActions.Disable();

    private void OnDestroy() => UnRegisterInputCallbacks();

    private void Update()
    {
        time += Time.deltaTime;
        GatherInput();
        HandleInputChange();
    }

    private void FixedUpdate()
    {
        CheckCollisions();

        HandleJump();
        HandleDirection();
        HandleGravity();

        ApplyMovement();
    }

    private void RegisterInputCallbacks()
    {
        playerActions.Jump.performed += onJumpPerformed;
        playerActions.Jump.canceled += onJumpCanceled;
        playerActions.Move.performed += onMovePerformed;
        playerActions.Move.canceled += onMoveCanceled;
    }

    private void UnRegisterInputCallbacks()
    {
        playerActions.Jump.performed -= onJumpPerformed;
        playerActions.Jump.canceled -= onJumpCanceled;
        playerActions.Move.performed -= onMovePerformed;
        playerActions.Move.canceled -= onMoveCanceled;
    }

    private void GatherInput()
    {
        currentFrameInput = new FrameInput
        {
            IsJumpingDown = playerActions.Jump.WasPressedThisFrame(),
            IsJumpHeld = isJumpBeingHeld,
            WASD = inputMovementVector
        };

        if (_settings.SnapInput)
        {
            currentFrameInput.WASD.x = Mathf.Abs(currentFrameInput.WASD.x) < _settings.HorizontalDeadZoneThreshold ? 0 : Mathf.Sign(currentFrameInput.WASD.x);
            currentFrameInput.WASD.y = Mathf.Abs(currentFrameInput.WASD.y) < _settings.VerticalDeadZoneThreshold ? 0 : Mathf.Sign(currentFrameInput.WASD.y);
        }

        if (currentFrameInput.IsJumpingDown)
        {
            jumpToConsume = true;
            timeJumpWasPressed = time;
        }
    }

    private void HandleInputChange()
    {
        if (!currentFrameInput.Equals(previousFrameInput)) {
            Debug.Log("Input change detected!");

            var dateTimeInput = new DateTimeInput
            {
                IsJumpingDown = currentFrameInput.IsJumpingDown,
                IsJumpHeld = currentFrameInput.IsJumpHeld,
                WASD = currentFrameInput.WASD,
                DateTime = DateTime.Now,
            };
        }

        previousFrameInput = currentFrameInput;
    }

    private void CheckCollisions()
    {
        Physics2D.queriesStartInColliders = false;

        // Ground and Ceiling
        bool groundHit = Physics2D.CapsuleCast(col.bounds.center, col.size, col.direction, 0, Vector2.down, _settings.GrounderDistance, ~_settings.PlayerLayer);
        bool ceilingHit = Physics2D.CapsuleCast(col.bounds.center, col.size, col.direction, 0, Vector2.up, _settings.GrounderDistance, ~_settings.PlayerLayer);

        // Hit a Ceiling
        if (ceilingHit) frameVelocity.y = Mathf.Min(0, frameVelocity.y);

        // Landed on the Ground
        if (!grounded && groundHit)
        {
            grounded = true;
            coyoteUsable = true;
            bufferedJumpUsable = true;
            endedJumpEarly = false;
        }
        // Left the Ground
        else if (grounded && !groundHit)
        {
            grounded = false;
            frameLeftGrounded = time;
        }

        Physics2D.queriesStartInColliders = cachedQueryStartInColliders;
    }

    private void HandleJump()
    {
        if (!endedJumpEarly && !grounded && !currentFrameInput.IsJumpHeld && rb.linearVelocity.y > 0) endedJumpEarly = true;

        if (!jumpToConsume && !HasBufferedJump) return;

        if (grounded || CanUseCoyote) ExecuteJump();

        jumpToConsume = false;
    }

    private void ExecuteJump()
    {
        endedJumpEarly = false;
        timeJumpWasPressed = 0;
        bufferedJumpUsable = false;
        coyoteUsable = false;
        frameVelocity.y = _settings.JumpPower;
    }

    private void HandleDirection()
    {
        if (currentFrameInput.WASD.x == 0)
        {
            var deceleration = grounded ? _settings.GroundDeceleration : _settings.AirDeceleration;
            frameVelocity.x = Mathf.MoveTowards(frameVelocity.x, 0, deceleration * Time.fixedDeltaTime);
        }
        else frameVelocity.x = Mathf.MoveTowards(frameVelocity.x, currentFrameInput.WASD.x * _settings.MaxSpeed, _settings.Acceleration * Time.fixedDeltaTime);
    }

    private void HandleGravity()
    {
        if (grounded && frameVelocity.y <= 0f) frameVelocity.y = _settings.GroundingForce;
        else
        {
            var inAirGravity = _settings.FallAcceleration;
            if (endedJumpEarly && frameVelocity.y > 0) inAirGravity *= _settings.JumpEndEarlyGravityModifier;
            frameVelocity.y = Mathf.MoveTowards(frameVelocity.y, -_settings.MaxFallSpeed, inAirGravity * Time.fixedDeltaTime);
        }
    }

    private void ApplyMovement() => rb.linearVelocity = frameVelocity;

    private struct FrameInput
    {
        public bool IsJumpingDown;
        public bool IsJumpHeld;
        public Vector2 WASD;

        public bool Equals(FrameInput other)
            => IsJumpingDown == other.IsJumpingDown
            && IsJumpHeld == other.IsJumpHeld
            && WASD.Equals(other.WASD);
    }
}
