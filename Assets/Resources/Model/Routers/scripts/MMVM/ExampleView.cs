using System;
using System.Collections.Generic;
using UnityEngine;

// 负责界面渲染
public class ExampleView : UI3DPanel
{
    [SerializeField] private ExampleRouterBaseStateSubView exampleRouterBaseStateSubView;
    // 存放RouteItem的结点,
    [SerializeField] private Transform listContent;

    // 路由条目Item
    [SerializeField] private GameObject RouteItemPrefab;

    private ExampleModel currentModel; // 缓存当前视图对应的Model
    private List<ExampleItem> routesItemList;

    #region 事件
    public event Action OnUpdated;
    public event Action OnDeleted;
    public event Action OnAdded;
    #endregion

    public void Initialize(ExampleModel model)
    {
        if (model == null)
        {
            Debug.LogError("RouterView: Initializing with a null RouterModel.");
            return;
        }
        currentModel = model;

        // 订阅Model的数据变化事件
        currentModel.OnDataChanged += UpdateDisplay;
        // 绑定VR交互事件
        // 首次渲染
        UpdateDisplay();
        Debug.Log($"RouterView initialized for '{currentModel.eRouterData.routerCoreStateData.routeId}'.");
    }


    void UpdateDisplay()
    {
        // 更新视图现实
        // 清除
        cleanRoutes();
        // 加载
        
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
    }

    public void cleanRoutes()
    {
        // 销毁所有现有的 RouteItem GameObject
        foreach (var item in routesItemList)
        {
            if (item != null && item.gameObject != null)
            {
                Destroy(item.gameObject);
            }
        }
        Debug.Log("RouterView: 已清空所有路由条目。");
    }
    

    // 当GameObject销毁时取消订阅，防止内存泄漏
    private void OnDestroy()
    {
        if (currentModel != null)
        {
            currentModel.OnDataChanged -= UpdateDisplay;
        }
    }
}