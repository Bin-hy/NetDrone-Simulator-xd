using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[Serializable]
public class RouterInterfaceData
{
    public string InterfaceName;
    public string IPAddress;    // e.g., "192.168.1.1"
    public string SubnetMask;   // e.g., "255.255.255.0"
}

[Serializable]
public class RouterCoreStateData
{
    public int directSubNetworkCount;
    public string RouterID;
    public List<RouterInterfaceData> Interfaces; // 新增：路由器的接口信息
}

public class RouterCoreState : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI routerIDText;
    [SerializeField] private TextMeshProUGUI directSubNetworkCountText;

    // 路由器的接口列表，这里只用于显示，实际数据在RouterData中
    // [SerializeField] private TextMeshProUGUI interfacesText; // 如果需要显示所有接口到一个Text中

    public string RouterID
    {
        set
        {
            if (routerIDText != null)
            {
                routerIDText.text = value;
            }
        }
        get { return routerIDText != null ? routerIDText.text : string.Empty; }
    }

    public int DirectSubNetworkCount
    {
        set
        {
            if (directSubNetworkCountText != null)
            {
                directSubNetworkCountText.text = value.ToString();
            }
        }
        get
        {
            if (directSubNetworkCountText != null && int.TryParse(directSubNetworkCountText.text, out int count))
            {
                return count;
            }
            return 0;
        }
    }

    public void Initialize(RouterCoreStateData data)
    {
        RouterID = data.RouterID;
        DirectSubNetworkCount = data.directSubNetworkCount;
        // 如果需要显示接口信息，可以在这里处理
        // string interfacesInfo = "";
        // if (data.Interfaces != null) {
        //     foreach (var iface in data.Interfaces) {
        //         interfacesInfo += $"{iface.InterfaceName}: {iface.IPAddress}/{iface.SubnetMask}\n";
        //     }
        // }
        // if (interfacesText != null) {
        //     interfacesText.text = interfacesInfo;
        // }
    }
}