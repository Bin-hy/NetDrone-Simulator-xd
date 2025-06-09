using System;
using UnityEngine;

[Serializable]
public class RoutingEntry
{
    public IPAddress DestinationNetwork { get; private set; } // 目标网络地址
    public IPAddress SubnetMask { get; private set; }       // 子网掩码
    public string OutInterfaceName { get; private set; }      // 出接口名称 (例如: "Ethernet0", "WLAN1")
    public IPAddress NextHop { get; private set; }           // 下一跳地址 (如果是直连，则为 null 或特殊值)
    public int Metric { get; private set; }                 // 路由度量 (例如跳数)

    // 构造函数用于直连路由
    public RoutingEntry(IPAddress destinationNetwork, IPAddress subnetMask, string outInterfaceName)
    {
        DestinationNetwork = destinationNetwork;
        SubnetMask = subnetMask;
        OutInterfaceName = outInterfaceName;
        NextHop = null; // 直连路由没有下一跳
        Metric = 0;     // 直连度量为0
    }

    // 构造函数用于非直连路由
    public RoutingEntry(IPAddress destinationNetwork, IPAddress subnetMask, string outInterfaceName, IPAddress nextHop, int metric)
    {
        DestinationNetwork = destinationNetwork;
        SubnetMask = subnetMask;
        OutInterfaceName = outInterfaceName;
        NextHop = nextHop;
        Metric = metric;
    }

    public override string ToString()
    {
        return $"目标网络: {DestinationNetwork}/{SubnetMask}, 出接口: {OutInterfaceName}, 下一跳: {(NextHop != null ? NextHop.ToString() : "直连")}, 度量: {Metric}";
    }

    // 判断目标IP是否匹配此路由条目
    public bool Matches(IPAddress targetIP)
    {
        return targetIP.GetNetworkAddress(SubnetMask).Equals(DestinationNetwork);
    }
}