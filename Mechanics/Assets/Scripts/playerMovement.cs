using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerMovement : MonoBehaviour
{
    [Header("Movement")]
    private float moveSpeed;
    public float WalkSpeed;
    public float SprintSpeed;
   
    public float groundDrag;

    public Transform orientation;
    float horizontalInput;
    float verticalInput;

    [Header("Jumping")]
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    bool readyToJump;
    Vector3 moveDirection;
    Rigidbody rb;

    [Header("Crouching")]
    public float crouchSpeed;
    public float crouchYScale;
    private float startYScale;

    [Header("Slope Handler")]
    public float maxSlopeAngle;
    public RaycastHit slopeHit;
    bool exitingSlope;



    [Header("Keybinds")]
    public KeyCode jumpkey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftControl;


    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatisGround;
    bool grounded;

    public MovementState state;
    public enum MovementState
    {
        walking,
        sprinting,
        crouching,
        air
    }
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        readyToJump = true;
        startYScale = transform.localScale.y;
    }
    void Update()
    {
        grounded = Physics.Raycast(transform.position,Vector3.down, playerHeight * 0.5f+0.3f , whatisGround);
               
        myInput();
        SpeedControl();
        StateHandler();

        if (grounded) 
            rb.drag = groundDrag;
        else
            rb.drag = 0;
    }
    private void FixedUpdate()
    {
        MovePlayer();
    }
    private void myInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxis("Vertical");
        if (Input.GetKey(jumpkey) && readyToJump && grounded)
        {
            readyToJump = false;
            Jump();
            Invoke(nameof(ResetJump),jumpCooldown);
        }
        if (Input.GetKeyDown(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x,crouchYScale,transform.localScale.z);
            rb.AddForce(Vector3.down*5f, ForceMode.Impulse);
        }
        if (Input.GetKeyUp(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);

        }
    }
    private void StateHandler()
    {
        if (Input.GetKey(crouchKey))
        {
            state = MovementState.crouching;
            moveSpeed = crouchSpeed;
        }
        if (grounded && Input.GetKey(sprintKey))
        {
            state = MovementState.sprinting;
            moveSpeed = SprintSpeed;
        }
        else if (grounded)
        {
            state = MovementState.walking;
            moveSpeed = WalkSpeed;
        }
        else
        {
            state = MovementState.air;
        }
    }
    private void MovePlayer()
    {

        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        //On slope
        if (Onslope() && !exitingSlope)
        {
            rb.AddForce(GetSlopeMoveDirection()*moveSpeed*20f, ForceMode.Force);

            if (rb.velocity.y > 0) { 
                rb.AddForce(Vector3.down *80f, ForceMode.Force);}
        }
        //On ground
        if(grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        //On air
        else if(!grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f *airMultiplier, ForceMode.Force);
        // Turn gravity off while on slope
        rb.useGravity = !Onslope();
    }

    private void SpeedControl()
    {
        //Limiting speed on slope
        if (Onslope() && !exitingSlope)
        {
            if (rb.velocity.magnitude > moveSpeed)
            {
                rb.velocity = rb.velocity.normalized * moveSpeed;
            }
        }
        //Limiting speed on ground or air
        else
        {
            Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
            }
        }
       
    }
    private void Jump()
    {
        exitingSlope = true;
        //reset y velocity
        rb.velocity = new Vector3(rb.velocity.x,0f,rb.velocity.z);
        rb.AddForce(transform.up*jumpForce, ForceMode.Impulse);
    }
    private void ResetJump()
    {
        readyToJump = true; 
        exitingSlope = false;

    }

    private bool Onslope()
    {
        if (Physics.Raycast(transform.position,Vector3.down,out slopeHit,playerHeight*0.5f+0.3f))
        {
            float Angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return Angle < maxSlopeAngle && Angle! < 0;
        }
        return false;
    }
    private Vector3 GetSlopeMoveDirection()
    {
        return Vector3.ProjectOnPlane(moveDirection,slopeHit.normal).normalized;
    }
   
}
