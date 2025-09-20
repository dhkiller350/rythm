using UnityEngine;
using ProceduralDungeonShooter.Core;

namespace ProceduralDungeonShooter.Player
{
    /// <summary>
    /// First-person player controller with movement and mouse look
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement Settings")]
        public float walkSpeed = 6f;
        public float runSpeed = 12f;
        public float jumpSpeed = 8f;
        public float gravity = 20f;
        public float stickToGroundForce = 10f;
        
        [Header("Mouse Look Settings")]
        public float mouseSensitivity = 2f;
        public float verticalRotationRange = 60f;
        
        [Header("Camera")]
        public Camera playerCamera;
        public float cameraHeight = 1.8f;
        
        // Private variables
        private CharacterController characterController;
        private Vector3 moveDirection = Vector3.zero;
        private bool grounded = false;
        private float verticalRotation = 0f;
        private bool canMove = true;
        
        // Input
        private float horizontal;
        private float vertical;
        private bool isRunning;
        private bool jumpInput;
        
        // Components
        private PlayerHealth playerHealth;
        private PlayerShooting playerShooting;
        
        void Awake()
        {
            characterController = GetComponent<CharacterController>();
            playerHealth = GetComponent<PlayerHealth>();
            playerShooting = GetComponent<PlayerShooting>();
            
            // Set up camera if not assigned
            if (playerCamera == null)
            {
                playerCamera = GetComponentInChildren<Camera>();
            }
            
            // Position camera
            if (playerCamera != null)
            {
                playerCamera.transform.localPosition = new Vector3(0, cameraHeight, 0);
            }
        }
        
        void Start()
        {
            // Lock cursor for FPS controls
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        
        void Update()
        {
            // Only process input if player can move and game is not over
            if (canMove && !GameManager.Instance.gameOver)
            {
                HandleInput();
                HandleMouseLook();
            }
        }
        
        void FixedUpdate()
        {
            if (canMove && !GameManager.Instance.gameOver)
            {
                HandleMovement();
            }
        }
        
        void HandleInput()
        {
            // Movement input
            horizontal = Input.GetAxis("Horizontal");
            vertical = Input.GetAxis("Vertical");
            
            // Running input
            isRunning = Input.GetKey(KeyCode.LeftShift);
            
            // Jump input
            jumpInput = Input.GetButtonDown("Jump");
        }
        
        void HandleMouseLook()
        {
            // Horizontal rotation (Y-axis)
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            transform.Rotate(0, mouseX, 0);
            
            // Vertical rotation (X-axis) - only camera
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
            verticalRotation -= mouseY;
            verticalRotation = Mathf.Clamp(verticalRotation, -verticalRotationRange, verticalRotationRange);
            
            if (playerCamera != null)
            {
                playerCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
            }
        }
        
        void HandleMovement()
        {
            // Check if grounded
            grounded = characterController.isGrounded;
            
            if (grounded)
            {
                // Calculate move direction
                Vector3 forward = transform.TransformDirection(Vector3.forward);
                Vector3 right = transform.TransformDirection(Vector3.right);
                
                // Determine speed based on running
                float currentSpeed = isRunning ? runSpeed : walkSpeed;
                
                // Calculate movement
                moveDirection = (forward * vertical + right * horizontal) * currentSpeed;
                
                // Jumping
                if (jumpInput)
                {
                    moveDirection.y = jumpSpeed;
                }
                else
                {
                    moveDirection.y = -stickToGroundForce;
                }
            }
            else
            {
                // Apply gravity when not grounded
                moveDirection.y -= gravity * Time.fixedDeltaTime;
            }
            
            // Apply movement
            characterController.Move(moveDirection * Time.fixedDeltaTime);
        }
        
        public void InitializePlayer()
        {
            // Reset player state
            canMove = true;
            verticalRotation = 0f;
            moveDirection = Vector3.zero;
            
            // Reset camera rotation
            if (playerCamera != null)
            {
                playerCamera.transform.localRotation = Quaternion.identity;
            }
            
            // Reset player rotation
            transform.rotation = Quaternion.identity;
            
            // Reset health if component exists
            if (playerHealth != null)
            {
                playerHealth.ResetHealth();
            }
            
            Debug.Log("Player initialized");
        }
        
        public void SetCanMove(bool value)
        {
            canMove = value;
        }
        
        public void TeleportTo(Vector3 position)
        {
            characterController.enabled = false;
            transform.position = position;
            characterController.enabled = true;
        }
        
        public bool IsGrounded()
        {
            return grounded;
        }
        
        public bool IsMoving()
        {
            return horizontal != 0 || vertical != 0;
        }
        
        public float GetCurrentSpeed()
        {
            return isRunning ? runSpeed : walkSpeed;
        }
        
        void OnControllerColliderHit(ControllerColliderHit hit)
        {
            // Handle collision events if needed
            // For example, interaction with environment objects
        }
    }
}