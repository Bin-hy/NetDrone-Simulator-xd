using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;


// 负责当前的切换控制器
public class DroneController : ControllerBase
{
    #region 数值定义
    [SerializeField]
    [Tooltip("是否旋转桨叶")]
    public bool isSpin = false;
    [SerializeField]
    [Tooltip("")]
    private Vector3 spinDir;
    [SerializeField]
    [Tooltip("桨叶Transform数组")]
    private Transform[] paddleRotations;

    [SerializeField]
    [Tooltip("桨叶转速")]
    private float paddleSpinSpeed = 1000f; // 更改变量名，避免与移动速度混淆

    [SerializeField]
    [Tooltip("桨叶旋转方向(1=顺时针, -1=逆时针)")]
    private int paddleSpinDirection = 1;

    [SerializeField]
    [Tooltip("无人机平移速度")]
    private float moveSpeed = 10f; // 更改为float类型，更符合速度的定义

    [SerializeField]
    [Tooltip("无人机垂直移动速度")]
    private float verticalSpeed = 5f; // 新增垂直移动速度

    [SerializeField]
    [Tooltip("无人机自转速度")]
    private float selfSpinSpeed = 50f; // 新增自转速度
    #endregion
    
    #region 按键教学
    bool isOpen = false; // 弹窗是否打开
    private bool _wasPrimaryButtonDown = false; // 用于跟踪上一帧按钮状态
    #endregion

    //无人机名称
    [SerializeField] private string _droneName;
    public string droneName => _droneName;
    [SerializeField] private Camera _droneCamera;
    // 相机渲染目标
    private RenderTexture _cameraRenderTexture;
    public RenderTexture cameraRenderTexture
    {
        get { return _cameraRenderTexture; }
    }

    protected override void Update()
    {
        handlePaddleSpin(); // 处理自我旋转
        base.Update();
    }

    protected override void HandleInput()
    {
        var leftDeviceState = ControllerManager.Instance.leftDeviceState;
        var rightDeviceState = ControllerManager.Instance.rightDeviceState;

        HandleLevelMove(leftDeviceState.primaryAxis2DValue, Time.deltaTime);
        HandleVerticalMove(rightDeviceState.primaryAxis2DValue.y, Time.deltaTime);
        HandleSelfSpin(rightDeviceState.primaryAxis2DValue.x, Time.deltaTime);

        // 单独按住 左手的 X： 切换到用户控制
        if (leftDeviceState.primaryButtonValue)
        {
            ControllerManager.Instance.SwitchController(ControlMode.PlayerControl); // 切换到玩家
        }

        // 按住 A 切换打开和关闭处理打开教学界面
        // 获取当前帧的primaryButtonValue
        bool currentPrimaryButtonValue = rightDeviceState.primaryButtonValue;

        // 检测按钮是否在这一帧被按下 (从false变为true)
        if (currentPrimaryButtonValue && !_wasPrimaryButtonDown) //
        {
            if (isOpen = !isOpen) //
            {
                UIManager.Instance.OpenScreenPanel("DroneTutorialPanel");
            }
            else
            {
                UIManager.Instance.CloseScreenPanel("DroneTutorialPanel");
            }
        }

        // 更新_wasPrimaryButtonDown为当前帧的按钮状态，供下一帧使用
        _wasPrimaryButtonDown = currentPrimaryButtonValue;



    }

    protected override void LoadInput()
    {
    }


    private string hashMapKey; // 获取 Map中无人机 的Key

    // 处理桨叶旋转逻辑
    void handlePaddleSpin()
    {
        // 桨叶旋转逻辑
        if (isSpin)
        {
            if (paddleRotations == null || paddleRotations.Length <= 0)
            {
                Debug.LogWarning("DroneController: Your script 'DroneController' without initial the rotations of the paddles.没有加载 桨叶数据。");
                return;
            }

            foreach (var rot in paddleRotations)
            {
                if (rot != null)
                {
                    HandlePaddleSpin(rot);
                }
            }
        }
        /// <summary>
        /// 处理桨叶旋转
        /// </summary>
        /// <param name="rot">桨叶的Transform组件</param>
        void HandlePaddleSpin(Transform rot)
        {
            
            float rotationAmount = paddleSpinSpeed * paddleSpinDirection * Time.deltaTime;
            rot.Rotate(0, rotationAmount, 0, Space.Self);
        }
    }

    /// <summary>
    /// 射线特效
    /// </summary>
    /// <param name="fromPoint"></param>
    /// <param name="toPoint"></param>
    /// <param name="duration">duration =0 表示一直发送</param>
    public void LaserEffect(Transform fromPoint, Transform toPoint, float duration = 0)
    {

    }

    #region 移动控制
    /// <summary>
    /// 控制无人机在水平面上的移动（前后左右）
    /// </summary>
    /// <param name="levelDir">水平方向向量 (x: 左右, y: 前后)</param>
    /// <param name="deltaTime">时间增量</param>
    private void HandleLevelMove(Vector2 levelDir, float deltaTime)
    {
        // 将二维输入转换为三维移动向量
        // levelDir.y 对应前后 (Z轴)，levelDir.x 对应左右 (X轴)
        Vector3 moveDirection = new Vector3(levelDir.x, 0, levelDir.y);

        // 使用 Translate 方法进行移动，Space.Self 使得移动是相对于无人机自身的方向
        transform.Translate(moveDirection * moveSpeed * deltaTime, Space.Self);
    }

    /// <summary>
    /// 控制无人机垂直移动（上下）
    /// </summary>
    /// <param name="dir">垂直方向 (1: 上升, -1: 下降)</param>
    /// <param name="deltaTime">时间增量</param>
    private void HandleVerticalMove(float dir, float deltaTime)
    {
        // 垂直移动是沿着无人机自身的Y轴
        transform.Translate(Vector3.up * dir * verticalSpeed * deltaTime, Space.Self);
    }

    /// <summary>
    /// 控制无人机自转（绕自身Y轴旋转）
    /// </summary>
    /// <param name="dir">旋转方向 (1: 顺时针, -1: 逆时针)</param>
    /// <param name="deltaTime">时间增量</param>
    private void HandleSelfSpin(float dir, float deltaTime)
    {
        // 自转是绕无人机自身的Y轴旋转
        transform.Rotate(0, dir * selfSpinSpeed * deltaTime, 0, Space.Self);
    }
    #endregion

    public void hoverButtonOnSelectSelf()
    {
        // 点击选中控制自己
        DroneManager.Instance.selectedDroneController = this;
        ControllerManager.Instance.SwitchController(ControlMode.PlayerControl);
        ControllerManager.Instance.SwitchController(ControlMode.DroneControl);
    }
    public void ButtonOnSelectSelf(SelectEnterEventArgs args)
    {
        // 点击选中控制自己
        DroneManager.Instance.selectedDroneController = this;
        ControllerManager.Instance.SwitchController(ControlMode.PlayerControl);
        ControllerManager.Instance.SwitchController(ControlMode.DroneControl);
    }
    XRSimpleInteractable clickButton;
    public const int textureWidth = 1920;
    public const int textureHeight = 1080;
    public const int depthBuffer = 24; // 深度缓冲位数，例如 0, 16, 24, 32
    public const RenderTextureFormat textureFormat = RenderTextureFormat.ARGB32; // 纹理格式
    private void Awake()
    {
        // 注册到DroneManager中
        hashMapKey = gameObject.name + Guid.NewGuid().ToString();
        DroneManager.Instance.dronesMap.Add(hashMapKey, this);
        DroneManager.Instance._droneList.Add(this);

        clickButton = gameObject.GetComponent<XRSimpleInteractable>();
        clickButton.selectEntered.AddListener(ButtonOnSelectSelf);
        _cameraRenderTexture = new RenderTexture(textureWidth, textureHeight, depthBuffer, textureFormat); // 相机的渲染target renderTexture
        _droneCamera.targetTexture = _cameraRenderTexture;
    }
    private void Start()
    {
        // 加载设备数据
        var endpointDevice = GetComponent<EndPointDevice>();
        if (endpointDevice)
        {
            this._droneName = endpointDevice.endpontBaseData.endpoinName;
            //
        }
    }
    private void OnDestroy()
    {
        // 移除注册
        DroneManager.Instance.dronesMap.Remove(gameObject.name);
        // 控制权切换为用户
        ControllerManager.Instance.SwitchController(ControlMode.PlayerControl);
    }


}
