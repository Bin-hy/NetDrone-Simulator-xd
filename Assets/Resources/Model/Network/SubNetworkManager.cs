using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubNetworkManager : MonoBehaviour
{
    #region 单例

    // 1. 静态实例
    private static SubNetworkManager _instance;

    // 2. 公共访问点
    public static SubNetworkManager Instance
    {
        get
        {
            if (_instance == null)
            {
                // 查找现有实例
                _instance = FindObjectOfType<SubNetworkManager>();

                // 如果没有则创建新实例
                if (_instance == null)
                {
                    GameObject obj = new GameObject("SubNetworkManager");
                    _instance = obj.AddComponent<SubNetworkManager>();
                    DontDestroyOnLoad(obj); // 跨场景不销毁
                }
            }
            return _instance;
        }
    }
    #endregion
    
    // 统一管理 子网络
    private List<SubNetworkController> _subNetworkControllers = new List<SubNetworkController>();
    public List<SubNetworkController> subNetworkControllers => _subNetworkControllers;

    private Dictionary<string, SubNetworkController> _subNetworkMaps = new Dictionary<string, SubNetworkController>();
    public Dictionary<string ,SubNetworkController> subNetworkMaps=>_subNetworkMaps;

    private List<EndPointDevice> _endpointlist = new List<EndPointDevice>();
    public  List<EndPointDevice> allendpointlist =>_endpointlist; // 终端列表

    // 根据networkName获取网络
    public SubNetworkController getSubNetworkController(string networkName)
    {
        if(!_subNetworkMaps.ContainsKey(networkName))
        {
            Debug.LogWarning($"{networkName} is not in Dictory.");
            return null;
        }
        return _subNetworkMaps[networkName];
    }

    /// <summary>
    /// 删除网络中的设备
    /// </summary>
    /// <param name="networkName"></param>
    /// <param name="device"></param>
    /// <returns></returns>
    public bool DeleteNetworkDevice(string networkName,EndPointDevice device)
    {
        SubNetworkController subNetworkController = getSubNetworkController(networkName);
        if(subNetworkController == null)
        {
            Debug.LogWarning($"{networkName} is not found");
            return false;
        }
        return subNetworkController.deleteEndpoint(device);

        
    }
}
