using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed = 5f;
    public float jumpHeight = 2.5f;
    public float gravity = -20f;
    public float mouseSensitivity = 100f;

    public Transform headTransform;

    private CharacterController controller;
    private Animator animator;

    private Vector3 velocity;
    private bool isGrounded;
    private float xRotation = 0f;

    private Vector3 currentMove;
    private Vector3 velocitySmooth;
    private float smoothTime = 0.1f;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // 마우스 입력
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Player는 Y축 회전
        transform.Rotate(Vector3.up * mouseX);

        // Head는 X축 회전만
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        headTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // 바닥 체크
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;

        // WASD 입력
        float h = Input.GetAxisRaw("Horizontal"); // A/D → -1/1
        float v = Input.GetAxisRaw("Vertical");   // W/S → 1/-1

        // 방향 벡터 계산
        Vector3 forward = Vector3.ProjectOnPlane(Camera.main.transform.forward, Vector3.up).normalized;
        Vector3 right = Vector3.ProjectOnPlane(Camera.main.transform.right, Vector3.up).normalized;

        Vector3 move = forward * v + right * h;

        // headbob구현 : 생동감 살리기
        float bobSpeed = 10f;
        float bobAmount = 0.05f;
        float bobTimer = 0f;

        if (move.magnitude > 0.1f && isGrounded)
        {
            bobTimer += Time.deltaTime * bobSpeed;
            float bobOffset = Mathf.Sin(bobTimer) * bobAmount;
            headTransform.localPosition = new Vector3(0f, 1.5f + bobOffset, 0f);
        }
        else
        {
            bobTimer = 0f;
            headTransform.localPosition = new Vector3(0f, 1.5f, 0f);
        }

        // 이동
        Vector3 targetMove = (headTransform.forward * v + headTransform.right * h).normalized; //+
        currentMove = Vector3.SmoothDamp(currentMove, targetMove, ref velocitySmooth, smoothTime); //+

        controller.Move(move * speed * Time.deltaTime);

        // 애니메이션
        float moveSpeed = new Vector2(h, v).magnitude;
        animator?.SetFloat("Speed", moveSpeed);

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            animator?.SetBool("IsJumping", true);
        }

        if (isGrounded)
            animator?.SetBool("IsJumping", false);

        // 중력
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}
