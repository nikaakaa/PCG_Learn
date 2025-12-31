using UnityEngine;

/// <summary>
/// 基于 Rigidbody 的玩家移动控制器
/// 使用 Rigidbody.MovePosition 实现物理兼容的移动
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class PlayerMove : MonoBehaviour
{
    [Header("移动设置")]
    [SerializeField] private float movementSpeed = 10f;

    [Header("视角设置")]
    [SerializeField] private float lookSpeed = 2f;
    [SerializeField] private float pitchClamp = 89f;

    // 缓存组件引用
    private Rigidbody rb;

    // 视角旋转状态
    private float pitch;
    private float yaw;

    void Start()
    {
        // 缓存 Rigidbody 引用
        rb = GetComponent<Rigidbody>();

        // 初始化视角角度
        Vector3 currentEuler = transform.rotation.eulerAngles;
        pitch = currentEuler.x;
        yaw = currentEuler.y;

        // 锁定鼠标光标
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        // 处理视角旋转（在 Update 中处理输入更平滑）
        HandleLook();

        // 处理移动（通过 Rigidbody）
        HandleMovement();
    }

    /// <summary>
    /// 处理鼠标视角旋转
    /// </summary>
    private void HandleLook()
    {
        // 获取鼠标输入
        float mouseX = Input.GetAxis("Mouse X") * lookSpeed;
        float mouseY = Input.GetAxis("Mouse Y") * lookSpeed;

        // 更新视角角度
        yaw += mouseX;
        pitch -= mouseY;

        // 限制俯仰角度，防止翻转
        pitch = Mathf.Clamp(pitch, -pitchClamp, pitchClamp);

        // 应用旋转
        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    /// <summary>
    /// 处理玩家移动（使用 Rigidbody）
    /// </summary>
    private void HandleMovement()
    {
        // 获取输入轴
        float horizontal = Input.GetAxis("Horizontal"); // A/D
        float vertical = Input.GetAxis("Vertical");     // W/S

        // 计算上下移动轴
        float upDown = 0f;
        if (Input.GetKey(KeyCode.Space)) upDown += 1f;
        if (Input.GetKey(KeyCode.LeftShift)) upDown -= 1f;

        // 计算移动向量
        Vector3 moveDirection = transform.forward * vertical
                              + transform.right * horizontal
                              + transform.up * upDown;

        // 计算目标位置
        Vector3 targetPosition = rb.position + moveDirection * movementSpeed * Time.deltaTime;

        // 使用 Rigidbody.MovePosition 进行物理移动
        rb.MovePosition(targetPosition);
    }
}
