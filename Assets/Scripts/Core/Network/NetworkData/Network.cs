using System.Collections.Generic;
using UnityEngine;

public class Network : MonoBehaviour
{
    public string NetworkName { get; private set; }
    public IPAddress NetworkAddress { get; private set; }
    public IPAddress SubnetMask { get; private set; }

    public Router Router { get; private set; }
    public List<Switch> Switches { get; private set; } = new List<Switch>();
    public List<NetworkDevice> Devices { get; private set; } = new List<NetworkDevice>(); // 包括交换机和设备

    public void Initialize(string name, string networkAddress, string subnetMask)
    {
        NetworkName = name;
        NetworkAddress = new IPAddress(networkAddress);
        SubnetMask = new IPAddress(subnetMask);
        gameObject.name = $"Network_{NetworkName}";
    }

    public void AddRouter(Router router)
    {
        Router = router;
        router.ParentNetwork = this;
        Devices.Add(router);
    }

    public void AddSwitch(Switch sw)
    {
        Switches.Add(sw);
        sw.ParentNetwork = this;
        sw.ParentRouter = Router; // 交换机直连路由器
        Devices.Add(sw);
    }

    public void AddDevice(NetworkDevice device)
    {
        Devices.Add(device);
        device.ParentNetwork = this;
    }

    public NetworkDevice GetDeviceByName(string deviceName)
    {
        foreach (var device in Devices)
        {
            if (device.DeviceName == deviceName)
            {
                return device;
            }
        }
        return null;
    }
}