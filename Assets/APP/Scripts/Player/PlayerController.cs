using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    const string AXIS_HORIZONTAL = "Horizontal";
    const string AXIS_VERTICAL = "Vertical";

    [Header("References")]
    [SerializeField] Rigidbody2D rb = null;
    [SerializeField] SpriteRenderer spriteRenderer = null;
    [SerializeField] List<Collider2D> playerColliders = null;

    [Header("Player Settings")]
    [SerializeField] float gravityY = -9.81f;
    [SerializeField] float maxSpeed = 10;
    [SerializeField] float maxAcceleration = 10;
    [SerializeField] float jumpHeight = 3;
    [SerializeField] int maxJumpCount = 2;
    [SerializeField] float maxGroundAngleInDegrees = 45f;

    [Header("Trackers - Debug Only")]
    [SerializeField] Vector2 movementAxis = default;
    [SerializeField] Vector2 velocity = default;
    [SerializeField] bool isGrounded = false;
    [SerializeField] bool isJumpDesired = false;
    [SerializeField] float maxGroundDot = 0.414f;
    [SerializeField] int groundContactCount = 0;
    [SerializeField] int jumpsLeft = 0;

    [SerializeField] bool _isInitialized = false;

    private Transform _t = null;


    internal void Initialize()
    {
        _isInitialized = true;

        rb.simulated = true;
        foreach(Collider2D collider in playerColliders)
        {
            collider.enabled = true;
        }

        spriteRenderer.enabled = true;

        jumpsLeft = maxJumpCount;

        rb.linearVelocity = Vector2.up * Mathf.Sqrt(0.6f * gravityY * -2f);

        Debug.Log($"Initialized : {nameof(PlayerController)}");
    }

    internal void DeInitialize()
    {
        _isInitialized = false;

        rb.simulated = false;
        foreach (Collider2D collider in playerColliders)
        {
            collider.enabled = false;
        }

        spriteRenderer.enabled = false;

        Debug.Log($"Deinitialized : {nameof(PlayerController)}");
    }

    void Start()
    {
        _t = this.transform;
        maxGroundDot = Mathf.Cos(maxGroundAngleInDegrees) * Mathf.Deg2Rad;
    }

    private void OnValidate()
    {
        maxGroundDot = Mathf.Cos(maxGroundAngleInDegrees) * Mathf.Deg2Rad;
    }

    void Update()
    {
        if(!_isInitialized) { return; }

        movementAxis = new Vector2(Input.GetAxisRaw(AXIS_HORIZONTAL), Input.GetAxisRaw(AXIS_VERTICAL));
        isJumpDesired |= Input.GetKeyDown(KeyCode.Space);
    }

    private void FixedUpdate()
    {
        if (!_isInitialized) { return; }

        float dt = Time.fixedDeltaTime;

        velocity = rb.linearVelocity;

        //Updating flags
        isGrounded = groundContactCount > 0;

        if(isGrounded)
        {
            jumpsLeft = maxJumpCount;
        }

        //Horizontal movement
        Vector2 horizontalVelocity = Vector2.zero;
        horizontalVelocity.x = (movementAxis.x * maxSpeed * dt);
        horizontalVelocity.x = Mathf.Clamp(horizontalVelocity.x * (maxAcceleration * dt), -maxSpeed, maxSpeed);

        Vector2 verticalVelocity = new Vector2(0f, velocity.y);

        if (isJumpDesired && (jumpsLeft > 0))
        {
            verticalVelocity = Vector2.up * Mathf.Sqrt(jumpHeight * gravityY * -2f);
            jumpsLeft -= 1;
        }

        if (isGrounded && verticalVelocity.y < 0f)
        {
            verticalVelocity = Vector2.down * 0.9f;
        }
        else
        {
            //Apply gravity
            verticalVelocity.y += gravityY * dt;
        }

        velocity = horizontalVelocity + verticalVelocity;

        rb.linearVelocity = velocity;

        //Reset trackers
        isJumpDesired = false;
        groundContactCount = 0;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        EvaluateCollision(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        EvaluateCollision(collision);
    }

    private void EvaluateCollision(Collision2D collision)
    {
        if (!_isInitialized) { return; }

        for (int i = 0; i < collision.contactCount; i++)
        {
            ContactPoint2D contact = collision.GetContact(i);

            if (Vector2.Dot(Vector2.up, contact.normal) >= maxGroundDot)
            {
                groundContactCount += 1;
            }
        }
    }
}
