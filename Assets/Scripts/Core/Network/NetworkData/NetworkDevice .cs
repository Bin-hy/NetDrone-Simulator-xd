using UnityEngine;

public abstract class NetworkDevice : MonoBehaviour
{
    public string DeviceName { get; protected set; }
    public IPAddress IPAddress { get; protected set; }
    public IPAddress SubnetMask { get; protected set; }
    public Network ParentNetwork { get; set; } // 所属的网络
    public Router ParentRouter { get; set; } // 所属的路由器（对于交换机和设备）
    public Switch ParentSwitch { get; set; } // 所属的交换机（对于设备）

    public virtual void SetIPAddress(string ipAddress, string subnetMask)
    {
        IPAddress = new IPAddress(ipAddress);
        SubnetMask = new IPAddress(subnetMask);
    }

    public abstract void Initialize(string name, string ip, string mask);
}