using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 负责显示路由器核心状态和路由表条目（仅UI显示，不处理数据逻辑或用户输入）。
/// RouterView 从 RouterController 接收数据，并负责创建、更新和销毁 RouteItem 的UI实例。
/// 不再承担加载/保存数据的职责，该职责由 RouterManager 负责。
/// </summary>
public class RouterView : UI3DPanel
{
    [Header("路由器基本信息")]
    [SerializeField] private RouterCoreState baseStateDisplay; // 显示路由器ID和直连子网数量

    [Header("路由表信息")]
    [SerializeField] private Transform listContent; // RouteItem Prefab的父对象
    [SerializeField] private GameObject RouteItemPrefab; // RouteItem的UI Prefab

    // 存储当前显示的RouteItem实例，方便管理
    private Dictionary<string, RouteItem> RoutesMap = new Dictionary<string, RouteItem>();

    // 事件：通知RouterManager保存路由表 (RouterView不再直接保存，而是通过事件通知Controller)
    public event Action<string, List<RouteItemData>> OnRoutesUpdated; // string为RouterID, List<RouteItemData>为更新后的路由数据

    // 事件：通知RouterPanelController选中了某个路由条目
    public event Action<string> OnRouteItemSelected;

    // 初始化视图：接收核心状态数据和路由数据列表，并更新UI
    public void InitializeView(RouterCoreStateData coreStateData, List<RouteItemData> routeDataList)
    {
        if (baseStateDisplay != null)
        {
            baseStateDisplay.Initialize(coreStateData);
        }

        // 清空现有UI条目
        cleanRoutes();

        // 根据传入的routeDataList创建或更新RouteItem UI
        if (routeDataList != null)
        {
            foreach (var data in routeDataList)
            {
                AddRouteEntry(data);
            }
        }
        else
        {
            Debug.LogWarning($"RouterView: routeDataList for RouterID {coreStateData.RouterID} is null.");
        }
    }

    // 在UI上添加一个路由条目
    public void AddRouteEntry(RouteItemData data)
    {
        if (RoutesMap.ContainsKey(data.RouteID))
        {
            Debug.LogWarning($"RouteItem with ID {data.RouteID} already exists in RoutesMap. Updating existing entry.");
            UpdateRouteEntry(data); // 如果已存在，则更新
            return;
        }

        GameObject routeItemGO = Instantiate(RouteItemPrefab, listContent);
        RouteItem routeItem = routeItemGO.GetComponent<RouteItem>();
        if (routeItem != null)
        {
            routeItem.Initialize(data);
            RoutesMap.Add(data.RouteID, routeItem);

            // 添加点击事件监听
            // 假设 RouteItem 上有一个 Button 组件或者可以响应点击的 Collider
            // 这里为了简化，直接在 RouteItem prefab上添加一个Button，并监听其点击事件
            Button selectButton = routeItemGO.GetComponentInChildren<Button>();
            if (selectButton != null)
            {
                selectButton.onClick.AddListener(() => OnRouteItemSelected?.Invoke(data.RouteID));
            }
        }
        else
        {
            Debug.LogError("RouteItemPrefab does not have a RouteItem component!");
            Destroy(routeItemGO);
        }
    }

    // 在UI上更新一个路由条目
    public void UpdateRouteEntry(RouteItemData newData)
    {
        if (RoutesMap.TryGetValue(newData.RouteID, out RouteItem existingItem))
        {
            existingItem.Initialize(newData); // 更新UI显示
        }
        else
        {
            Debug.LogWarning($"Cannot update route {newData.RouteID}: not found in UI map.");
            // 如果UI上没有，但数据存在，可以考虑重新添加，但这里假定数据源与UI同步
        }
    }

    // 从UI上移除一个路由条目
    public void RemoveRouteEntry(string routeID)
    {
        if (RoutesMap.TryGetValue(routeID, out RouteItem routeItem))
        {
            RoutesMap.Remove(routeID);
            if (routeItem != null && routeItem.gameObject != null)
            {
                Destroy(routeItem.gameObject);
                Debug.Log($"成功删除了路由条目: {routeID}");
            }
            else
            {
                Debug.LogWarning($"路由条目 {routeID} 的 GameObject 已经不存在或为空，无法销毁。");
            }
        }
        else
        {
            Debug.LogWarning($"路由条目 {routeID} 不存在于RoutesMap中，无法删除。");
        }
    }

    // 清空所有路由条目UI
    public void cleanRoutes()
    {
        foreach (var item in RoutesMap.Values)
        {
            if (item != null && item.gameObject != null)
            {
                Destroy(item.gameObject);
            }
        }
        RoutesMap.Clear();
    }

    // RouterView 不再直接处理 LoadView 或 SaveRoutesToFile
    // LoadView 的概念被 InitializeView 替代，因为它现在是从 Controller 接收数据并渲染。
    // SaveRoutesToFile 的职责通过 OnRoutesUpdated 事件传递给 Controller。

    // 隐藏和显示面板
    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}