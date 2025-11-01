using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator), typeof(SpriteRenderer))]
public class PlayerConttroller : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 6f;

    [Header("Jump")]
    [SerializeField] private float jumpVelocity = 10f;            // vertical velocity applied on jump
    [SerializeField] private float doubleJumpMultiplier = 1f;     // multiplier for second jump (1 = same force)
    [SerializeField] private int maxJumps = 2;                    // number of total jumps (1 = single jump, 2 = double)

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;               // a point under the player
    [SerializeField] private float groundCheckRadius = 0.12f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Effect")] 
    [SerializeField] private ParticleSystem dustEffect;
    [SerializeField] private GameObject doubleJumpEffect;
    
    private Vector2 _movementAxis;
    private Rigidbody2D _rb;
    private SpriteRenderer _sr;
    private Animator _anim;

    private bool _isGrounded;
    private bool _wasGroundedLastFrame;
    private int _jumpsRemaining;

    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _anim = GetComponent<Animator>();
        _sr = GetComponent<SpriteRenderer>();
        _jumpsRemaining = maxJumps;
        if (groundCheck == null) groundCheck = transform; // fallback
    }

    void Update()
    {
        // ground check using OverlapCircle (more reliable than tiny Linecast)
        _isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // Detect landing
        if (_wasGroundedLastFrame && _isGrounded)
        {
            dustEffect.Play();
            _wasGroundedLastFrame = false;
        }
        
        // reset jumps when grounded
        if (_isGrounded)
        {
            _jumpsRemaining = maxJumps;
            
        }
        else
        {
            _wasGroundedLastFrame = true;
        }

        Animating();
    }

    void FixedUpdate()
    {
        Moving();
    }

    private void Moving()
    {
        // preserve vertical velocity and set horizontal velocity properly
        Vector2 vel = _rb.linearVelocity;
        vel.x = _movementAxis.x * speed;
        _rb.linearVelocity = vel;
    }

    private void Animating()
    {
        _anim.SetBool("IsGrounded", _isGrounded);
        _anim.SetBool("Walking", Mathf.Abs(_movementAxis.x) > 0.01f);
        if (_movementAxis.x < 0) _sr.flipX = true;
        else if (_movementAxis.x > 0) _sr.flipX = false;
    }

    public void MoveAxis(InputAction.CallbackContext context)
    {
        if (GameManager.Instance.gameState == GameState.Paused) return;

        _movementAxis = context.ReadValue<Vector2>();
    }

    public void JumpButton(InputAction.CallbackContext context)
    {
        if (!context.performed || GameManager.Instance.gameState == GameState.Paused) return; // only act on performed phase

        if (_jumpsRemaining > 0)
        {
            // compute jump velocity: same for first jump, maybe multiplied for second
            float appliedVelocity = jumpVelocity;
            if (_jumpsRemaining < maxJumps) // this is a mid-air jump (double, triple...)
            {
                appliedVelocity *= doubleJumpMultiplier;
            }

            // set vertical velocity directly for deterministic jump
            Vector2 v = _rb.linearVelocity;
            v.y = appliedVelocity;
            _rb.linearVelocity = v;

            _anim.SetTrigger("Jump");
            if (!_isGrounded)
            {
                GameObject dust = Instantiate(doubleJumpEffect, groundCheck.position, Quaternion.identity);
                Destroy(dust, .5f);
            }
            _jumpsRemaining--;
        }
    }

    // optional: draw ground check gizmo
    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
