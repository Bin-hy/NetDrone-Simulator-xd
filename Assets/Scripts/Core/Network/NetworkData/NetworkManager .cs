using System.Collections.Generic;
using UnityEngine;
using System.Linq; // for Linq usage

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Instance { get; private set; }

    public List<Network> Networks { get; private set; } = new List<Network>();
    public List<Router> Routers { get; private set; } = new List<Router>();
    public List<Switch> Switches { get; private set; } = new List<Switch>();
    public List<EndDevice> EndDevices { get; private set; } = new List<EndDevice>();
    public List<NetworkDevice> AllDevices { get; private set; } = new List<NetworkDevice>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // --- 初始化网络拓扑 ---
    public void InitializeNetworkTopology()
    {
        // 清除旧数据 (如果多次调用)
        Networks.Clear();
        Routers.Clear();
        Switches.Clear();
        EndDevices.Clear();
        AllDevices.Clear();

        // 示例：创建多个网络和设备
        // Network 1: 192.168.1.0/24
        var net1 = new GameObject("Network_1").AddComponent<Network>();
        net1.Initialize("Network1", "192.168.1.0", "255.255.255.0");
        Networks.Add(net1);

        var router1 = new GameObject("Router_1").AddComponent<Router>();
        router1.Initialize("R1", "192.168.1.1", "255.255.255.0");
        router1.AddDirectlyConnectedRoute(new IPAddress("192.168.1.0"), new IPAddress("255.255.255.0"), "LocalInterface");
        net1.AddRouter(router1);
        Routers.Add(router1);
        AllDevices.Add(router1);

        var switch1 = new GameObject("Switch_1").AddComponent<Switch>();
        switch1.Initialize("SW1", "192.168.1.254", "255.255.255.0");
        net1.AddSwitch(switch1);
        Switches.Add(switch1);
        AllDevices.Add(switch1);

        var deviceA = new GameObject("Device_A").AddComponent<EndDevice>();
        deviceA.Initialize("PC-A", "192.168.1.10", "255.255.255.0");
        switch1.AddConnectedDevice(deviceA);
        net1.AddDevice(deviceA);
        EndDevices.Add(deviceA);
        AllDevices.Add(deviceA);

        var deviceB = new GameObject("Device_B").AddComponent<EndDevice>();
        deviceB.Initialize("PC-B", "192.168.1.11", "255.255.255.0");
        switch1.AddConnectedDevice(deviceB);
        net1.AddDevice(deviceB);
        EndDevices.Add(deviceB);
        AllDevices.Add(deviceB);

        // Network 2: 192.168.2.0/24
        var net2 = new GameObject("Network_2").AddComponent<Network>();
        net2.Initialize("Network2", "192.168.2.0", "255.255.255.0");
        Networks.Add(net2);

        var router2 = new GameObject("Router_2").AddComponent<Router>();
        router2.Initialize("R2", "192.168.2.1", "255.255.255.0");
        router2.AddDirectlyConnectedRoute(new IPAddress("192.168.2.0"), new IPAddress("255.255.255.0"), "LocalInterface");
        net2.AddRouter(router2);
        Routers.Add(router2);
        AllDevices.Add(router2);

        var switch2 = new GameObject("Switch_2").AddComponent<Switch>();
        switch2.Initialize("SW2", "192.168.2.254", "255.255.255.0");
        net2.AddSwitch(switch2);
        Switches.Add(switch2);
        AllDevices.Add(switch2);

        var deviceC = new GameObject("Device_C").AddComponent<EndDevice>();
        deviceC.Initialize("PC-C", "192.168.2.10", "255.255.255.0");
        switch2.AddConnectedDevice(deviceC);
        net2.AddDevice(deviceC);
        EndDevices.Add(deviceC);
        AllDevices.Add(deviceC);

        // Network 3: 192.168.3.0/24 (用于演示跨网络路由)
        var net3 = new GameObject("Network_3").AddComponent<Network>();
        net3.Initialize("Network3", "192.168.3.0", "255.255.255.0");
        Networks.Add(net3);

        var router3 = new GameObject("Router_3").AddComponent<Router>();
        router3.Initialize("R3", "192.168.3.1", "255.255.255.0");
        router3.AddDirectlyConnectedRoute(new IPAddress("192.168.3.0"), new IPAddress("255.255.255.0"), "LocalInterface");
        net3.AddRouter(router3);
        Routers.Add(router3);
        AllDevices.Add(router3);

        var switch3 = new GameObject("Switch_3").AddComponent<Switch>();
        switch3.Initialize("SW3", "192.168.3.254", "255.255.255.0");
        net3.AddSwitch(switch3);
        Switches.Add(switch3);
        AllDevices.Add(switch3);

        var deviceD = new GameObject("Device_D").AddComponent<EndDevice>();
        deviceD.Initialize("PC-D", "192.168.3.10", "255.255.255.0");
        switch3.AddConnectedDevice(deviceD);
        net3.AddDevice(deviceD);
        EndDevices.Add(deviceD);
        AllDevices.Add(deviceD);


        Debug.Log("网络拓扑初始化完成。");
    }

    // --- 获取设备 ---
    public NetworkDevice GetDeviceByIP(IPAddress ip)
    {
        return AllDevices.FirstOrDefault(d => d.IPAddress != null && d.IPAddress.Equals(ip));
    }

    public NetworkDevice GetDeviceByName(string name)
    {
        return AllDevices.FirstOrDefault(d => d.DeviceName == name);
    }

    // --- 用户编辑路由表 ---
    public void EditRouterRoutingTable(string routerName, string destinationNetwork, string subnetMask, string outInterfaceName, string nextHopIP, int metric)
    {
        Router router = GetDeviceByName(routerName) as Router;
        if (router == null)
        {
            Debug.LogError($"路由器 '{routerName}' 不存在。");
            return;
        }

        IPAddress destNet = new IPAddress(destinationNetwork);
        IPAddress subMask = new IPAddress(subnetMask);
        IPAddress nextHop = null;
        if (!string.IsNullOrEmpty(nextHopIP))
        {
            nextHop = new IPAddress(nextHopIP);
        }

        router.AddRoute(destNet, subMask, outInterfaceName, nextHop, metric);
        Debug.Log($"路由器 {routerName} 的路由表已更新。");
    }

    // --- 判断连接成功与否并显示路由步骤 ---
    public (bool success, List<string> steps) AttemptConnection(string sourceDeviceName, string destinationDeviceName)
    {
        List<string> routeSteps = new List<string>();

        NetworkDevice sourceDevice = GetDeviceByName(sourceDeviceName);
        NetworkDevice destinationDevice = GetDeviceByName(destinationDeviceName);

        if (sourceDevice == null)
        {
            routeSteps.Add($"错误: 源设备 '{sourceDeviceName}' 不存在。");
            return (false, routeSteps);
        }
        if (destinationDevice == null)
        {
            routeSteps.Add($"错误: 目标设备 '{destinationDeviceName}' 不存在。");
            return (false, routeSteps);
        }

        IPAddress sourceIP = sourceDevice.IPAddress;
        IPAddress destinationIP = destinationDevice.IPAddress;

        routeSteps.Add($"尝试从 {sourceDevice.DeviceName} ({sourceIP}) 连接到 {destinationDevice.DeviceName} ({destinationIP})。");

        // 1. 同一交换机下的直连设备
        if (sourceDevice.ParentSwitch != null && sourceDevice.ParentSwitch == destinationDevice.ParentSwitch)
        {
            if (sourceDevice.ParentSwitch.TryForwardWithinLocalNetwork(sourceIP, destinationIP, routeSteps))
            {
                routeSteps.Add("连接成功: 位于同一交换机下。");
                return (true, routeSteps);
            }
            else
            {
                // 虽然在同一交换机下，但TryForwardWithinLocalNetwork可能因为其他原因（例如目标IP不匹配）返回false
                routeSteps.Add($"连接失败: 目标设备 {destinationDevice.DeviceName} 未被交换机 {sourceDevice.ParentSwitch.DeviceName} 识别或无法直连。");
                return (false, routeSteps);
            }
        }

        // 2. 同一网络但不同交换机 (通过路由器)
        Router sourceRouter = sourceDevice.ParentRouter;
        if (sourceRouter == null)
        {
            routeSteps.Add($"错误: 源设备 {sourceDevice.DeviceName} 未连接到路由器。");
            return (false, routeSteps);
        }

        if (sourceDevice.ParentNetwork == destinationDevice.ParentNetwork)
        {
            routeSteps.Add($"数据包从 {sourceDevice.DeviceName} 发送至网关 {sourceRouter.DeviceName} ({sourceRouter.IPAddress})。");

            // 在同一网络内，数据包到达路由器，路由器会根据其路由表判断如何转发
            Router routerInvolved = sourceRouter;

            // 路由器查找直连目标交换机（模拟）
            Switch targetSwitch = Switches.FirstOrDefault(s => s.ParentNetwork == destinationDevice.ParentNetwork && s.ConnectedDevices.Contains(destinationDevice));

            if (targetSwitch != null)
            {
                routeSteps.Add($"路由器 {routerInvolved.DeviceName} 识别目标 {destinationDevice.DeviceName} 在本地网络，转发到交换机 {targetSwitch.DeviceName}。");
                if (targetSwitch.TryForwardWithinLocalNetwork(sourceIP, destinationIP, routeSteps))
                {
                    routeSteps.Add("连接成功: 经过路由器在同一网络内转发。");
                    return (true, routeSteps);
                }
                else
                {
                    routeSteps.Add($"连接失败: 交换机 {targetSwitch.DeviceName} 无法将数据包转发到 {destinationDevice.DeviceName}。");
                    return (false, routeSteps);
                }
            }
            else
            {
                routeSteps.Add($"错误: 路由器 {routerInvolved.DeviceName} 无法找到目标设备 {destinationDevice.DeviceName} 所属的交换机。");
                return (false, routeSteps);
            }
        }
        else // 跨网络路由
        {
            routeSteps.Add($"数据包从 {sourceDevice.DeviceName} 发送至网关 {sourceRouter.DeviceName} ({sourceRouter.IPAddress})，进行跨网络路由。");

            Router currentRouter = sourceRouter;
            int maxHops = 10; // 防止无限循环
            int hops = 0;

            while (currentRouter != null && hops < maxHops)
            {
                hops++;
                RoutingEntry route = currentRouter.GetBestRoute(destinationIP);

                if (route == null)
                {
                    routeSteps.Add($"路由器 {currentRouter.DeviceName} 路由表未找到到达 {destinationIP} 的路径。");
                    return (false, routeSteps); // 找不到路由，直接返回失败
                }

                routeSteps.Add($"路由器 {currentRouter.DeviceName} 找到路由: 目标 {route.DestinationNetwork}/{route.SubnetMask}, 出接口 {route.OutInterfaceName}, 下一跳: {(route.NextHop != null ? route.NextHop.ToString() : "直连")}, 度量: {route.Metric}");

                if (route.NextHop == null) // 直连路由
                {
                    // 检查目标IP是否在当前路由器的直连网络内
                    if (destinationIP.GetNetworkAddress(route.SubnetMask).Equals(route.DestinationNetwork))
                    {
                        // 目标在当前路由器直连的网络中，尝试通过其直连交换机找到设备
                        Switch targetSwitch = Switches.FirstOrDefault(s => s.ParentRouter == currentRouter && s.ConnectedDevices.Contains(destinationDevice));
                        if (targetSwitch != null)
                        {
                            routeSteps.Add($"路由器 {currentRouter.DeviceName} 识别目标 {destinationDevice.DeviceName} 直连，转发到交换机 {targetSwitch.DeviceName}。");
                            if (targetSwitch.TryForwardWithinLocalNetwork(sourceIP, destinationIP, routeSteps))
                            {
                                routeSteps.Add("连接成功: 跨网络路由到达目标。");
                                return (true, routeSteps);
                            }
                            else
                            {
                                routeSteps.Add($"连接失败: 交换机 {targetSwitch.DeviceName} 无法将数据包转发到 {destinationDevice.DeviceName}。");
                                return (false, routeSteps);
                            }
                        }
                        else
                        {
                            routeSteps.Add($"错误: 路由器 {currentRouter.DeviceName} 无法找到目标设备 {destinationDevice.DeviceName} 所属的交换机。");
                            return (false, routeSteps);
                        }
                    }
                    else
                    {
                        routeSteps.Add($"错误: 直连路由 {route.DestinationNetwork} 不匹配目标 {destinationIP}。");
                        return (false, routeSteps);
                    }
                }
                else // 非直连路由，需要转发到下一跳路由器
                {
                    NetworkDevice nextHopDevice = GetDeviceByIP(route.NextHop);
                    if (nextHopDevice is Router nextRouter)
                    {
                        routeSteps.Add($"数据包转发至下一跳路由器 {nextRouter.DeviceName} ({nextRouter.IPAddress})。");
                        currentRouter = nextRouter; // 继续下一跳路由
                        // 循环将继续，无需在此处返回
                    }
                    else
                    {
                        routeSteps.Add($"错误: 下一跳 {route.NextHop} 不是一个已知的路由器或无法到达。");
                        return (false, routeSteps); // 下一跳无效，返回失败
                    }
                }
            }

            // 循环结束但未成功连接的情况
            if (hops >= maxHops)
            {
                routeSteps.Add($"连接失败: 路由跳数达到最大限制 ({maxHops})，可能存在路由循环或路径过长。");
            }
            else // 可能是 currentRouter 变为 null (未找到下一跳路由器)
            {
                routeSteps.Add($"连接失败: 无法到达目标设备 {destinationDevice.DeviceName}。路由过程中断。");
            }
            return (false, routeSteps); // 循环结束后仍未成功，返回失败
        }
    }
}