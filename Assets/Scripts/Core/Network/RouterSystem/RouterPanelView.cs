using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // For TextMeshProUGUI, TMP_InputField
using System.Linq; // For .Where() and .FirstOrDefault()

public class RouterPanelView : MonoBehaviour
{
    [Header("路由器基本信息显示")]
    [SerializeField] private TextMeshProUGUI routerIDText;
    [SerializeField] private TextMeshProUGUI directSubNetworkCountText;
    [SerializeField] private TextMeshProUGUI interfacesInfoText; // 用于显示接口信息

    [Header("路由条目输入字段")]
    [SerializeField] private TMP_InputField targetNetworkInput;
    [SerializeField] private TMP_InputField subnetMaskInput;
    [SerializeField] private TMP_InputField nextHopInput;
    [SerializeField] private TMP_InputField outInterfaceInput;
    [SerializeField] private TMP_InputField metricInput;

    [Header("路由操作按钮")]
    [SerializeField] private Button addRouteButton;
    [SerializeField] private Button updateRouteButton;
    [SerializeField] private Button deleteRouteButton;
    [SerializeField] private Button clearInputButton; // 新增清空输入字段的按钮

    [Header("路由表显示")]
    [SerializeField] private Transform routeItemContentParent; // 路由条目UI的父对象
    [SerializeField] private GameObject routeItemPrefab; // 单个路由条目的Prefab

    private List<RouteItem> _currentRouteItemUIs = new List<RouteItem>(); // 当前显示的路由条目UI实例
    private string _selectedRouteIDForEdit = null; // 存储当前选中进行修改/删除的路由条目的ID

    // 事件：通知Controller进行数据操作
    public event Action<RouteItemData> OnAddRouteRequested;
    public event Action<string, RouteItemData> OnUpdateRouteRequested;
    public event Action<string> OnDeleteRouteRequested;

    private void Awake()
    {
        // 绑定按钮点击事件
        addRouteButton.onClick.AddListener(OnAddRouteButtonClicked);
        updateRouteButton.onClick.AddListener(OnUpdateRouteButtonClicked);
        deleteRouteButton.onClick.AddListener(OnDeleteRouteButtonClicked);
        clearInputButton.onClick.AddListener(ClearInputFields);
    }

    private void OnDestroy()
    {
        // 解绑按钮点击事件，防止内存泄漏
        addRouteButton.onClick.RemoveListener(OnAddRouteButtonClicked);
        updateRouteButton.onClick.RemoveListener(OnUpdateRouteButtonClicked);
        deleteRouteButton.onClick.RemoveListener(OnDeleteRouteButtonClicked);
        clearInputButton.onClick.RemoveListener(ClearInputFields);
    }

    /// <summary>
    /// 显示路由器核心状态和路由表
    /// </summary>
    /// <param name="coreState">路由器的核心状态数据</param>
    /// <param name="routeDataList">路由表数据列表</param>
    public void DisplayRouterData(RouterCoreStateData coreState, List<RouteItemData> routeDataList)
    {
        // 更新路由器基本信息
        if (routerIDText != null) routerIDText.text = coreState.RouterID;
        if (directSubNetworkCountText != null) directSubNetworkCountText.text = coreState.directSubNetworkCount.ToString();

        // 更新接口信息
        if (interfacesInfoText != null)
        {
            string interfacesInfo = "Interfaces:\n";
            if (coreState.Interfaces != null)
            {
                foreach (var iface in coreState.Interfaces)
                {
                    interfacesInfo += $"{iface.InterfaceName}: {iface.IPAddress}/{iface.SubnetMask}\n";
                }
            }
            interfacesInfoText.text = interfacesInfo;
        }

        // 清空旧的路由条目UI
        ClearRouteEntries();

        // 创建新的路由条目UI
        if (routeDataList != null)
        {
            foreach (var data in routeDataList)
            {
                AddRouteEntryUI(data);
            }
        }

        ClearInputFields(); // 每次显示新路由器的路由表时，清空输入字段
        _selectedRouteIDForEdit = null; // 重置选中状态
    }

    /// <summary>
    /// 清空当前显示的所有路由条目UI
    /// </summary>
    public void ClearRouteEntries()
    {
        foreach (var item in _currentRouteItemUIs)
        {
            if (item != null && item.gameObject != null)
            {
                Destroy(item.gameObject);
            }
        }
        _currentRouteItemUIs.Clear();
    }

    /// <summary>
    /// 在UI上添加一个新的路由条目
    /// </summary>
    /// <param name="data">要添加的路由数据</param>
    public void AddRouteEntryUI(RouteItemData data)
    {
        GameObject routeItemGO = Instantiate(routeItemPrefab, routeItemContentParent);
        RouteItem routeItem = routeItemGO.GetComponent<RouteItem>();
        if (routeItem != null)
        {
            routeItem.Initialize(data);
            _currentRouteItemUIs.Add(routeItem);

            // 为每个路由条目添加选择功能
            Button selectButton = routeItemGO.GetComponentInChildren<Button>(); // 假设RouteItem prefab包含一个Button
            if (selectButton != null)
            {
                selectButton.onClick.AddListener(() => OnRouteItemSelected(data.RouteID));
            }
        }
        else
        {
            Debug.LogError("RouteItemPrefab does not have a RouteItem component or its children do not have a Button!");
            Destroy(routeItemGO);
        }
    }

    /// <summary>
    /// 更新UI上指定的路由条目
    /// </summary>
    /// <param name="newData">更新后的路由数据</param>
    public void UpdateRouteEntryUI(RouteItemData newData)
    {
        RouteItem existingItem = _currentRouteItemUIs.FirstOrDefault(item => item.RoutesID == newData.RouteID);
        if (existingItem != null)
        {
            existingItem.Initialize(newData); // 更新UI显示
            Debug.Log($"Updated UI for route: {newData.RouteID}");
        }
        else
        {
            Debug.LogWarning($"RouteItem with ID {newData.RouteID} not found in UI list for update.");
        }
    }

    /// <summary>
    /// 从UI上移除指定的路由条目
    /// </summary>
    /// <param name="routeID">要移除的路由条目的ID</param>
    public void RemoveRouteEntryUI(string routeID)
    {
        RouteItem itemToRemove = _currentRouteItemUIs.FirstOrDefault(item => item.RoutesID == routeID);
        if (itemToRemove != null)
        {
            _currentRouteItemUIs.Remove(itemToRemove);
            Destroy(itemToRemove.gameObject);
            Debug.Log($"Removed UI for route: {routeID}");
            // 如果删除的是当前选中的条目，清空输入框并重置选中状态
            if (_selectedRouteIDForEdit == routeID)
            {
                ClearInputFields();
                _selectedRouteIDForEdit = null;
            }
        }
        else
        {
            Debug.LogWarning($"RouteItem with ID {routeID} not found in UI list for removal.");
        }
    }

    /// <summary>
    /// 当用户点击某个路由条目时，将该条目的数据填充到输入字段中
    /// </summary>
    /// <param name="routeID">被选中的路由条目的ID</param>
    public void OnRouteItemSelected(string routeID)
    {
        RouteItem selectedItem = _currentRouteItemUIs.FirstOrDefault(item => item.RoutesID == routeID);
        if (selectedItem != null)
        {
            targetNetworkInput.text = selectedItem.TargetNetwork;
            subnetMaskInput.text = selectedItem.SubnetMask;
            nextHopInput.text = selectedItem.NextHop;
            outInterfaceInput.text = selectedItem.OutInterface;
            metricInput.text = selectedItem.Metric.ToString();
            _selectedRouteIDForEdit = routeID; // 记录当前选中的ID
        }
    }

    /// <summary>
    /// 清空所有输入字段
    /// </summary>
    public void ClearInputFields()
    {
        targetNetworkInput.text = "";
        subnetMaskInput.text = "";
        nextHopInput.text = "";
        outInterfaceInput.text = "";
        metricInput.text = "";
        _selectedRouteIDForEdit = null; // 清空选中ID
    }

    // --- 按钮点击事件处理 ---

    private void OnAddRouteButtonClicked()
    {
        RouteItemData newData = CollectRouteDataFromInputs(true); // true 表示是添加操作，生成新ID
        if (newData != null)
        {
            OnAddRouteRequested?.Invoke(newData);
            ClearInputFields(); // 添加后清空输入
        }
    }

    private void OnUpdateRouteButtonClicked()
    {
        if (string.IsNullOrEmpty(_selectedRouteIDForEdit))
        {
            Debug.LogWarning("Please select a route to update.");
            return;
        }

        RouteItemData updatedData = CollectRouteDataFromInputs(false); // false 表示是更新操作，使用现有ID
        if (updatedData != null)
        {
            updatedData.RouteID = _selectedRouteIDForEdit; // 确保更新时使用正确的RouteID
            OnUpdateRouteRequested?.Invoke(_selectedRouteIDForEdit, updatedData);
            ClearInputFields(); // 更新后清空输入
        }
    }

    private void OnDeleteRouteButtonClicked()
    {
        if (string.IsNullOrEmpty(_selectedRouteIDForEdit))
        {
            Debug.LogWarning("Please select a route to delete.");
            return;
        }

        OnDeleteRouteRequested?.Invoke(_selectedRouteIDForEdit);
        ClearInputFields(); // 删除后清空输入
    }

    /// <summary>
    /// 从输入字段收集路由数据
    /// </summary>
    /// <param name="generateNewID">是否生成新的RouteID</param>
    /// <returns>构建的RouteItemData对象，如果输入无效则返回null</returns>
    private RouteItemData CollectRouteDataFromInputs(bool generateNewID)
    {
        // 简单的输入验证
        if (string.IsNullOrWhiteSpace(targetNetworkInput.text) ||
            string.IsNullOrWhiteSpace(subnetMaskInput.text) ||
            string.IsNullOrWhiteSpace(nextHopInput.text) ||
            string.IsNullOrWhiteSpace(outInterfaceInput.text) ||
            string.IsNullOrWhiteSpace(metricInput.text))
        {
            Debug.LogError("所有路由字段都必须填写！");
            return null;
        }

        if (!int.TryParse(metricInput.text, out int metricValue))
        {
            Debug.LogError("度量（Metric）必须是有效的整数！");
            return null;
        }

        return new RouteItemData
        {
            TargetNetwork = targetNetworkInput.text,
            SubnetMask = subnetMaskInput.text,
            NextHop = nextHopInput.text,
            OutInterface = outInterfaceInput.text,
            Metric = metricValue,
            RouteID = generateNewID ? Guid.NewGuid().ToString() : _selectedRouteIDForEdit // 添加时生成新ID，更新时使用选中ID
        };
    }

    public void ShowPanel()
    {
        gameObject.SetActive(true);
    }

    public void HidePanel()
    {
        gameObject.SetActive(false);
    }
}
