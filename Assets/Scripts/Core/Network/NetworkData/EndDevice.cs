using UnityEngine;

public class EndDevice : NetworkDevice
{
    public override void Initialize(string name, string ip, string mask)
    {
        DeviceName = name;
        SetIPAddress(ip, mask);
        gameObject.name = $"Device_{DeviceName}";
    }
}