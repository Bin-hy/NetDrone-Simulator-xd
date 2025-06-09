using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class RoutingService : MonoBehaviour
{
    #region Singleton
    private static RoutingService _instance;
    public static RoutingService Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<RoutingService>();
                if (_instance == null)
                {
                    GameObject obj = new GameObject("RoutingService");
                    _instance = obj.AddComponent<RoutingService>();
                    DontDestroyOnLoad(obj);
                }
            }
            return _instance;
        }
    }
    #endregion

    /// <summary>
    /// 尝试在两个设备之间建立连接并返回路由路径。
    /// </summary>
    /// <param name="sourceDevice">源设备</param>
    /// <param name="destinationDevice">目标设备</param>
    /// <param name="routePath">输出参数：如果成功，返回路由步骤列表</param>
    /// <returns>连接是否成功</returns>
    public bool TryConnectDevices(EndPointDevice sourceDevice, EndPointDevice destinationDevice, out List<RouteStep> routePath)
    {
        routePath = new List<RouteStep>(); // 正确地初始化了 out 参数 routePath

        if (sourceDevice == null || destinationDevice == null)
        {
            Debug.LogError("Source or destination device is null.");
            return false;
        }

        string sourceIP = sourceDevice.endpontBaseData.network; // 假设 network 字段存储了设备的 IP 地址
        string destinationIP = destinationDevice.endpontBaseData.network; // 假设 network 字段存储了设备的 IP 地址

        Debug.Log($"尝试从 {sourceIP} 连接到 {destinationIP}");

        // 1. 判断是否在同一个子网 (直接连接)
        if (IsInSameSubnet(sourceIP, sourceDevice.endpontBaseData.subMask, destinationIP, destinationDevice.endpontBaseData.subMask))
        {
            routePath.Add(new RouteStep
            {
                RouterID = "N/A", // 对于直连，没有特定的路由器
                IncomingInterface = "N/A",
                OutgoingInterface = "N/A",
                NextHop = "直连",
                Destination = destinationIP,
                Action = "直接转发",
                Reason = "目标在同一子网"
            });
            Debug.Log($"设备 {sourceIP} 和 {destinationIP} 位于同一子网，直接连接成功。");
            return true;
        }

        // 2. 否则，需要通过路由器转发 (查找路由表)
        // 从源设备所属网络找到最近的路由器（如果设备在子网中）
        // 这里需要一个机制来获取源设备连接的路由器
        // 假设每个子网都连接到一个或多个路由器，或者设备本身能够触发路由过程
        // 简单起见，我们假设路由从源设备所在网络的路由器开始
        RouterController currentRouter = FindRouterConnectedToDevice(sourceDevice);

        if (currentRouter == null)
        {
            Debug.LogError($"无法找到连接到源设备 {sourceIP} 的路由器。");
            routePath.Add(new RouteStep { Action = "丢弃", Reason = "无法找到源设备连接的路由器" });
            return false;
        }

        // 使用 BFS 或 Dijkstra 算法寻找最短路径
        Queue<Tuple<RouterController, List<RouteStep>>> queue = new Queue<Tuple<RouterController, List<RouteStep>>>();
        HashSet<string> visitedRouters = new HashSet<string>();

        // 初始状态：从源设备连接的路由器开始
        List<RouteStep> initialPath = new List<RouteStep>();
        // 添加一个步骤，表示数据包进入第一个路由器
        initialPath.Add(new RouteStep
        {
            RouterID = currentRouter.RouterID,
            IncomingInterface = "N/A", // 假设数据包进入路由器没有特定接口（取决于实现）
            OutgoingInterface = "N/A",
            NextHop = "N/A",
            Destination = destinationIP,
            Action = "开始路由",
            Reason = "数据包到达路由器"
        });
        queue.Enqueue(Tuple.Create(currentRouter, initialPath));
        visitedRouters.Add(currentRouter.RouterID);

        while (queue.Any())
        {
            Tuple<RouterController, List<RouteStep>> currentTuple = queue.Dequeue();
            RouterController router = currentTuple.Item1;
            List<RouteStep> currentPath = currentTuple.Item2;

            Debug.Log($"当前路由器: {router.RouterID}");

            // 查找路由表
            List<RouteItemData> routerRoutes = router.GetRouterData()?.routes?.Items;

            if (routerRoutes == null || !routerRoutes.Any())
            {
                Debug.LogWarning($"路由器 {router.RouterID} 没有路由表。");
                // 无法继续路由，记录为丢弃并继续下一个路径（如果存在）
                routePath = new List<RouteStep>(currentPath); // 记录当前失败路径
                routePath.Add(new RouteStep
                {
                    RouterID = router.RouterID,
                    Action = "丢弃",
                    Reason = "路由器无路由表"
                });
                continue; // 尝试队列中的下一个路径
            }

            // 找到最长匹配的路由
            RouteItemData bestMatchRoute = FindLongestMatchRoute(routerRoutes, destinationIP);

            if (bestMatchRoute != null)
            {
                Debug.Log($"路由器 {router.RouterID} 找到最佳匹配路由: 目标网络 {bestMatchRoute.TargetNetwork}, 下一跳 {bestMatchRoute.NextHop}, 出接口 {bestMatchRoute.OutInterface}");

                // 如果下一跳是“直连”，则检查目标是否在出接口所在的子网
                if (bestMatchRoute.NextHop == "直连")
                {
                    // 获取当前路由器出接口的IP和子网掩码
                    RouterInterfaceData outInterfaceData = router.GetRouterData()?.routerCoreStateData?.Interfaces
                        .FirstOrDefault(i => i.InterfaceName == bestMatchRoute.OutInterface);

                    if (outInterfaceData != null && IsInSameSubnet(destinationIP, destinationDevice.endpontBaseData.subMask, outInterfaceData.IPAddress, outInterfaceData.SubnetMask))
                    {
                        List<RouteStep> successPath = new List<RouteStep>(currentPath);
                        successPath.Add(new RouteStep
                        {
                            RouterID = router.RouterID,
                            IncomingInterface = "N/A", // 取决于之前步骤的OutgoingInterface
                            OutgoingInterface = bestMatchRoute.OutInterface,
                            NextHop = "直连",
                            Destination = destinationIP,
                            Action = "直接转发",
                            Reason = "目标直连"
                        });
                        routePath = successPath; // 找到成功路径并赋值给 out 参数
                        Debug.Log($"成功找到从 {sourceIP} 到 {destinationIP} 的路径。");
                        return true;
                    }
                    else
                    {
                        Debug.LogWarning($"路由器 {router.RouterID} 的直连路由 {bestMatchRoute.TargetNetwork} 未能将数据包转发到 {destinationIP}，目标不在出接口子网。");
                        // 记录丢弃步骤
                        List<RouteStep> failedPath = new List<RouteStep>(currentPath);
                        failedPath.Add(new RouteStep
                        {
                            RouterID = router.RouterID,
                            IncomingInterface = "N/A",
                            OutgoingInterface = bestMatchRoute.OutInterface,
                            NextHop = "直连",
                            Destination = destinationIP,
                            Action = "丢弃",
                            Reason = "直连路由目标不在出接口子网"
                        });
                        routePath = failedPath; // 记录当前失败路径
                        continue;
                    }
                }
                else // 下一跳是另一个路由器
                {
                    RouterController nextRouter = FindRouterByInterfaceIP(bestMatchRoute.NextHop, bestMatchRoute.OutInterface);

                    if (nextRouter != null && !visitedRouters.Contains(nextRouter.RouterID))
                    {
                        List<RouteStep> newPath = new List<RouteStep>(currentPath);
                        newPath.Add(new RouteStep
                        {
                            RouterID = router.RouterID,
                            IncomingInterface = "N/A",
                            OutgoingInterface = bestMatchRoute.OutInterface,
                            NextHop = bestMatchRoute.NextHop,
                            Destination = destinationIP,
                            Action = "转发到下一跳",
                            Reason = "通过路由表转发"
                        });
                        queue.Enqueue(Tuple.Create(nextRouter, newPath));
                        visitedRouters.Add(nextRouter.RouterID);
                    }
                    else if (nextRouter != null && visitedRouters.Contains(nextRouter.RouterID))
                    {
                        Debug.LogWarning($"检测到路由循环或重复访问路由器 {nextRouter.RouterID}。");
                        // 记录循环或重复访问，并标记为失败
                        List<RouteStep> loopPath = new List<RouteStep>(currentPath);
                        loopPath.Add(new RouteStep
                        {
                            RouterID = router.RouterID,
                            IncomingInterface = "N/A",
                            OutgoingInterface = bestMatchRoute.OutInterface,
                            NextHop = bestMatchRoute.NextHop,
                            Destination = destinationIP,
                            Action = "丢弃",
                            Reason = "检测到路由循环或重复访问"
                        });
                        routePath = loopPath; // 记录当前失败路径
                        continue;
                    }
                    else
                    {
                        Debug.LogWarning($"路由器 {router.RouterID} 无法找到下一跳路由器 {bestMatchRoute.NextHop}。");
                        // 记录丢弃步骤
                        List<RouteStep> failedPath = new List<RouteStep>(currentPath);
                        failedPath.Add(new RouteStep
                        {
                            RouterID = router.RouterID,
                            IncomingInterface = "N/A",
                            OutgoingInterface = bestMatchRoute.OutInterface,
                            NextHop = bestMatchRoute.NextHop,
                            Destination = destinationIP,
                            Action = "丢弃",
                            Reason = "找不到下一跳路由器"
                        });
                        routePath = failedPath; // 记录当前失败路径
                        continue;
                    }
                }
            }
            else
            {
                Debug.LogWarning($"路由器 {router.RouterID} 没有找到匹配 {destinationIP} 的路由条目。");
                // 没有找到匹配路由，数据包被丢弃
                List<RouteStep> failedPath = new List<RouteStep>(currentPath);
                failedPath.Add(new RouteStep
                {
                    RouterID = router.RouterID,
                    IncomingInterface = "N/A",
                    OutgoingInterface = "N/A",
                    NextHop = "N/A",
                    Destination = destinationIP,
                    Action = "丢弃",
                    Reason = "无匹配路由"
                });
                routePath = failedPath; // 记录当前失败路径
                continue;
            }
        }

        // 如果队列为空，但仍未找到路径，则表示连接失败
        Debug.LogWarning($"无法从 {sourceIP} 连接到 {destinationIP}。");
        // routePath 在函数开头已经被初始化，并会在每次失败时更新为当前路径，
        // 如果最终未找到成功路径，routePath 将保留最后一次尝试的失败路径。
        return false;
    }

    /// <summary>
    /// 判断两个IP地址是否在同一个子网内。
    /// </summary>
    private bool IsInSameSubnet(string ip1, string subnetMask1, string ip2, string subnetMask2)
    {
        // 假设子网掩码相同且有效
        if (subnetMask1 != subnetMask2)
        {
            Debug.LogWarning($"IP地址 {ip1} 和 {ip2} 所在的子网掩码不一致。");
            return false;
        }

        uint ip1Uint = IpAddressToUint(ip1);
        uint ip2Uint = IpAddressToUint(ip2);
        uint maskUint = IpAddressToUint(subnetMask1);

        if (maskUint == 0)
        {
            Debug.LogError("无效的子网掩码，无法进行子网判断。");
            return false;
        }

        return (ip1Uint & maskUint) == (ip2Uint & maskUint);
    }

    /// <summary>
    /// 将IP地址字符串转换为无符号整型。
    /// </summary>
    private uint IpAddressToUint(string ipAddress)
    {
        string[] octets = ipAddress.Split('.');
        uint ipUint = 0;
        if (octets.Length == 4)
        {
            for (int i = 0; i < 4; i++)
            {
                if (uint.TryParse(octets[i], out uint octetValue))
                {
                    ipUint |= (octetValue << (32 - (i + 1) * 8));
                }
                else
                {
                    Debug.LogError($"无效的IP地址八位字节: {octets[i]}");
                    return 0; // 返回0表示错误或无效IP
                }
            }
        }
        else
        {
            Debug.LogError($"无效的IP地址格式: {ipAddress}");
            return 0; // 返回0表示错误或无效IP
        }
        return ipUint;
    }

    /// <summary>
    /// 寻找连接到给定设备的路由器。
    /// 此方法需要根据您的网络拓扑和设备与路由器的关联方式进行具体实现。
    /// </summary>
    private RouterController FindRouterConnectedToDevice(EndPointDevice device)
    {
        // 这是一个简化实现，假设每个子网都通过一个路由器连接
        // 您可能需要更复杂的逻辑，例如遍历所有路由器及其接口，
        // 判断哪个路由器的接口与设备的IP在同一子网。
        string deviceNetworkAddress = device.endpontBaseData.network;
        string deviceSubnetMask = device.endpontBaseData.subMask;

        foreach (RouterController router in RouterManager.Instance.AllRouters)
        {
            RouterData routerData = router.GetRouterData();
            if (routerData != null && routerData.routerCoreStateData != null && routerData.routerCoreStateData.Interfaces != null)
            {
                foreach (var iface in routerData.routerCoreStateData.Interfaces)
                {
                    // 检查路由器接口的IP是否与设备在同一子网
                    if (IsInSameSubnet(deviceNetworkAddress, deviceSubnetMask, iface.IPAddress, iface.SubnetMask))
                    {
                        return router;
                    }
                }
            }
        }
        return null; // 未找到连接到该设备的路由器
    }

    /// <summary>
    /// 在路由器的路由表中找到与目标IP最长匹配的路由条目。
    /// </summary>
    private RouteItemData FindLongestMatchRoute(List<RouteItemData> routes, string destinationIP)
    {
        RouteItemData bestMatch = null;
        int longestMatchCidr = -1;

        uint destinationIpUint = IpAddressToUint(destinationIP);

        foreach (var route in routes)
        {
            // 确保 TargetNetwork 是一个网络地址，而不是主机地址
            // 并且 SubnetMask 是有效的
            uint targetNetworkUint = IpAddressToUint(route.TargetNetwork);
            string subnetMaskString = route.SubnetMask;
            int currentCidr = ConvertSubnetMaskToCidr(subnetMaskString);

            if (currentCidr == -1)
            {
                Debug.LogWarning($"路由条目 {route.TargetNetwork}/{route.SubnetMask} 的子网掩码无效。");
                continue;
            }

            uint maskUint = GetSubnetMaskUintFromCidr(currentCidr);

            // 检查目标IP是否匹配该路由条目的目标网络
            if ((destinationIpUint & maskUint) == (targetNetworkUint & maskUint))
            {
                if (currentCidr > longestMatchCidr)
                {
                    longestMatchCidr = currentCidr;
                    bestMatch = route;
                }
            }
        }
        return bestMatch;
    }

    /// <summary>
    /// 根据下一跳IP地址和出接口名称查找路由器。
    /// (这个辅助方法需要改进，因为下一跳路由器不一定能通过其某个接口IP精确识别，
    /// 而是需要通过该接口连接到的网络来识别。)
    /// 暂时假设下一跳IP就是下一跳路由器的某个接口IP。
    /// </summary>
    private RouterController FindRouterByInterfaceIP(string nextHopIP, string outInterfaceName)
    {
        foreach (RouterController router in RouterManager.Instance.AllRouters)
        {
            RouterData routerData = router.GetRouterData();
            if (routerData != null && routerData.routerCoreStateData != null && routerData.routerCoreStateData.Interfaces != null)
            {
                // 检查该路由器的接口是否与nextHopIP和outInterfaceName匹配
                // 注意：这里的 outInterfaceName 是当前路由器的出接口名称，不是下一跳路由器的入接口名称。
                // 更好的做法是 nextHopIP 直接是下一跳路由器的某个接口IP
                if (routerData.routerCoreStateData.Interfaces.Any(iface => iface.IPAddress == nextHopIP))
                {
                    return router;
                }
            }
        }
        return null;
    }


    // 将子网掩码字符串转换为CIDR
    private int ConvertSubnetMaskToCidr(string subnetMaskString)
    {
        if (string.IsNullOrEmpty(subnetMaskString)) return -1;

        uint mask = 0;
        string[] octets = subnetMaskString.Split('.');
        if (octets.Length != 4) return -1;

        try
        {
            for (int i = 0; i < 4; i++)
            {
                mask |= (uint.Parse(octets[i]) << (32 - (i + 1) * 8));
            }
        }
        catch (FormatException)
        {
            Debug.LogError($"无效的子网掩码格式: {subnetMaskString}");
            return -1;
        }

        int cidr = 0;
        while ((mask & 0x80000000) != 0 && cidr < 32)
        {
            mask <<= 1;
            cidr++;
        }
        return cidr;
    }

    // 根据CIDR获取子网掩码的无符号整型表示
    private uint GetSubnetMaskUintFromCidr(int cidr)
    {
        if (cidr < 0 || cidr > 32) return 0;
        return ~(0xFFFFFFFF >> cidr);
    }

    // 将无符号整型IP地址转换回字符串（用于调试）
    private string UintToIpAddress(uint ipUint)
    {
        return $"{((ipUint >> 24) & 0xFF)}.{((ipUint >> 16) & 0xFF)}.{((ipUint >> 8) & 0xFF)}.{((ipUint) & 0xFF)}";
    }
}