using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class MovementController : MonoBehaviour
{
    public static MovementController Instance;
    [Header("Restraints")]
    public bool moveLocked;
    public bool lookLocked;
    
    [Header("Components")]
    private GameObject cam;
    [SerializeField] private CapsuleCollider hitbox;
    [SerializeField] private Vector3 cameraOffset;
    [SerializeField] private float mouseSensitivity = 2.5f;
    [SerializeField] private float clampAngle = 80.0f;
    
    [Header("Extra Gravity")]
    [SerializeField] private float aerialGravity = 20f;
    
    [Header("Walking")]
    [SerializeField] private float walkSpeed = 15;
    [SerializeField] private float airSpeed = 10;
    [SerializeField] private float acceleration = 60;
    [SerializeField] private float jumpHeight = 15;
    [SerializeField] private float stoppingDrag = 5;
    [SerializeField] private float airDrag = 2;
    [SerializeField] private float maxSlopeGroundAngle = 50;
    
    [Header("wallrunning")]
    [SerializeField] private float wallrunUpSpeed = 9.81f;
    [SerializeField] private float wallrunDownSpeed  = -9.81f;
    [SerializeField] private float wallrunSlowDownSpeed = -2;
    [SerializeField] private float wallrunMinimumHorizontalSpeed = 2f;
    
    [Header("Dash")]
    [SerializeField] private float dashSpeed = 50;
    [SerializeField] private float dashTime = 0.25f;
    [SerializeField] private int dashCount = 1;
    [SerializeField] private float dashChargeTime = 0.25f;
    
    //Hidden, dash related things.
    private float remainingDashTime;
    private int remainingDashes;
    private float remainingDashChargeTime;
    private Vector3 dashDirection;


    [Header("Slamming")]
    [SerializeField] private float minFloorDistance;
    [SerializeField] private float slamForce;
    [SerializeField] private float slamRechargeDelay;
    //Prevents crouching and jumping at the same time leading to jitters
    private bool crouchConsumedByJump;

    [Header("Sliding")]
    [SerializeField] private float slideDrag = 1f;
    [SerializeField] private float slideHeight = 1f;
    [SerializeField] private float slideCamHeightOffset = -1f;
    [SerializeField] private float slideCameraLerpSpeed = 12f;
    [SerializeField] private float slopeMultiplier;
    private float standingBottomY;
    private float currentCameraYOffset;
    private float targetCameraYOffset;
    private bool slideStarted;
    private float standardHeight;
    
    
    [Header("Ground Check")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private Vector3 groundCheckOffset;
    [SerializeField] private Vector3 groundCheckSize;
    [SerializeField] private float wallrunOffset;
    [SerializeField] private float wallrunCheckHeight, wallrunCheckThickness;

    //This should have been a state machine. However fuck you.
    [Header("States")]
    public bool grounded;
    public bool wallrunning;
    public bool slamming;
    public bool sliding;
    public bool dashing;
    private bool dashActivated;
    
    //Private variables used for a lot of things
    private Rigidbody rb;
    private PlayerInputActions input;
    
    //Mouse rotations
    private float rotY;
    private float rotX;
    
    //Storing inputs
    private Vector2 moveInput;
    private Vector3 horizontalMovement;
    private Vector2 lookInput;

    //Allows slopes to send the player flying
    private bool wasGrounded;
    private bool justLeftGround;
    private float lastGroundedYVelocity;
    
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        standardHeight = hitbox.height;
        Instance = this;
        input = new PlayerInputActions();
        standingBottomY = hitbox.center.y - (standardHeight / 2f);
    }
    
    void Start()
    {
        //Always turn camera forward
        Vector3 rot = transform.localRotation.eulerAngles;
        rotY = rot.y;
        rotX = rot.x;

        //Apply camera offset but unparent for easier management of other player components (Mostly the sliding scale etc being instant)
        cam = Camera.main.gameObject;
        cam.transform.SetParent(null, true);

        //Lock Cursor at the start.
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        UpdateCameraPosition();
        if (PlayerPrefs.HasKey("MouseSensitivity")) mouseSensitivity = PlayerPrefs.GetFloat("MouseSensitivity");

        SceneTransitionManager.OnSceneLoaded += () =>
        {
            Transform spawnTransform = GameObject.FindWithTag("PlayerSpawn").transform;
            transform.SetPositionAndRotation(spawnTransform.position, spawnTransform.rotation);
        };
    }
    
    private void UpdateCameraPosition()
    {
        //Ensures Camera is stuck to the player at the desired position.
        currentCameraYOffset = Mathf.Lerp(
            currentCameraYOffset,
            targetCameraYOffset,
            slideCameraLerpSpeed * Time.deltaTime
        );
        Vector3 baseOffset = cameraOffset;
        baseOffset.y += currentCameraYOffset;

        Vector3 rotatedOffset = transform.rotation * baseOffset;

        cam.transform.position = transform.position + rotatedOffset;
        cam.transform.rotation = Quaternion.Euler(rotX, rotY, 0f);
    }

    void Update()
    {
        grounded = GroundCheck();
        wallrunning = WallRunCheck();
        moveInput = input.Player.Move.ReadValue<Vector2>();
        horizontalMovement = new Vector3(moveInput.x, 0, moveInput.y).normalized;

        if (!lookLocked) MouseLook();

        //stick to ground
        if (horizontalMovement.magnitude < 0.1f && grounded && !sliding)
        {
            //If you arent moving just... stay where you are, plis
            if (rb.linearVelocity.y < jumpHeight / 2) rb.linearVelocity = new Vector3(0, 0, 0);
            rb.useGravity = false;
        }
        else
        {
            rb.useGravity = true;
        }

        //Activate a jump
        if (input.Player.Jump.triggered && grounded && !moveLocked)
        {
            Jump();
            crouchConsumedByJump = true;
            sliding = false;
        }

        if (!moveLocked)
        {
            if (input.Player.Dash.triggered && !dashing && remainingDashes > 0 && horizontalMovement != Vector3.zero)
            {
                remainingDashes--;
                dashing = true;
                dashActivated = true;
                remainingDashTime = dashTime;
                dashDirection = transform.forward;
            }

            //Enable slide after jump when you let go to prevent jitter
            if (!input.Player.Crouch.IsPressed())
            {
                crouchConsumedByJump = false;
            }

            //Slide
            if (grounded && input.Player.Crouch.triggered && !crouchConsumedByJump)
            {
                sliding = true;
            }
            else if (!input.Player.Crouch.inProgress)
            {
                sliding = false;
            }

            //Activate slam if youre in the air and try to slide
            if (!slamming && !grounded && !wallrunning && input.Player.Crouch.triggered && !crouchConsumedByJump && !Physics.Raycast(transform.position, -transform.up, minFloorDistance, groundLayer))
            {
                slamming = true;
                rb.linearVelocity = Vector3.down * slamForce;
            }
        }

        //Handle slide heights
        if (sliding)
        {
            hitbox.height = slideHeight;

            float newCenterY = standingBottomY + (slideHeight / 2f);
            hitbox.center = new Vector3(0f, newCenterY, 0f);

            targetCameraYOffset = slideCamHeightOffset;
        }
        else
        {
            hitbox.height = standardHeight;
            hitbox.center = new Vector3(0f, 0f, 0f);

            targetCameraYOffset = 0f;
        }
    }

    public void ResetVelocity()
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;
    }

    public void KinematicOff()
    {
        rb.isKinematic = false;
    }

    void FixedUpdate()
    {
        if (!moveLocked)
        {
            Movement();
        }
        
        //Handle dashing
        if (dashing && remainingDashTime > 0)
        {
            remainingDashTime -= Time.fixedDeltaTime;
        } else if (dashing)
        {
            Vector3 horizontal = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            float currentSpeed = horizontal.magnitude;
            float newSpeed = !sliding ? Mathf.Max(0f, currentSpeed - dashSpeed) : Mathf.Max(0f, currentSpeed - dashSpeed / 2);
            Vector3 newHorizontal = horizontal.normalized * newSpeed;
            rb.linearVelocity = new Vector3(newHorizontal.x, rb.linearVelocity.y, newHorizontal.z);
            remainingDashChargeTime = dashChargeTime;
            dashing = false;
        } else if (remainingDashes < dashCount)
        {
            if (remainingDashChargeTime > 0) remainingDashChargeTime -= Time.fixedDeltaTime;
            else if(grounded)
            {
                remainingDashChargeTime = dashChargeTime;
                remainingDashes++;
            }
        }
    }

    void LateUpdate()
    {
        //Before rendering frame, makes sure camera is in the right spot. Could probably be in update but its "cleaner" to keep it here where theres no other clutter
        if (cam == null) return;
        UpdateCameraPosition();
    }
    
    
    //Checks if you are next to a wall you can run up. Could be changed to only allow if the wall is to the sides, but this felt better to me
    private bool WallRunCheck()
    {
        if (moveInput.y <= 0.25 && !wallrunning) return false;
        Vector3 center = transform.position;
        Collider[] hits = Physics.OverlapCapsule(center + transform.up * wallrunCheckHeight/2, center + -transform.up * wallrunCheckHeight/2, wallrunCheckThickness);
            
        foreach (var hit in hits)
        {
            if(0 != (wallLayer & (1 << hit.gameObject.layer)))
            {
                return true;
            }
        }

        return false;
    }
    
    //Esoteric ground check using a pair of box colliders to avoid issues with the player easily falling because raycast is at the center / not thick enough.
    private bool GroundCheck()
    {
        Vector3 center = transform.position + groundCheckOffset;
        Quaternion rotation = Quaternion.Euler(0, 45f, 0);

        List<Collider> hits = new List<Collider>();
        hits.AddRange(Physics.OverlapBox(center, groundCheckSize / 2));
        hits.AddRange(Physics.OverlapBox(center, groundCheckSize / 2, rotation));
        
        RaycastHit rayHit;
        bool hasGroundNormal = Physics.Raycast(transform.position, Vector3.down, out rayHit, 2f, groundLayer);

        foreach (var hit in hits)
        {
            if (0 != (groundLayer & (1 << hit.gameObject.layer)))
            {
                //If surface is too large a slope, wont count it as ground. Helps make slams into steep slopes more dynamic
                if (hasGroundNormal)
                {
                    float angle = Vector3.Angle(rayHit.normal, Vector3.up);
                    if (angle > maxSlopeGroundAngle)
                    {
                        return false;
                    }
                }
                grounded = true;
    
                // If you are slamming and you hit the ground, transitions velocity toward the direction of the normals.
                if (slamming && hasGroundNormal)
                {
                    slamming = false;
                    sliding = true;
                    
                    Vector3 projected = Vector3.ProjectOnPlane(rb.linearVelocity, rayHit.normal);
                    rb.linearVelocity = new Vector3(projected.x, rb.linearVelocity.y, projected.z);
                }
                if (!hasGroundNormal) return false;
                return true;
            }
        }

        return false;
    }

    private void Jump()
    {
        rb.AddForce(Vector3.up * jumpHeight, ForceMode.VelocityChange);
    }

    //Mouse look. could be a separate script but it wasnt in the original so i felt no need to make it one here.
    private void MouseLook()
    {
        lookInput = input.Player.Look.ReadValue<Vector2>();

        float mouseX = lookInput.x;
        float mouseY = -lookInput.y;

        rotY += mouseX * mouseSensitivity;
        rotX += mouseY * mouseSensitivity;

        rotX = Mathf.Clamp(rotX, -clampAngle, clampAngle);

        transform.rotation = Quaternion.Euler(0f, rotY, 0f);
    }

    //Spaghetti
    private void Movement()
    {
        if (slamming) return;
        
        Vector3 velocity = rb.linearVelocity;
        Vector3 horizontalVelocity = new Vector3(velocity.x, 0, velocity.z);

        Vector3 moveDirection = transform.forward * horizontalMovement.z + transform.right * horizontalMovement.x;
        
        // Dashing
        if (dashing && dashActivated)
        {
            Vector3 targetVelocity = dashDirection * dashSpeed;
            rb.linearVelocity += targetVelocity;
            dashActivated = false;
            return;
        }
        
        if ((!wallrunning || grounded) && !sliding)
        {
            //Normal movement
            if (horizontalMovement.magnitude > 0.1f && grounded && !dashing)
            {
                //Get normal and project movement along it.
                Vector3 groundNormal = Vector3.up;
                if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 2f, groundLayer))
                {
                    groundNormal = hit.normal;
                }
                float slopeAngle = Vector3.Angle(groundNormal, Vector3.up);
                Vector3 desiredDir = Vector3.ProjectOnPlane(moveDirection.normalized, groundNormal).normalized;
                
                //Project eeeeverything along the shit, so that you properly stay stuck to it
                Vector3 currentVelocity = rb.linearVelocity;
                Vector3 velocityIntoGround = Vector3.Project(currentVelocity, groundNormal);
       
                Vector3 newVelocity = desiredDir * walkSpeed;
                
                rb.linearVelocity = new Vector3(
                    newVelocity.x,
                    rb.linearVelocity.y,
                    newVelocity.z
                );
                
                Vector3 slopedGravity = Vector3.ProjectOnPlane(Physics.gravity, groundNormal) * Time.fixedDeltaTime;
                if (slopeAngle > 5f)
                {
                    slopedGravity *= 0.25f;
                }
                rb.linearVelocity = newVelocity + velocityIntoGround + slopedGravity;
                
            } // Air movement (could maybe be combined but if it aint broken dont fix it. at least during a game jam)
            else if (horizontalMovement != Vector3.zero)
            {
                Vector3 desiredDir = moveDirection.normalized;

                if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 2f, groundLayer))
                {
                    desiredDir = Vector3.ProjectOnPlane(desiredDir, hit.normal).normalized;
                }
                float currentSpeed = horizontalVelocity.magnitude;
                float targetSpeed = currentSpeed > airSpeed ? currentSpeed : airSpeed;
                Vector3 newVelocity = desiredDir * targetSpeed;
                newVelocity.y = rb.linearVelocity.y;
                rb.linearVelocity = newVelocity;
            }
    
            //Extra gravity when in the air but not slamming etc
            if (!grounded && !slamming && !dashing)
            {
                rb.AddForce(Vector3.down * aerialGravity, ForceMode.Acceleration);
            }

            //Manual drag since we want to alter it based on your current mode of movement. Could be set to the rigid body but this felt better at the time.
            if (grounded && horizontalMovement == Vector3.zero)
            {
                Vector3 stoppingForce = -horizontalVelocity * stoppingDrag;
                rb.AddForce(stoppingForce, ForceMode.Acceleration);
            }
            else if (!grounded && horizontalMovement == Vector3.zero)
            {
                Vector3 stoppingForce = -horizontalVelocity * airDrag;
                rb.AddForce(stoppingForce, ForceMode.Acceleration);
            }
        }// Sliding
        else if (sliding && grounded)
        {
            if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 2f, groundLayer))
            {
                Vector3 groundNormal = hit.normal;
                
                Vector3 slopeVelocity = Vector3.ProjectOnPlane(rb.linearVelocity, groundNormal);
                // only flatten if moving INTO the ground, not away from it
                if (Vector3.Dot(rb.linearVelocity, groundNormal) < 0f)
                {
                    rb.linearVelocity = slopeVelocity;
                }
                Vector3 slopeDir = Vector3.ProjectOnPlane(Vector3.down, groundNormal).normalized;
                
                //Apply boost only when going down
                if (rb.linearVelocity.y <= 0)
                    rb.AddForce(slopeDir * slopeMultiplier, ForceMode.Acceleration);
              
                rb.AddForce(-slopeVelocity * slideDrag, ForceMode.Acceleration);
            }
        }//Wallrunning
        else if (wallrunning && horizontalMovement != Vector3.zero)
        {
            float movementSpeed =  walkSpeed;
            Vector3 targetVelocity = moveDirection * movementSpeed;
            horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);

            Vector2 horizontalSpeed = new Vector2(rb.linearVelocity.x, rb.linearVelocity.z);
            if (moveInput.y <= 0.25)
            {
                //Slide down as if falling if you dont press forward
                rb.linearVelocity = new Vector3(horizontalVelocity.x, Mathf.Clamp(rb.linearVelocity.y, rb.linearVelocity.y, wallrunDownSpeed), horizontalVelocity.z);
            } else if (input.Player.Crouch.inProgress)
            {
                //Going down
                rb.linearVelocity = new Vector3(horizontalVelocity.x,  wallrunDownSpeed, horizontalVelocity.z);
            } else if (input.Player.Jump.inProgress)
            {
                //Going up
                rb.linearVelocity = new Vector3(horizontalVelocity.x, Mathf.Clamp(rb.linearVelocity.y, wallrunUpSpeed, rb.linearVelocity.y), horizontalVelocity.z);
            } else if (horizontalSpeed.magnitude < wallrunMinimumHorizontalSpeed)
            {
                //Slide down if too slow, and not intentionally going up or dowwn
                rb.linearVelocity = new Vector3(horizontalVelocity.x, wallrunSlowDownSpeed, horizontalVelocity.z);
            }
            else
            {
                //Retain vertical position
                rb.linearVelocity = new Vector3(horizontalVelocity.x, Mathf.Clamp(rb.linearVelocity.y, 0, rb.linearVelocity.y), horizontalVelocity.z);
            }
        }
    }
    
    //not sure if needed, but there they are.
    private void OnEnable()
    {
        input.Enable();
    }

    private void OnDisable()
    {
        input.Disable();
    }

    // Magic colors and fancy things to make me see the invisible things from my nightmares.
    private void OnDrawGizmosSelected()
    {
        //Wallrun Check
        #if UNITY_EDITOR
        Color wallColor = wallrunning ? Color.red : Color.green;
        Handles.color = wallColor;
        Vector3 center = transform.position;

        Vector3 point1 = transform.position + transform.up * wallrunCheckHeight/2;
        Vector3 point2 = transform.position - transform.up * wallrunCheckHeight/2;
        DrawCapsule(point1, point2, wallrunCheckThickness);
        #endif
        
        //Min Slam
        Gizmos.color = Physics.Raycast(transform.position, -transform.up, minFloorDistance, groundLayer) ? Color.red : Color.green;
        Gizmos.DrawLine(transform.position, transform.position + -transform.up * minFloorDistance);
        
        // Ground Check
        Gizmos.color = grounded ? Color.red : Color.green;

        center = transform.position + groundCheckOffset;
        
        Gizmos.matrix = Matrix4x4.identity;
        Gizmos.DrawWireCube(center, groundCheckSize);
        Quaternion rotation = Quaternion.Euler(0f, 45f, 0f);
        Gizmos.matrix = Matrix4x4.TRS(center, rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, groundCheckSize);
        Gizmos.matrix = Matrix4x4.identity;
    }
    
    #if UNITY_EDITOR
        private void DrawCapsule(Vector3 point1, Vector3 point2, float radius)
        {
            Vector3 direction = (point2 - point1).normalized;
            
            Handles.DrawWireDisc(point1, direction, radius);
            Handles.DrawWireDisc(point2, direction, radius);
            
            Vector3 right = Vector3.Cross(direction, Vector3.up);
            if (right.sqrMagnitude < 0.001f)
                right = Vector3.Cross(direction, Vector3.forward);

            right.Normalize();
            Vector3 forward = Vector3.Cross(direction, right);
            
            Handles.DrawLine(point1 + right * radius, point2 + right * radius);
            Handles.DrawLine(point1 - right * radius, point2 - right * radius);

            Handles.DrawLine(point1 + forward * radius, point2 + forward * radius);
            Handles.DrawLine(point1 - forward * radius, point2 - forward * radius);
            
            DrawWireSphere(point1, radius);
            DrawWireSphere(point2, radius);
        }
        
        void DrawWireSphere(Vector3 center, float radius)
        {
            Handles.DrawWireArc(center, Vector3.right, Vector3.up, 360f, radius);
            Handles.DrawWireArc(center, Vector3.up, Vector3.forward, 360f, radius);
            Handles.DrawWireArc(center, Vector3.forward, Vector3.up, 360f, radius);
        }
    #endif
}
