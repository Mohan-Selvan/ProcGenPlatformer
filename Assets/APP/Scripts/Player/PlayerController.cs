using UnityEngine;

public class PlayerController : MonoBehaviour
{
    const string AXIS_HORIZONTAL = "Horizontal";
    const string AXIS_VERTICAL = "Vertical";

    [Header("References")]
    [SerializeField] Rigidbody2D rb = null;
    [SerializeField] Collider2D playerCollider = null;

    [Header("Player Settings")]
    [SerializeField] float gravityY = -9.81f;
    [SerializeField] float maxSpeed = 10;
    [SerializeField] float maxAcceleration = 10;
    [SerializeField] float jumpHeight = 3;
    [SerializeField] float maxGroundAngleInDegrees = 45f;
    
    [Header("Trackers - Debug Only")]
    [SerializeField] Vector2 movementAxis = default;
    [SerializeField] Vector2 velocity = default;
    [SerializeField] bool isGrounded = false;
    [SerializeField] bool isJumpDesired = false;
    [SerializeField] float maxGroundDot = 0.414f;
    [SerializeField] int groundContactCount = 0;

    private Transform _t = null;

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
        movementAxis = new Vector2(Input.GetAxisRaw(AXIS_HORIZONTAL), Input.GetAxisRaw(AXIS_VERTICAL));
        isJumpDesired |= Input.GetKeyDown(KeyCode.Space);
    }

    private void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;

        velocity = rb.linearVelocity;

        //Updating flags
        isGrounded = groundContactCount > 0;

        //Horizontal movement
        Vector2 horizontalVelocity = Vector2.zero;
        horizontalVelocity.x = (movementAxis.x * maxSpeed * dt);
        horizontalVelocity.x = Mathf.Clamp(horizontalVelocity.x * (maxAcceleration * dt), -maxSpeed, maxSpeed);

        Vector2 verticalVelocity = new Vector2(0f, velocity.y);

        if (isJumpDesired)
        {
            verticalVelocity += Vector2.up * Mathf.Sqrt(jumpHeight * gravityY * -2f);
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
