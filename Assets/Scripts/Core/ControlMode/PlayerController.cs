using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// 负责当前的切换控制器
public class PlayerController : ControllerBase
{
    #region 外部引用
    [Header("玩家数值")]

    [Tooltip("玩家")][SerializeField] private GameObject playerObj;
    [SerializeField] private float moveSpeed = 10;

    [Tooltip("玩家相机")]  public Transform MainCamera;
    #endregion

    #region 按键教学
    bool isOpen = false; // 弹窗是否打开
    private bool _wasPrimaryButtonDown = false; // 用于跟踪上一帧按钮状态
    #endregion

    #region 个人移动参数
    [SerializeField]
    [Tooltip("玩家旋转速度")]
    private float selfSpinSpeed = 50f; // 新增自转速度
    #endregion

    public void HandleLevelMove(Vector2 levelDir, float deltaTime)
    {
        // 获取相机当前的前方向和右方向
        Vector3 forward = MainCamera.forward;
        Vector3 right = MainCamera.right;

        // 组合移动方向（前/后 + 左/右）
        Vector3 moveDirection = (forward * levelDir.y) + (right * levelDir.x);
        moveDirection.y = 0; // 确保仅在水平面移动

        // 避免斜向移动速度加快
        if (moveDirection.magnitude > 1)
        {
            moveDirection.Normalize();
        }

        // 应用移动
        playerObj.transform.Translate(moveDirection * moveSpeed * deltaTime, Space.World);
    }
    private void HandleSpin(float levelDir, float deltaTime)
    {
        playerObj.transform.Rotate(0, levelDir * selfSpinSpeed * deltaTime, 0, Space.Self);
    }

    protected override void HandleInput()
    {
        var leftDeviceState = ControllerManager.Instance.leftDeviceState;
        var rightDeviceState = ControllerManager.Instance.rightDeviceState;
        HandleLevelMove(leftDeviceState.primaryAxis2DValue, Time.deltaTime);
        HandleSpin(rightDeviceState.primaryAxis2DValue.x, Time.deltaTime);
        // 按住 A 切换打开和关闭处理打开教学界面
        // 获取当前帧的primaryButtonValue
        bool currentPrimaryButtonValue = rightDeviceState.primaryButtonValue;

        // 检测按钮是否在这一帧被按下 (从false变为true)
        if (currentPrimaryButtonValue && !_wasPrimaryButtonDown) //
        {
            if (isOpen = !isOpen) //
            {
                UIManager.Instance.OpenScreenPanel("PlayerTutorialPanel");
            }
            else
            {
                UIManager.Instance.CloseScreenPanel("PlayerTutorialPanel");
            }
        }

        // 更新_wasPrimaryButtonDown为当前帧的按钮状态，供下一帧使用
        _wasPrimaryButtonDown = currentPrimaryButtonValue;
    }

    protected override void LoadInput()
    {
    }

}
