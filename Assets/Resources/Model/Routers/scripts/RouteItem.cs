using Palmmedia.ReportGenerator.Core.Parser.Analysis; // 确认这个引用是否需要，如果不需要可以删除
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[Serializable]
public class RouteItemData
{
    public string TargetNetwork; // 目标网络地址 (e.g., "192.168.2.0")
    public string SubnetMask;   // 目标网络的子网掩码 (e.g., "255.255.255.0")
    public string NextHop;      // 下一跳地址 (e.g., "192.168.1.1")
    public string OutInterface; // 出接口名称 (e.g., "Ethernet0/1")
    public int Metric;          // 路由度量，越小越优先 (e.g., 10, 100)
    public string RouteID;      // 唯一标识这条路由条目
}

[Serializable]
public class RouteItemListWrapper
{
    public List<RouteItemData> Items;
}

public class RouteItem : MonoBehaviour
{
    [Header("展示信息")]
    [SerializeField] private TextMeshProUGUI targetNetwork;
    [SerializeField] private TextMeshProUGUI nextJump; // 更名为 nextHopText，但这里保留以匹配旧代码
    [SerializeField] private TextMeshProUGUI subnetMaskText; // 新增
    [SerializeField] private TextMeshProUGUI outInterface;
    [SerializeField] private TextMeshProUGUI metricText; // 新增

    private string routesID; // 当前路由的key

    public string TargetNetwork
    {
        get { return targetNetwork.text; }
        set { targetNetwork.text = value; }
    }

    public string SubnetMask
    { // 新增
        get { return subnetMaskText.text; }
        set { subnetMaskText.text = value; }
    }

    public string NextHop // 更名为 NextHop
    {
        get { return nextJump.text; }
        set { nextJump.text = value; }
    }

    public string OutInterface
    {
        set { outInterface.text = value; }
        get { return outInterface.text; }
    }

    public int Metric
    { // 新增
        get
        {
            if (int.TryParse(metricText.text, out int result))
            {
                return result;
            }
            return 0; // 默认值或错误处理
        }
        set { metricText.text = value.ToString(); }
    }

    public string RoutesID // 修改为 RoutesID 属性
    {
        set { routesID = value; }
        get { return routesID; }
    }

    public RouteItem() { }

    public RouteItem(RouteItemData data)
    {
        Initialize(data);
    }

    /// <summary>
    /// 初始化数据
    /// </summary>
    /// <param name="data"></param>
    public void Initialize(RouteItemData data)
    {
        TargetNetwork = data.TargetNetwork;
        SubnetMask = data.SubnetMask;
        NextHop = data.NextHop;
        OutInterface = data.OutInterface;
        Metric = data.Metric;
        RoutesID = data.RouteID; // 确保RoutesID被初始化
    }
}
