using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// 对无人机进行CRUD ， 编辑无人机的基础信息
public class DronesManagerPanel : MonoBehaviour
{
    #region 单例

    // 1. 静态实例
    private static SubNetworkManager _instance;

    // 2. 公共访问点
    public static SubNetworkManager Instance
    {
        get
        {
            if (_instance == null)
            {
                // 查找现有实例
                _instance = FindObjectOfType<SubNetworkManager>();

                // 如果没有则创建新实例
                if (_instance == null)
                {
                    GameObject obj = new GameObject("SubNetworkManager");
                    _instance = obj.AddComponent<SubNetworkManager>();
                    DontDestroyOnLoad(obj); // 跨场景不销毁
                }
            }
            return _instance;
        }
    }
    #endregion

    [SerializeField] private TextMeshProUGUI title;
    #region 加载网络的按钮
    [SerializeField] private Button button1;
    [SerializeField] private Button button2;
    [SerializeField] private Button button3;
    [SerializeField] private Button button4;
    [SerializeField] private Button button5;

    private SubNetworkController _curSubNetworkController; // 当前的子网
    // 使用  a property or a getter method 规避问题
    public SubNetworkController curSubNetworkController {
        get { return _curSubNetworkController; }
        set
        {
            // 如果当前子网控制器有变化
            if (_curSubNetworkController != value)
            {
                // 如果旧的控制器不为null，先取消订阅事件
                if (_curSubNetworkController != null)
                {
                    _curSubNetworkController.OnEndpointListChanged -= UpdateView;
                    Debug.Log($"取消订阅旧网络 {_curSubNetworkController.networkName} 的事件");
                }

                _curSubNetworkController = value;

                // 如果新的控制器不为null，订阅事件
                if (_curSubNetworkController != null)
                {
                    _curSubNetworkController.OnEndpointListChanged += UpdateView;
                    Debug.Log($"订阅新网络 {_curSubNetworkController.networkName} 的事件");
                }
            }
        }
    } // 当前的子网
    #endregion

    #region 无人机CRUD管理界面
    [SerializeField] private Transform itemContent; // 内容填充的地方
    [SerializeField] private Button addButton; // 添加的按钮
    [SerializeField] private GameObject item; // 添加的按钮

    #region 无人机列表
    [SerializeField] private GameObject dronePrefab; // 无人机预制体
    #endregion

    #endregion

    // 渲染 界面
    private void UpdateView()
    {
        Debug.Log($"更新界面渲染,当前网络{curSubNetworkController.networkName}，当前终端数量 {curSubNetworkController.curEndpointCount} , endlists 数量 is{curSubNetworkController.endpointList.Count}");
        // 清空界面

        for (int i = itemContent.childCount - 1; i >= 0; i--)
        {
            // 获取子Transform
            Transform child = itemContent.GetChild(i);
            // 销毁子GameObject
            Destroy(child.gameObject);
        }
        if (!curSubNetworkController)
        {
            Debug.LogWarning("未选择查看的网络");
            return;
        }
        Debug.LogWarning($"{curSubNetworkController.networkName}");
        // 加载界面数据
        foreach (var endpoint in curSubNetworkController.endpointList)
        {
            var go = Instantiate(item, itemContent);
            var dronePanelItem = go.GetComponent<DroneManagerPanelItem>();
            dronePanelItem.Init(endpoint); // string targetNetwork, string macAddress,string deviceName, EndPointDevice endPointDevice
        }
        Debug.LogWarning($"{curSubNetworkController.networkName} UpdateView run : current count is {curSubNetworkController.endpointList.Count}");
    }
    private void Awake()
    {
        addButton.onClick.AddListener(OnAddDeviceClicked);

        //var net1 = SubNetworkManager.Instance.getSubNetworkController("network-1");
        //var net2 = SubNetworkManager.Instance.getSubNetworkController("network-2");
        //var net3 = SubNetworkManager.Instance.getSubNetworkController("network-3");
        //var net4 = SubNetworkManager.Instance.getSubNetworkController("network-4");
        //var net5 = SubNetworkManager.Instance.getSubNetworkController("network-5");
        //net1.OnChange += UpdateView;
        //net2.OnChange += UpdateView;
        //net3.OnChange += UpdateView;
        //net4.OnChange += UpdateView;
        //net5.OnChange += UpdateView;
        button1.onClick.AddListener(() => {
            curSubNetworkController = SubNetworkManager.Instance.getSubNetworkController("network-1");
            title.text = "网络 1";
            Debug.LogWarning($"切换到了{curSubNetworkController.networkName}");
            UpdateView();
        });
        button2.onClick.AddListener(() => {
            curSubNetworkController = SubNetworkManager.Instance.getSubNetworkController("network-2");
            Debug.LogWarning($"切换到了{curSubNetworkController.networkName}");
            title.text = "网络 2";
            UpdateView();

        });
        button3.onClick.AddListener(() => {
            curSubNetworkController = SubNetworkManager.Instance.getSubNetworkController("network-3");
            title.text = "网络 3";
            Debug.LogWarning($"切换到了{curSubNetworkController.networkName}");
            UpdateView();
        });
        button4.onClick.AddListener(() => {
            curSubNetworkController = SubNetworkManager.Instance.getSubNetworkController("network-4");
            title.text = "网络 4";
            Debug.LogWarning($"切换到了{curSubNetworkController.networkName}");
            UpdateView();
        });
        button5.onClick.AddListener(() => {
            curSubNetworkController = SubNetworkManager.Instance.getSubNetworkController("network-5");
            title.text = "网络 5";
            Debug.LogWarning($"切换到了{curSubNetworkController.networkName}");
            UpdateView();
        });
    }

    // 当按钮被点击时将调用此方法
    private void OnAddDeviceClicked()
    {
        // 此时，'curSubNetworkController' 将是点击时的当前值
        if (curSubNetworkController == null)
        {
            UIManager.Instance.OpenConfirm("ConfirmToolTip", "提示", "请先在右侧选中管理的网络后操作", () => { });
            return;
        }
        if (curSubNetworkController.isFull)
        {
            UIManager.Instance.OpenConfirm("ConfirmToolTip", "提示", $"当前网络只允许添加{curSubNetworkController.maxEndpointCout}", () => { });
            return;
        }
        curSubNetworkController.createOneDevice(dronePrefab);
    }

    void OnDestroy()
    {
        // 在销毁时取消订阅，防止内存泄漏
        if (_curSubNetworkController != null)
        {
            _curSubNetworkController.OnEndpointListChanged -= UpdateView;
        }
        // 移除按钮监听器
        button1.onClick.RemoveAllListeners();
        button2.onClick.RemoveAllListeners();
        button3.onClick.RemoveAllListeners();
        button4.onClick.RemoveAllListeners();
        button5.onClick.RemoveAllListeners();
    }

}
