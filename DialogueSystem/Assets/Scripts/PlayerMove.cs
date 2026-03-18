using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMove : MonoBehaviour
{
    #region Movement Settings
    [SerializeField]
    [Tooltip("Maximum movement speed of the player")]
    private float speed = 5f;

    [SerializeField]
    [Tooltip("How quickly the player accelerates when moving")]
    private float acceleration = 20f;

    [SerializeField]
    [Tooltip("How quickly the player slows down when no input is given")]
    private float deceleration = 15f;
    #endregion

    #region Jump Settings
    [SerializeField]
    [Tooltip("Force applied when the player jumps")]
    private float jumpForce = 5f;

    [SerializeField]
    [Tooltip("Damping applied while grounded to prevent sliding")]
    private float groundDrag = 0.3f;

    [SerializeField]
    [Tooltip("Multiplier for fall speed (higher = faster falling). 1.0 = normal gravity, 2.0 = twice as fast")]
    private float fallMultiplier = 2f;

    private const int MaxJumps = 2;
    #endregion

    #region Physics
    private Rigidbody _rb;
    #endregion

    #region Movement State
    private Vector3 _currentVelocity = Vector3.zero;
    private Vector3 _movement;
    #endregion

    #region Jump State
    private bool _isGrounded;
    private int _jumpsRemaining = 1;
    #endregion

    #region Initialization
    void Start()
    {
        // Cache the Rigidbody component for efficient physics access
        _rb = GetComponent<Rigidbody>();
    }
    #endregion

    #region Input Handling
    void Update()
    {
        // Get player input for movement direction
        Vector3 direction = Vector3.zero;
        if (Keyboard.current.wKey.isPressed) direction += Vector3.forward;
        if (Keyboard.current.sKey.isPressed) direction += Vector3.back;
        if (Keyboard.current.aKey.isPressed) direction += Vector3.left;
        if (Keyboard.current.dKey.isPressed) direction += Vector3.right;
        
        // Normalize direction to ensure consistent speed in all directions (including diagonals)
        if (direction != Vector3.zero) direction = direction.normalized;
        _movement = direction;

        // Handle jump input: check if space is pressed and if player can jump
        if (Keyboard.current.spaceKey.wasPressedThisFrame && (_isGrounded || _jumpsRemaining > 0))
        {
            Jump();
        }
    }
    #endregion

    #region Physics & Movement
    void FixedUpdate()
    {
        // Calculate target velocity based on input and max speed
        Vector3 targetVelocity = _movement * speed;
        targetVelocity.y = _rb.linearVelocity.y; // Preserve gravity's vertical velocity
        
        // Smoothly interpolate between current and target velocity
        // Uses acceleration when moving, deceleration when idle for responsive feel
        float accelerationRate = _movement.magnitude > 0 ? acceleration : deceleration;
        _currentVelocity = Vector3.Lerp(_currentVelocity, targetVelocity, accelerationRate * Time.fixedDeltaTime);
        
        // Apply the smoothed velocity to the rigidbody
        _rb.linearVelocity = _currentVelocity;

        // Apply damping (friction) when grounded to prevent sliding
        if (_isGrounded)
        {
            _rb.linearDamping = groundDrag;
        }
        else
        {
            _rb.linearDamping = 0; // No friction in the air
            
            // Apply extra downward force when falling for faster fall speed
            if (_rb.linearVelocity.y < 0)
            {
                _rb.linearVelocity += Vector3.down * (Physics.gravity.magnitude * (fallMultiplier - 1f) * Time.fixedDeltaTime);
            }
        }
    }
    #endregion

    #region Jump System
    private void Jump()
    {
        // Apply upward impulse force for the jump
        _rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        _isGrounded = false;
        _jumpsRemaining--;
    }
    #endregion

    #region Collision Detection
    private void OnCollisionEnter(Collision collision)
    {
        // Detect when player touches the ground and reset jump counter
        if (collision.gameObject.CompareTag("Ground"))
        {
            _isGrounded = true;
            _jumpsRemaining = MaxJumps; // Reset to 2 jumps (1 from ground + 1 mid-air)
        }
    }
    #endregion
}
