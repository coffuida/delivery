using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement & Look")]
    public float moveSpeed = 10f;
    public float sprintSpeed = 15f;
    public float jumpHeight = 2f;
    public float gravity = -15f;
    public Camera playerCamera;
    public float mouseSensitivity = 15f;

    [Header("Bound State")]
    public bool isBound = false;

    private CharacterController controller;
    private Animator animator;
    private Vector3 velocity;
    private float xRotation = 0f;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // 속박 중에는 이동만 불가, 카메라 조작은 가능
        if (!isBound)
        {
            HandleMovement();
        }

        // 카메라는 항상 조작 가능
        HandleLook();
    }

    void HandleMovement()
    {
        bool isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
            animator.SetBool("isJumping", false);
        }

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        bool isMoving = (x != 0 || z != 0);
        bool isSprinting = isMoving && Input.GetKey(KeyCode.LeftShift);

        float currentSpeed = isSprinting ? sprintSpeed : moveSpeed;

        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * currentSpeed * Time.deltaTime);

        animator.SetBool("isMoving", isMoving);
        animator.SetBool("isSprinting", isSprinting);

        float horizontalVelocity = new Vector3(controller.velocity.x, 0, controller.velocity.z).magnitude;
        animator.SetFloat("speed", horizontalVelocity);

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            animator.SetBool("isJumping", true);
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void HandleLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    /// <summary>
    /// 속박 상태 설정 (세라핌에서 호출)
    /// </summary>
    public void SetBound(bool bound)
    {
        isBound = bound;

        Debug.Log($"[플레이어] 속박: {(bound ? "ON (이동 불가, 카메라 조작 가능)" : "OFF")}");
    }
}
