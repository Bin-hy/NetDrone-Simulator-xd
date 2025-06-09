using System.Collections.Generic;
using UnityEngine;

public class Switch : NetworkDevice
{
    public List<NetworkDevice> ConnectedDevices { get; private set; } = new List<NetworkDevice>(); // 直连的设备

    public override void Initialize(string name, string ip, string mask)
    {
        DeviceName = name;
        SetIPAddress(ip, mask); // 交换机一般没有IP，这里简化为方便管理
        gameObject.name = $"Switch_{DeviceName}";
    }

    public void AddConnectedDevice(NetworkDevice device)
    {
        if (ConnectedDevices.Count >= 3)
        {
            Debug.LogWarning($"Switch {DeviceName} already has maximum connected devices (3). Cannot add {device.DeviceName}.");
            return;
        }
        ConnectedDevices.Add(device);
        device.ParentSwitch = this;
        device.ParentRouter = ParentRouter; // 设备也知道它上层的路由器
        Debug.Log($"{device.DeviceName} 连接到交换机 {DeviceName}");
    }

    // 交换机转发：在同一子网内直连设备之间进行转发
    public bool TryForwardWithinLocalNetwork(IPAddress sourceIP, IPAddress destinationIP, List<string> routeSteps)
    {
        // 检查目标IP是否直连到此交换机
        foreach (var device in ConnectedDevices)
        {
            if (device.IPAddress.Equals(destinationIP))
            {
                routeSteps.Add($"数据包从 {sourceIP} 到达交换机 {DeviceName}，直接转发到直连设备 {device.DeviceName} ({device.IPAddress})。");
                return true;
            }
        }
        return false;
    }
}