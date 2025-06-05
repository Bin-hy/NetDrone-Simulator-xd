using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.XR;

public enum ControlMode
{
    PlayerControl,
    DroneControl
}


/// <summary>
/// 数值的对照按照官方，PICO4 Ultra
/// https://developer.picoxr.com/zh/document/unity/input-mapping/#2b382374
/// </summary>
public struct DeviceState
{
    // 获取扳机的值
    public bool triggerValue;
    // 获取遥感的值
    // 1. 遥感按下
    public bool primaryAxisClickValue;
    // 2.遥感 移动
    public Vector2 primaryAxis2DValue;

    // 按键 X | A
    public bool primaryButtonValue;

    // 按键 Y | B
    public bool secondaryButtonValue;

    // 抓握
    // 1.状态
    public bool isGrip;
    // 2.数值
    public float grip;
}

// 1.负责当前的切换控制器
// 2.负责全局获取控制器的输入
public class ControllerManager : MonoBehaviour
{
    ControllerBase curController;
    ControlMode curState;
    #region 单例

    // 1. 静态实例
    private static ControllerManager _instance;

    // 2. 公共访问点
    public static ControllerManager Instance
    {
        get
        {
            if (_instance == null)
            {
                // 查找现有实例
                _instance = FindObjectOfType<ControllerManager>();

                // 如果没有则创建新实例
                if (_instance == null)
                {
                    GameObject obj = new GameObject("ControllerManager");
                    _instance = obj.AddComponent<ControllerManager>();
                    DontDestroyOnLoad(obj); // 跨场景不销毁
                }
            }
            return _instance;
        }
    }
    #endregion

    [SerializeField] private PlayerController _playerController;
    [SerializeField] private DroneController _droneController;
    // 设置当前的无人机控制器
    public void SetDroneController(DroneController controller)
    {
        CloseDroneControl();
        _droneController = controller;
    }
    public void CloseDroneControl() // 关闭当前的控制
    {
        if (_droneController != null)
        {
            _droneController.isControled = false;
        }
    }
    // 切换控制器
    public void SwitchController(ControlMode newState)
    {
        curState = newState;
        switch (newState)
        {
            case ControlMode.PlayerControl:
                _playerController.isControled = true;
                if(_droneController) _droneController.isControled = false;

                break;
            case ControlMode.DroneControl:
                _playerController.isControled = false;
                if (_droneController) _droneController.isControled = true;
                break;
        }
    }

    #region 输入
    private InputDevice LeftHandDevice;
    private InputDevice rightHandDevice;
    public DeviceState _leftDeviceState = new DeviceState();
    public DeviceState _rightDeviceState = new DeviceState();
    public DeviceState leftDeviceState
    {
        get { return _leftDeviceState; }
    }
    public DeviceState rightDeviceState
    {
        get { return _rightDeviceState; }
    }
    #endregion

    private void Start()
    {
        SwitchController(ControlMode.PlayerControl);// 切换玩家
    }
    private void Update()
    {
        LoadDevices();
        // 负责获取值
        GetDeviceTriggerValue(LeftHandDevice, ref _leftDeviceState);
        GetDeviceTriggerValue(rightHandDevice, ref _rightDeviceState);

    }

    // 输入获取
    /// <summary>
    /// 加载手柄
    /// </summary>
    void LoadDevices()
    {
        LeftHandDevice = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        rightHandDevice = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
    }

    /// <summary>
    /// 获取值变化
    /// </summary>
    /// <param name="device"></param>
    /// <param name="state"></param>
    private void GetDeviceTriggerValue(InputDevice device, ref DeviceState state)
    {
        // 获取扳机的值
        device.TryGetFeatureValue(CommonUsages.triggerButton, out state.triggerValue);

        // 获取遥感的值
        // 1. 遥感按下
        device.TryGetFeatureValue(CommonUsages.primary2DAxisClick, out state.primaryAxisClickValue);

        // 2.遥感 移动
        device.TryGetFeatureValue(CommonUsages.primary2DAxis, out state.primaryAxis2DValue);

        // 按键 X | A
        device.TryGetFeatureValue(CommonUsages.primaryButton, out state.primaryButtonValue);

        // 按键 Y | B
        device.TryGetFeatureValue(CommonUsages.secondaryButton, out state.secondaryButtonValue);

        // 抓握
        // 1.状态
        device.TryGetFeatureValue(CommonUsages.gripButton, out state.isGrip);
        // 2.数值
        device.TryGetFeatureValue(CommonUsages.grip, out state.grip);
    }

}
