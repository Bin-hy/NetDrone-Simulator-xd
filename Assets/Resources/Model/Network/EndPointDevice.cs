using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EndpointBaseData {
    public string networkName; // 所在网络名称
    public string endpoinName; // 设备名称
    public string network; // 具体网络 192.168.1.0/24
    public string subMask;// 子网掩码
    public string macAddress; // mac地址
    public int _subMaskInt;
    public int subMaskInt {
        get {
            // 通过255.255.255.0
            return ConvertSubnetMaskToCidr(subMask);
        }
    }
    public int ConvertSubnetMaskToCidr(string subnetMaskString)
    {
        if (string.IsNullOrEmpty(subnetMaskString))
        {
            Debug.LogError("Subnet mask string cannot be null or empty.");
            return -1;
        }

        string[] octets = subnetMaskString.Split('.');
        if (octets.Length != 4)
        {
            Debug.LogError("Invalid subnet mask format. It should have 4 octets separated by dots.");
            return -1;
        }

        int cidr = 0;
        foreach (string octet in octets)
        {
            if (int.TryParse(octet, out int octetValue))
            {
                if (octetValue < 0 || octetValue > 255)
                {
                    Debug.LogError("Invalid octet value in subnet mask. Must be between 0 and 255.");
                    return -1;
                }

                // Count the number of set bits (1s) in the binary representation of the octet
                cidr += CountSetBits(octetValue);
            }
            else
            {
                Debug.LogError($"Invalid octet '{octet}' in subnet mask. Not a valid number.");
                return -1;
            }
        }

        return cidr;
    }

    /// <summary>
    /// Counts the number of set bits (1s) in an integer.
    /// </summary>
    /// <param name="n">The integer to count bits for.</param>
    /// <returns>The number of set bits.</returns>
    private int CountSetBits(int n)
    {
        int count = 0;
        while (n > 0)
        {
            n &= (n - 1); // Brian Kernighan's algorithm to clear the least significant set bit
            count++;
        }
        return count;
    }

}

// 终端设备
// 挂载在设备上了
public class EndPointDevice : MonoBehaviour
{
    public EndpointBaseData endpontBaseData = new EndpointBaseData();
    // 直接的初始化的方式
    [SerializeField] private string networkName; // 所在网络名称
    [SerializeField] private string endpoinName; // 设备名称
    [SerializeField] private string network; // 具体网络 192.168.1.0/24
    [SerializeField] private string subMask;// 子网掩码
    [SerializeField] private string macAddress; // mac地址
    [Tooltip("是否是预先放置的")][SerializeField] private bool isPlaced = true;
    [SerializeField] private TextMeshProUGUI outerLabel;
    private void Start()
    {
        if (!isPlaced)return ;
        endpontBaseData.networkName = networkName;
        endpontBaseData.endpoinName = endpoinName;
        endpontBaseData.network = network;
        endpontBaseData.subMask = subMask;
        endpontBaseData.macAddress = macAddress;
        outerLabel.text = endpoinName;
        // 添加到网络下
        var subNetworkController = SubNetworkManager.Instance.getSubNetworkController(networkName);
        if (!subNetworkController) {
            Debug.LogError($"{networkName} is not exsit ,please setup Script 'EndPointDevice Corectly!' ");
            return;
        }
        subNetworkController.addIntoNetwork(this); // 预先放置的自己加载进来
    }

    public void SetupDeviceData(string netName, string epName, string netAddr, string sMask, string macAddr,bool isPlaced = false)
    {
        endpontBaseData.networkName = netName;
        endpontBaseData.endpoinName = epName;
        endpontBaseData.network = netAddr; // 此时 network 字段存的是具体的 IP 地址
        endpontBaseData.subMask = sMask;
        endpontBaseData.macAddress = macAddr;
        outerLabel.text = epName;
        this.isPlaced = isPlaced; // 创建默认设置是非放置模式
    }
    void UpdateView()
    {

    }
}
