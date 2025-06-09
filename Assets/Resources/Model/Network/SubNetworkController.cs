using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using UnityEditor.UI;
using UnityEngine;

// 网络信息
public class SubNetworkController : MonoBehaviour
{
    public string networkName = ""; // 和DronesManagerPanel中的networkName 对应
    public string network = "192.168.1.0";
    public string subnetMask= "255.255.255.0";
    
    // 最大终端数
    private int _maxEndpointCount = 3;
    public int maxEndpointCout => _maxEndpointCount;
    // 当前已有的终端数量
    private int _curEndpointCount = 0;
    public int curEndpointCount => _curEndpointCount;
    private bool _isfull = false;
    public bool isFull => _isfull;
    // 挂载的节点位置
    [SerializeField] Transform[] endpointParents;

    // 存储 挂载的节点位置
    private Dictionary<EndPointDevice, Transform > _endpointMap = new Dictionary<EndPointDevice, Transform>();
    public Dictionary<EndPointDevice, Transform> endpointMap => _endpointMap;
    private List<EndPointDevice> _endpointList = new List<EndPointDevice>();
    public List<EndPointDevice> endpointList => _endpointList; // 所有网络下的终端
    public event Action OnEndpointListChanged; // 当列表改变 通知更新事件
    // 将终端设备添加到当前网络中
    public bool addIntoNetwork(EndPointDevice target) {
        Debug.Log($"{target.endpontBaseData.endpoinName} 尝试添加到网络 {target.endpontBaseData.networkName}中,此时网络终端数量 {curEndpointCount}");
        if (_curEndpointCount >= _maxEndpointCount) return false; // 满了

        foreach(var parent in endpointParents)
        {
            // 选一个挂载
            if (parent.childCount != 0) continue; // 一个 parent只允许挂载一个

            // 可以挂载
            target.gameObject.transform.SetParent(parent);
            target.gameObject.transform.localPosition = Vector3.zero;
            Debug.LogWarning($"{target.endpontBaseData.endpoinName} Added into {networkName}");
            _endpointMap.Add(target, parent);
            _endpointList.Add(target);

            _curEndpointCount++;
            _isfull = (_maxEndpointCount == curEndpointCount);
            // 通知更新界面
            OnEndpointListChanged?.Invoke();
            SubNetworkManager.Instance.allendpointlist.Add(target);
            return true;
        }
        // 挂载点与最大数量不匹配，无法挂载
        Debug.LogError("挂载点与最大数量不匹配，无法挂载");
        return false;
    }

    // 删除终端
    public bool deleteEndpoint(EndPointDevice endpoint)
    {
        if (endpointMap.ContainsKey(endpoint))
        {
            Transform parent ;
            endpointMap.TryGetValue(endpoint,out parent);
            endpointMap.Remove(endpoint);
            endpointList.Remove(endpoint);
            _curEndpointCount--;
            _isfull = (_maxEndpointCount == curEndpointCount);
            for (int i = 0; i< parent.childCount; i++)
            {
                Destroy(parent.GetChild(i).gameObject);
            }
            
            // 通知更新界面
            OnEndpointListChanged?.Invoke();
            Debug.LogWarning($"{endpoint.endpontBaseData.endpoinName}删除成功！");
            // 删除列表
            SubNetworkManager.Instance.allendpointlist.RemoveAll(item => item == endpoint);
            return true; // 删除成功
        }

        Debug.LogWarning("删除的目标终端不在该网络下！");
        return false; 
    }

    public void createOneDevice(GameObject endPointDevicePrefab)
    {
        var go = Instantiate(endPointDevicePrefab);
        var endpointDeviceCom = go.GetComponent<EndPointDevice>();
        if (endpointDeviceCom == null)
        {
            Debug.LogError($"{endPointDevicePrefab.name} doesn't has the component 'EndPointDevice' ");
            Destroy(go); // 如果没有 EndPointDevice 组件，销毁实例化的对象
            return;
        }
        // 设置网络信息
        var networkName = this.networkName;
        var subMask = this.subnetMask;
        endpointDeviceCom.endpontBaseData.networkName = networkName; //
        endpointDeviceCom.endpontBaseData.subMask = subMask; //

        #region 分配 IP 地址
        // 
        // 1. 解析网络地址
        string[] networkOctets = network.Split('.'); //
        if (networkOctets.Length != 4) //
        {
            Debug.LogError($"Invalid network address format: {network}");
            Destroy(go);
            return;
        }

        // 获取网络地址的前三个八位字节 (例如 "192.168.1")
        string networkPrefix = $"{networkOctets[0]}.{networkOctets[1]}.{networkOctets[2]}"; //

        // 2. 根据当前终端数量分配主机序号
        // 假设从 .10 开始分配，后续递增
        int hostId = 10 + _curEndpointCount;

        // 3. 构建完整的 IP 地址
        string assignedIpAddress = $"{networkPrefix}.{hostId}"; //

        // 将分配的 IP 地址存储到 endpontBaseData.network 中
        // 注意：EndPointDevice 的 endpontBaseData.network 字段用于存储具体的IP地址，而不是网络段。
        #endregion
        endpointDeviceCom.endpontBaseData.network = assignedIpAddress; //
        
        Debug.Log($"为新设备 '{endpointDeviceCom.endpontBaseData.endpoinName}' 分配了 IP 地址: {assignedIpAddress}");

        // 可以在这里设置设备的名称，例如：
        var networkId = networkName.Split("-")[1];// network-4
        var endpoinName = $"无人机{networkId}-{hostId}号";
        endpointDeviceCom.endpontBaseData.endpoinName = endpoinName;
        
        endpointDeviceCom.SetupDeviceData(networkName, endpoinName, assignedIpAddress, subMask, GenerateRandomMacAddress());
        addIntoNetwork(endpointDeviceCom);
        Debug.Log($"创建 设备 在网络{endpointDeviceCom.endpontBaseData.networkName}下，设备名称 {endpointDeviceCom.endpontBaseData.endpoinName}");
    }
    
    private string GenerateRandomMacAddress()
    {
        byte[] bytes = new byte[6];
        System.Random random = new System.Random();

        // 填充前5个字节
        for (int i = 0; i < 5; i++)
        {
            bytes[i] = (byte)random.Next(0, 256);
        }
        bytes[0] = (byte)(random.Next(0, 256) & 0xFC);

        // 填充最后一个字节
        bytes[5] = (byte)random.Next(0, 256);

        // 将字节数组格式化为MAC地址字符串
        return string.Format("{0:X2}:{1:X2}:{2:X2}:{3:X2}:{4:X2}:{5:X2}",
            bytes[0], bytes[1], bytes[2], bytes[3], bytes[4], bytes[5]);
    }
    private void Awake()
    {
        // 注册到Manager
        SubNetworkManager.Instance.subNetworkControllers.Add(this);
        SubNetworkManager.Instance.subNetworkMaps.Add(this.networkName, this);
        Debug.Log($"{this.networkName} add in the network maps");
    }


    
}
