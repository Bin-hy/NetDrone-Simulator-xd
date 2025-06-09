using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq; // For LINQ methods

[Serializable]
public class RoutersData
{
    public List<RouterData> routersData;
}

public class RouterManager : MonoBehaviour
{
    #region Singleton
    private static RouterManager _instance;
    public static RouterManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<RouterManager>();
                if (_instance == null)
                {
                    GameObject obj = new GameObject("RouterManager");
                    _instance = obj.AddComponent<RouterManager>();
                    DontDestroyOnLoad(obj);
                }
            }
            return _instance;
        }
    }
    #endregion

    [SerializeField] private List<RouterController> routerControllerList; // 场景中手动关联的路由器控制器
    [SerializeField] private RouterPanelController routerPanelController; // 对RouterPanelController的引用

    private Dictionary<string, RouterController> _routerControllersMap = new Dictionary<string, RouterController>();
    private RoutersData _allRoutersData; // 存储所有路由器的原始数据

    private const string routersDataPath = "DataJson/routers-config"; // JSON配置文件路径

    public List<RouterController> AllRouters => routerControllerList; // 提供一个公共访问接口

    private void Awake()
    {
        // 确保单例正确性
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;

        // 初始化路由器控制器映射
        foreach (RouterController controller in routerControllerList)
        {
            if (controller != null && !string.IsNullOrEmpty(controller.RouterID))
            {
                _routerControllersMap[controller.RouterID] = controller;
            }
            else
            {
                Debug.LogWarning("Found a RouterController in list with null or empty RouterID.");
            }
        }

        LoadConfigData();
        InitializeRouterControllers(); // 将加载的数据分配给RouterController
    }

    void LoadConfigData()
    {
        var json = Resources.Load<TextAsset>(routersDataPath);
        if (json != null)
        {
            _allRoutersData = JsonUtility.FromJson<RoutersData>(json.text);
            Debug.Log("Router configuration loaded successfully.");
        }
        else
        {
            Debug.LogError($"Failed to load router config data from: {routersDataPath}");
            _allRoutersData = new RoutersData { routersData = new List<RouterData>() };
        }
    }

    void InitializeRouterControllers()
    {
        if (_allRoutersData == null || _allRoutersData.routersData == null)
        {
            Debug.LogWarning("No router data to initialize controllers with.");
            return;
        }

        foreach (RouterData data in _allRoutersData.routersData)
        {
            if (_routerControllersMap.TryGetValue(data.routerCoreStateData.RouterID, out RouterController controller))
            {
                controller.SetRouterData(data); // 将加载的数据设置给对应的RouterController
                // 确保生成直连路由（可以在初始化时执行一次）
                controller.GenerateDirectRoutes();
            }
            else
            {
                Debug.LogWarning($"No RouterController found for RouterID: {data.routerCoreStateData.RouterID}");
            }
        }
    }

    // 获取特定路由器的数据
    public RouterData GetRouterData(string routerID)
    {
        if (_allRoutersData != null && _allRoutersData.routersData != null)
        {
            return _allRoutersData.routersData.FirstOrDefault(r => r.routerCoreStateData.RouterID == routerID);
        }
        return null;
    }

    /// <summary>
    /// 由 RouterPanelController 或 RouterController 调用，通知 RouterManager 数据已更新，并保存。
    /// </summary>
    /// <param name="routerID">被更新的路由器ID</param>
    /// <param name="updatedData">更新后的RouterData</param>
    public void UpdateRouterData(string routerID, RouterData updatedData)
    {
        if (_allRoutersData == null || _allRoutersData.routersData == null) return;

        int index = _allRoutersData.routersData.FindIndex(r => r.routerCoreStateData.RouterID == routerID);
        if (index != -1)
        {
            _allRoutersData.routersData[index] = updatedData;
            Debug.Log($"Router data for {routerID} updated in RouterManager's internal list.");
            // 此时可以触发保存到文件的操作
            SaveAllRoutersDataToFile();
        }
        else
        {
            Debug.LogError($"Failed to update routes for RouterID: {routerID}. Data not found in RouterManager.");
        }
    }

    // 保存所有路由器数据到文件
    public void SaveAllRoutersDataToFile()
    {
        if (_allRoutersData == null)
        {
            Debug.LogWarning("No router data to save.");
            return;
        }

        string json = JsonUtility.ToJson(_allRoutersData, true); // true for pretty print

        // 在 Unity Editor 中，可以将数据保存到 Application.dataPath + "/Resources/DataJson/routers-config.json"
        // 但在运行时，通常保存到 Application.persistentDataPath
        string filePath = Application.persistentDataPath + "/routers-config.json";
        System.IO.File.WriteAllText(filePath, json);
        Debug.Log($"All router data saved to: {filePath}");

        // 注意：在UnityEditor中，Resources.Load 只能从 Assets/Resources 目录读取。
        // 如果要在运行时保存后立即通过 Resources.Load 读取到最新数据，你需要额外的Editor脚本来复制文件。
        // 对于运行时保存，通常直接从 persistentDataPath 读取或只在启动时加载一次。
    }

    // 获取特定路由器
    public RouterController GetRouterController(string routerID)
    {
        _routerControllersMap.TryGetValue(routerID, out RouterController controller);
        return controller;
    }

    // 处理路由器选中事件，显示或隐藏RouterPanel
    public void HandleRouterSelection(string selectedRouterID)
    {
        if (string.IsNullOrEmpty(selectedRouterID))
        {
            Debug.LogWarning("Attempted to handle router selection with null or empty selectedRouterID.");
            // 如果没有选中任何路由器，隐藏面板
            if (routerPanelController != null) routerPanelController.HidePanel();
            return;
        }

        // 遍历所有路由器控制器，隐藏它们的3D RouterView（如果适用）
        foreach (RouterController controller in routerControllerList)
        {
            if (controller != null)
            {
                // 可以让 RouterController 内部管理其自身的3D视图显示/隐藏
                // 或者在这里通过 GetComponent<RouterView>() 隐藏
                // 这里我们假设 RouterController 负责其3D实体的激活状态
                if (controller.RouterID != selectedRouterID)
                {
                    // 可以让 RouterController 内部处理非选中状态的隐藏
                    // controller.gameObject.SetActive(false); // 根据你的交互逻辑决定是否完全隐藏3D模型
                    // 如果 RouterView 也是3D物体的一部分，可能需要访问它来 Hide
                    RouterView rv = controller.GetComponentInChildren<RouterView>();
                    if (rv != null) rv.Hide(); // 隐藏3D RouterView
                }
            }
        }

        // 显示选中路由器的 RouterPanel
        if (routerPanelController != null)
        {
            routerPanelController.InitializeAndShow(selectedRouterID);
        }
        else
        {
            Debug.LogError("RouterPanelController reference is not set on RouterManager!");
        }
    }

    private void OnDestroy()
    {
        // 如果 RouterPanelController 有事件订阅，在这里也需要解绑
        // 例如：routerPanelController.OnSomeEvent -= SomeHandler;
    }
}