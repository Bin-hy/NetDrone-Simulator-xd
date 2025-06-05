using System;
using System.Collections.Generic;
using UnityEngine;

public class RouterView : UI3DPanel
{
    [Header("路由器基本信息")]
    [SerializeField] private RouterCoreState baseState;
    
    // 1 基本信息
    public RouterCoreState BaseState { get { return baseState; } set { baseState = value; } }
    // 2 
    [Tooltip("当前路由器编号Id")]public int RouterId; // 1,2,3,4 ，共四个路由器
    // 存放RoutesMap的结点,
    [SerializeField] private Transform listContent;
    // 路由字典
    private Dictionary<string, RouteItem> RoutesMap = new Dictionary<string, RouteItem>();
    // 路由信息 列表
    private List<RouteItem> RoutesList = new List<RouteItem>();

    // 路由条目Item
    [SerializeField] private GameObject RouteItemPrefab;

    public event Action<RouterView> updateViewEvent;

    // 路由数据文件对应的 Key
    [SerializeField] private string routerDataKey = "router_routes_data"; // 可以在 Inspector 中设置，也可以是 RouterID 等唯一标识

    // 添加一个辅助方法来设置路由器基本信息


    public void LoadView()
    {
        // 加载数据视图变化
        // 1. 移除所有的视图实体
        cleanRoutes(); // 清空当前显示的路由条目

        // 2. 重新添加
        // 使用 JSONDataUtility 从本地加载路由数据
        RouteItemDataListWrapper dataWrapper = JSONDataUtility.LoadData<RouteItemDataListWrapper>(routerDataKey);

        if (dataWrapper != null && dataWrapper.routes != null)
        {
            foreach (var routeData in dataWrapper.routes)
            {
                addRoutes(routeData); // 重新添加路由条目到视图
            }
            Debug.Log($"RouterView: 已从 {routerDataKey}.json 加载 {dataWrapper.routes.Count} 条路由数据。");
        }
        else
        {
            Debug.LogWarning($"RouterView: 未找到路由数据或数据为空，键名: {routerDataKey}，将加载默认或空路由表。");
            // 可以添加一些默认路由或提示用户没有数据
        }

        // 更新路由器基本信息显示
        //SetRouterCoreStateDisplay();

        // 触发视图更新事件，通知其他监听者视图已加载
        updateViewEvent?.Invoke(this);
    }

    public void addRoutes(RouteItemData routeData)
    {
        if (RouteItemPrefab == null)
        {
            Debug.LogError("RouteItemPrefab 未赋值，无法添加路由条目。");
            return;
        }

        if (string.IsNullOrEmpty(routeData.routesID))
        {
            Debug.LogError("路由条目数据缺少 routesID，无法添加。");
            return;
        }

        if (RoutesMap.ContainsKey(routeData.routesID))
        {
            Debug.LogWarning($"尝试添加重复的路由条目ID: {routeData.routesID}，跳过。");
            return;
        }

        // 实例化一个
        var go = Instantiate(RouteItemPrefab, listContent); // 实例化并设置父级为 listContent
        go.name = routeData.routesID;
        var addRouteItem = go.GetComponent<RouteItem>();

        if (addRouteItem == null)
        {
            Debug.LogError($"RouteItemPrefab 缺少 RouteItem 组件，无法添加路由条目: {go.name}");
            Destroy(go); // 销毁错误的实例
            return;
        }

        // 初始化 RouteItem 的数据
        addRouteItem.Initialize(routeData); // 使用 Initialize 方法初始化数据

        // 添加一个路由条目到字典和列表
        RoutesMap.Add(routeData.routesID, addRouteItem);
        RoutesList.Add(addRouteItem);
    }

    public void cleanRoutes()
    {
        // 销毁所有现有的 RouteItem GameObject
        foreach (var item in RoutesList)
        {
            if (item != null && item.gameObject != null)
            {
                Destroy(item.gameObject);
            }
        }
        // 清空字典和列表
        RoutesMap.Clear();
        RoutesList.Clear();
        Debug.Log("RouterView: 已清空所有路由条目。");
    }

    public void deleteRoutes(string routesId)
    {
        RouteItem routeItem;
        bool isGet = RoutesMap.TryGetValue(routesId, out routeItem);
        if (!isGet) // 如果 isGet 为 false，说明不存在
        {
            Debug.LogWarning($"尝试删除不存在的路由条目ID: {routesId}。");
            return;
        }

        // 从字典中移除
        RoutesMap.Remove(routesId);
        // 从列表中移除
        RoutesList.Remove(routeItem);

        // 销毁视图对应的 GameObject
        if (routeItem != null && routeItem.gameObject != null)
        {
            Destroy(routeItem.gameObject);
            Debug.Log($"成功删除了路由条目: {routesId}");
        }
        else
        {
            Debug.LogWarning($"路由条目 {routesId} 的 GameObject 已经不存在或为空，无法销毁。");
        }
    }

    // 为了 JsonUtility 能够反序列化 RouteItemData 的列表，需要一个包装类
    [Serializable]
    private class RouteItemDataListWrapper
    {
        public List<RouteItemData> routes;
    }

    // 示例：保存路由数据的方法
    public void SaveRoutesToFile()
    {
        RouteItemDataListWrapper dataWrapper = new RouteItemDataListWrapper();
        dataWrapper.routes = new List<RouteItemData>();

        foreach (var item in RoutesList)
        {
            if (item != null)
            {
                // 从 RouteItem 中提取数据来构造 RouteItemData
                RouteItemData data = new RouteItemData
                {
                    routesID = item.RoutesID,
                    TargetNetwork = item.TargetNetwork,
                    NextJump = item.NextJump,
                    OutInterface = item.OutInterface
                };
                dataWrapper.routes.Add(data);
            }
        }

        // 使用 JSONDataUtility 保存数据
        JSONDataUtility.SaveData(routerDataKey, dataWrapper);
    }
}