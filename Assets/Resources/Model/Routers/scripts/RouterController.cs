using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[Serializable]
public class RouterData
{
    public RouterCoreStateData routerCoreStateData;
    public RouteItemListWrapper routes; // 使用新的RouteItemListWrapper
}

/// <summary>
/// RouterController 应该管理其对应的 RouterView 的显示和隐藏。
/// UpdataView 应该负责将 RouterData 传递给 RouterView，让 RouterView 刷新显示。
/// 移除构造函数 RouterController(RouterCoreState baseState)，因为 MonoBehaviour 不应通过构造函数初始化，而应通过 Awake 或 Start 或公共方法。
/// 添加一个 SetRouterData 方法，用于在 RouterManager 中初始化时设置数据。
/// </summary>
public class RouterController : MonoBehaviour
{
    /// <summary>
    /// 需要给 Go 挂载交互脚本
    /// </summary>
    [SerializeField] private XRSimpleInteractable interactableCom;
    [SerializeField] private RouterView routerView; // 3D场景中的RouterView
    private RouterData _routerData;
    private bool _isSelected = false; // 判断当前是否选中

    public string RouterID => _routerData?.routerCoreStateData?.RouterID; // 方便外部获取ID
    public RouterData GetRouterData() => _routerData; // 允许外部获取完整数据

    private void Awake()
    {
        // 确保 routerView 引用已设置
        if (routerView == null)
        {
            Debug.LogError("RouterView reference is not set on RouterController: " + gameObject.name);
        }

        if (interactableCom != null)
        {
            interactableCom.hoverEntered.AddListener(OnHoverEnter);
            interactableCom.hoverExited.AddListener(OnHoverExit);
            interactableCom.selectEntered.AddListener(OnSelectEnter);
        }
        else
        {
            Debug.LogError("XRSimpleInteractable reference is not set on RouterController: " + gameObject.name);
        }
    }

    // 由 RouterManager 在加载配置数据时调用，初始化 RouterController 的数据
    public void SetRouterData(RouterData data)
    {
        _routerData = data;
        // 初始化 RouterView 的核心状态信息 (注意：这里RouterView不再负责路由表显示)
        if (routerView != null && _routerData != null && _routerData.routerCoreStateData != null)
        {
            // RouterView 现在只显示核心状态，路由表由 RouterPanelController 处理
            routerView.InitializeView(_routerData.routerCoreStateData, new List<RouteItemData>()); // 路由表数据传入空列表或不传入，RouterView不再显示路由表
        }
    }

    /// <summary>
    /// 更新路由器的 RouterData。当 RouterManager 从面板收到更新时调用。
    /// </summary>
    /// <param name="newData"></param>
    public void UpdateRouterData(RouterData newData)
    {
        _routerData = newData;
        // 如果需要，可以在这里更新3D场景中的RouterView的显示（例如核心状态）
        if (routerView != null && _routerData != null && _routerData.routerCoreStateData != null)
        {
            routerView.InitializeView(_routerData.routerCoreStateData, new List<RouteItemData>());
        }
        Debug.Log($"Router {RouterID} data updated from RouterManager.");
    }

    /// <summary>
    /// 根据路由器的接口信息，自动生成直连路由条目。
    /// </summary>
    public void GenerateDirectRoutes()
    {
        if (_routerData == null || _routerData.routerCoreStateData == null || _routerData.routerCoreStateData.Interfaces == null)
        {
            Debug.LogWarning($"Router {RouterID} has no interface data to generate direct routes.");
            return;
        }

        // 清空旧的直连路由，避免重复
        // 假设直连路由的 NextHop 是 "直连" 或 0.0.0.0
        // 或者，可以根据 RouteID 约定进行清除，例如 "routerIdX_local" 或 "routerIdX_direct"
        _routerData.routes.Items.RemoveAll(r => r.NextHop == "直连" || r.Metric == 0);

        foreach (var interfaceData in _routerData.routerCoreStateData.Interfaces)
        {
            // 提取网络地址和子网掩码
            string networkAddress = GetNetworkAddress(interfaceData.IPAddress, interfaceData.SubnetMask);

            // 对于直连路由，目标网络就是接口所在网络，下一跳是"直连"或0.0.0.0，出接口是当前接口
            string targetNetwork = networkAddress;
            string subnetMask = interfaceData.SubnetMask;
            string nextHop = "直连"; // 通常直连路由的下一跳是"直连"或者可以是自己的接口IP
            string outInterface = interfaceData.InterfaceName;

            _routerData.routes.Items.Add(new RouteItemData
            {
                RouteID = Guid.NewGuid().ToString(), // 生成唯一ID
                TargetNetwork = targetNetwork,
                SubnetMask = subnetMask,
                NextHop = nextHop,
                OutInterface = outInterface,
                Metric = 0 // 直连路由的度量通常为0
            });
        }

        // 通知 RouterManager 更新数据并保存
        RouterManager.Instance.UpdateRouterData(_routerData.routerCoreStateData.RouterID, _routerData);
        Debug.Log($"Router {RouterID} generated direct routes and saved.");
    }

    // 辅助方法：根据IP地址和子网掩码计算网络地址
    private string GetNetworkAddress(string ipAddress, string subnetMask)
    {
        System.Net.IPAddress ip = System.Net.IPAddress.Parse(ipAddress);
        System.Net.IPAddress mask = System.Net.IPAddress.Parse(subnetMask);

        byte[] ipBytes = ip.GetAddressBytes();
        byte[] maskBytes = mask.GetAddressBytes();
        byte[] networkBytes = new byte[4];

        for (int i = 0; i < 4; i++)
        {
            networkBytes[i] = (byte)(ipBytes[i] & maskBytes[i]);
        }
        return new System.Net.IPAddress(networkBytes).ToString();
    }


    void OnHoverEnter(HoverEnterEventArgs args)
    {
        if (_isSelected) return;
        // 如果有3D视图需要显示（如高亮），在这里设置
        // gameObject.SetActive(true); // 如果这个是UI，可能会引起问题
    }
    void OnHoverExit(HoverExitEventArgs args)
    {
        if (_isSelected) return;
        // 如果有3D视图需要隐藏，在这里设置
        // gameObject.SetActive(false);
    }

    void OnSelectEnter(SelectEnterEventArgs args)
    {
        // 切换选中状态
        _isSelected = !_isSelected;
        // 通知 RouterManager 处理选中状态，它会负责显示/隐藏 RouterPanel
        RouterManager.Instance.HandleRouterSelection(RouterID);
    }

    private void OnDestroy()
    {
        if (interactableCom != null)
        {
            interactableCom.hoverEntered.RemoveListener(OnHoverEnter);
            interactableCom.hoverExited.RemoveListener(OnHoverExit);
            interactableCom.selectEntered.RemoveListener(OnSelectEnter);
        }
    }
}