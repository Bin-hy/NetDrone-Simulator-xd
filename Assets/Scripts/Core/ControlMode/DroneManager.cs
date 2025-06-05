using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// 负责当前的切换控制器
public class DroneManager : MonoBehaviour
{
    #region 单例

    // 1. 静态实例
    private static DroneManager _instance;

    // 2. 公共访问点
    public static DroneManager Instance
    {
        get
        {
            if (_instance == null)
            {
                // 查找现有实例
                _instance = FindObjectOfType<DroneManager>();

                // 如果没有则创建新实例
                if (_instance == null)
                {
                    GameObject obj = new GameObject("DroneManager");
                    _instance = obj.AddComponent<DroneManager>();
                    DontDestroyOnLoad(obj); // 跨场景不销毁
                }
            }
            return _instance;
        }
    }
    #endregion

    public DroneController _controller;
    public event Action OnDroneCountChange;
    // 当前选中的无人机
    public DroneController selectedDroneController {
        set {
           
            ControllerManager.Instance.CloseDroneControl(); // 关闭之前的控制
            ControllerManager.Instance.SetDroneController(value);
            _controller = value;
        }
        get { return _controller; }
    }
    // public droneList
    public List<DroneController> _droneList = new List<DroneController>();

    //public dronesMap
    // 无人机字典 - 使用hashkey作为键
    private Dictionary<string, DroneController> _dronesMap = new Dictionary<string, DroneController>();

    public Dictionary<string, DroneController> dronesMap
    {
        get {
            OnDroneCountChange?.Invoke();// 当获取时候执行通知事件
            return _dronesMap; 
        }
    }
    // 获取Map中无人机的控制器
    public DroneController GetDroneControllerByHashKey(string hashKey)
    {
        if (dronesMap.TryGetValue(hashKey, out var controller))
        {
            return controller;
        }
        return null;
    }



}
