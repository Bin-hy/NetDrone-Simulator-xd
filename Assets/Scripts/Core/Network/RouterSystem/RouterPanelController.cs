using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// RouterPanelController 负责管理 RouterPanelView 的逻辑和数据流。
/// 它从 RouterManager 获取数据，传递给 RouterPanelView 显示，并处理来自 RouterPanelView 的用户操作，
/// 然后将数据更新通知回 RouterManager。
/// </summary>
public class RouterPanelController : MonoBehaviour
{
    [SerializeField] private RouterPanelView routerPanelView;

    private string _currentRouterID; // 当前面板显示的是哪个路由器的信息
    private RouterData _currentRouterData; // 当前路由器完整的RouterData

    private void Awake()
    {
        if (routerPanelView == null)
        {
            routerPanelView = GetComponentInChildren<RouterPanelView>();
            if (routerPanelView == null)
            {
                Debug.LogError("RouterPanelView reference is not set on RouterPanelController or not found in children!");
                return;
            }
        }

        // 订阅 RouterPanelView 的事件
        routerPanelView.OnAddRouteRequested += HandleAddRoute;
        routerPanelView.OnUpdateRouteRequested += HandleUpdateRoute;
        routerPanelView.OnDeleteRouteRequested += HandleDeleteRoute;
    }

    private void OnDestroy()
    {
        // 取消订阅事件，防止内存泄漏
        if (routerPanelView != null)
        {
            routerPanelView.OnAddRouteRequested -= HandleAddRoute;
            routerPanelView.OnUpdateRouteRequested -= HandleUpdateRoute;
            routerPanelView.OnDeleteRouteRequested -= HandleDeleteRoute;
        }
    }

    /// <summary>
    /// 初始化并显示路由器面板，加载指定路由器的数据。
    /// </summary>
    /// <param name="routerID">要显示面板的路由器ID</param>
    public void InitializeAndShow(string routerID)
    {
        _currentRouterID = routerID;
        _currentRouterData = RouterManager.Instance.GetRouterData(routerID);

        if (_currentRouterData != null)
        {
            // 将数据传递给 RouterPanelView 显示
            routerPanelView.DisplayRouterData(_currentRouterData.routerCoreStateData, _currentRouterData.routes.Items);
            routerPanelView.ShowPanel(); // 显示面板
            Debug.Log($"Router Panel displayed for RouterID: {_currentRouterID}");
        }
        else
        {
            Debug.LogError($"无法找到 RouterID: {routerID} 的数据。");
            routerPanelView.HidePanel(); // 隐藏面板
        }
    }

    /// <summary>
    /// 隐藏路由器面板。
    /// </summary>
    public void HidePanel()
    {
        routerPanelView.HidePanel();
    }

    // --- 事件处理方法 ---

    private void HandleAddRoute(RouteItemData newRouteData)
    {
        if (_currentRouterData == null) return;

        // 确保RouteID唯一性 (如果RouteID是GUID，通常是唯一的)
        if (_currentRouterData.routes.Items.Any(r => r.RouteID == newRouteData.RouteID))
        {
            Debug.LogWarning($"Attempted to add route with duplicate ID: {newRouteData.RouteID}. Generating new ID.");
            newRouteData.RouteID = Guid.NewGuid().ToString(); // 强制生成新ID
        }

        _currentRouterData.routes.Items.Add(newRouteData);
        routerPanelView.AddRouteEntryUI(newRouteData); // 更新UI
        RouterManager.Instance.UpdateRouterData(_currentRouterID, _currentRouterData); // 通知RouterManager更新数据
        Debug.Log($"Added new route for {_currentRouterID}: {newRouteData.TargetNetwork}");
    }

    private void HandleUpdateRoute(string routeID, RouteItemData updatedRouteData)
    {
        if (_currentRouterData == null) return;

        int index = _currentRouterData.routes.Items.FindIndex(item => item.RouteID == routeID);
        if (index != -1)
        {
            _currentRouterData.routes.Items[index] = updatedRouteData; // 更新数据
            routerPanelView.UpdateRouteEntryUI(updatedRouteData); // 更新UI
            RouterManager.Instance.UpdateRouterData(_currentRouterID, _currentRouterData); // 通知RouterManager更新数据
            Debug.Log($"Updated route {routeID} for {_currentRouterID}.");
        }
        else
        {
            Debug.LogWarning($"Route with ID {routeID} not found for update in {_currentRouterID}.");
        }
    }

    private void HandleDeleteRoute(string routeID)
    {
        if (_currentRouterData == null) return;

        int initialCount = _currentRouterData.routes.Items.Count;
        _currentRouterData.routes.Items.RemoveAll(item => item.RouteID == routeID); // 从数据列表中移除

        if (_currentRouterData.routes.Items.Count < initialCount)
        {
            routerPanelView.RemoveRouteEntryUI(routeID); // 更新UI
            RouterManager.Instance.UpdateRouterData(_currentRouterID, _currentRouterData); // 通知RouterManager更新数据
            Debug.Log($"Deleted route {routeID} from {_currentRouterID}.");
        }
        else
        {
            Debug.LogWarning($"Route with ID {routeID} not found for deletion in {_currentRouterID}.");
        }
    }
}