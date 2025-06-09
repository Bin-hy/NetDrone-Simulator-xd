using System.Collections.Generic;
using UnityEngine;

public class Router : NetworkDevice
{
    public List<RoutingEntry> RoutingTable { get; private set; } = new List<RoutingEntry>();
    public List<string> Interfaces { get; private set; } = new List<string>(); // 路由器接口，例如 "Ethernet0"

    public override void Initialize(string name, string ip, string mask)
    {
        DeviceName = name;
        SetIPAddress(ip, mask); // 路由器通常每个接口有IP，这里简化为代表整个路由器的IP
        gameObject.name = $"Router_{DeviceName}";
        // 假设路由器有多个接口，这里只模拟一个，实际可以扩展
        Interfaces.Add("LocalInterface"); // 模拟一个内部接口
    }

    // 初始化本地直连路由
    public void AddDirectlyConnectedRoute(IPAddress networkAddress, IPAddress subnetMask, string interfaceName)
    {
        RoutingTable.Add(new RoutingEntry(networkAddress, subnetMask, interfaceName));
        Debug.Log($"{DeviceName} 添加直连路由: {networkAddress}/{subnetMask} 经由 {interfaceName}");
    }

    // 用户编辑路由表：添加非直连路由
    public void AddRoute(IPAddress destinationNetwork, IPAddress subnetMask, string outInterfaceName, IPAddress nextHop, int metric)
    {
        // 检查是否已存在相同目标网络的路由，如果存在则更新，否则添加
        RoutingEntry existingEntry = RoutingTable.Find(
            e => e.DestinationNetwork.Equals(destinationNetwork) && e.SubnetMask.Equals(subnetMask)
        );

        if (existingEntry != null)
        {
            // 更新现有路由条目
            existingEntry = new RoutingEntry(destinationNetwork, subnetMask, outInterfaceName, nextHop, metric);
            Debug.Log($"{DeviceName} 更新路由: {destinationNetwork}/{subnetMask} 经由 {outInterfaceName}, 下一跳: {nextHop}, 度量: {metric}");
        }
        else
        {
            // 添加新路由条目
            RoutingTable.Add(new RoutingEntry(destinationNetwork, subnetMask, outInterfaceName, nextHop, metric));
            Debug.Log($"{DeviceName} 添加路由: {destinationNetwork}/{subnetMask} 经由 {outInterfaceName}, 下一跳: {nextHop}, 度量: {metric}");
        }
        // 路由表排序：通常按子网掩码长度或度量排序，这里简化为添加即可
        RoutingTable.Sort((a, b) => b.SubnetMask.AddressBytes.Length.CompareTo(a.SubnetMask.AddressBytes.Length)); // 优先匹配更精确的路由
    }

    // 根据目标IP查找最佳路由
    public RoutingEntry GetBestRoute(IPAddress targetIP)
    {
        RoutingEntry bestMatch = null;
        // 遍历路由表，找到最匹配的路由（最长匹配原则）
        foreach (var entry in RoutingTable)
        {
            if (entry.Matches(targetIP))
            {
                if (bestMatch == null || entry.SubnetMask.AddressBytes.Length > bestMatch.SubnetMask.AddressBytes.Length)
                {
                    bestMatch = entry;
                }
            }
        }
        return bestMatch;
    }
}