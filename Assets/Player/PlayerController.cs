using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public Transform HeadTransform;

    public float MouseSensitivity = 250f;

    public bool MouseInvertedY = false;

    public float WalkSpeed = 4;

    public float JumpHeight = 2;

    public Texture CrosshairTexture;

    public LayerMask GroundCheckLayer;

    private CharacterController CharacterController;

    private float cameraRotationY;

    private Vector3 velocity;

    public float CharacterHeight
    {
        get
        {
            return CharacterController.height + 2 * CharacterController.skinWidth;
        }
        set
        {
            CharacterController.height = value - 2 * CharacterController.skinWidth;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        CharacterController = GetComponent<CharacterController>();

        CursorHide();
    }

    void Update()
    {
        // Reload current scene
        if (Input.GetKeyUp(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        // Check for standing on the ground and ground the player if needed
        bool isGrounded = IsGrounded();
        if (isGrounded && velocity.y < 0)
        {
            SetGrounded();
        }

        // Fix of shaking when moving towards an obstacle while jumping
        if (isGrounded)
        {
            CharacterController.stepOffset = 0.3f;
        }
        else
        {
            CharacterController.stepOffset = 0f;
        }

        // Get mouse input
        float mouseX = Input.GetAxis("Mouse X") * MouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * MouseSensitivity * Time.deltaTime;

        cameraRotationY += mouseY * (MouseInvertedY ? 1f : -1f);
        cameraRotationY = Mathf.Clamp(cameraRotationY, -90f, 90f);

        // Apply camera transformations
        HeadTransform.localRotation = Quaternion.Euler(cameraRotationY, 0f, 0f);
        transform.Rotate(new Vector3(0f, mouseX, 0f));

        // Get movement input
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        var inputXY = new Vector2(horizontalInput, verticalInput);
        inputXY.Normalize();

        // Apply X-Z movement
        float speed = GetMovementSpeed();
        Vector3 moveDirection = (transform.forward * inputXY.y + transform.right * inputXY.x);
        CharacterController.Move(moveDirection * speed * Time.deltaTime);

        // Apply gravity
        if (!isGrounded)
        {
            velocity += Physics.gravity * Time.deltaTime;
            CharacterController.Move(velocity * Time.deltaTime);
        }
        // Jumping
        else if (Input.GetButtonDown("Jump"))
        {
            isGrounded = false;
            transform.Translate(0f, CharacterController.skinWidth, 0f);
            velocity.y = Mathf.Sqrt(2f * (-Physics.gravity.y) * JumpHeight);
        }
        // Standing on the ground
        else
        {
            velocity.y = 0f;
        }

        if (isGrounded && (Input.GetButton("Crouch") || !CanStandUp()))
        {
            CharacterHeight = 1f;
        }
        else
        {
            CharacterHeight = 2f;
        }

        // Show/hide cursor
        if (Input.GetButton("Cancel"))
        {
            ToggleCursorVisibility();
        }
    }

    private bool CanStandUp()
    {
        return true;
    }

    private float GetMovementSpeed()
    {
        return WalkSpeed;
    }

    private void OnGUI()
    {
        if (CrosshairTexture != null)
        {
            GUI.DrawTexture(
                new Rect(
                    x: Screen.width / 2 - CrosshairTexture.width / 2,
                    y: Screen.height / 2 - CrosshairTexture.height / 2, 
                    width: CrosshairTexture.width, 
                    height: CrosshairTexture.height), 
                CrosshairTexture);
        }
    }

    #region CursorParameters

    public void CursorHide()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public bool ToggleCursorVisibility()
    {
        if (Cursor.visible = !Cursor.visible)
        {
            Cursor.lockState = CursorLockMode.None;
            return true;
        }
        Cursor.lockState = CursorLockMode.Locked;
        return false;
    }

    #endregion

    #region GroundChecks

    private bool IsGrounded()
    {
        return GroundCast().HasValue;
    }

    private void SetGrounded()
    {
        var groundHit = GroundCast();

        if (groundHit.HasValue)
        {
            transform.position += (new Vector3(0f, -groundHit.Value.distance, 0f));
        }
    }

    private RaycastHit? GroundCast()
    {
        CharacterController.enabled = false;

        bool foundGround = Physics.SphereCast(
            origin: transform.position + CharacterController.center + Vector3.up * (-CharacterController.height / 2 + CharacterController.radius),
            radius: CharacterController.radius,
            direction: Vector3.down,
            maxDistance: CharacterController.skinWidth,
            hitInfo: out RaycastHit hitInfo,
            layerMask: 1 << gameObject.layer);
        CharacterController.enabled = true;

        return foundGround ? (RaycastHit?)hitInfo : null;
    }

    #endregion
}
