using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[Serializable]
public class RouteItemData {
    public string TargetNetwork;
    public string NextJump;
    public string OutInterface;
    public string routesID;
}

[Serializable]
public class RouteItemListWrapper { 
    public List<RouteItemData> Items;
}

public class RouteItem : MonoBehaviour
{
    [Header("展示信息")]
    // 目标网络
    [SerializeField] private TextMeshProUGUI targetNetwork;
    // 下一跳
    [SerializeField] private TextMeshProUGUI nextJump;
    // 路由器接口
    [SerializeField] private TextMeshProUGUI outInterface;

    private string routesID; // 当前路由的key
    public string TargetNetwork {
        get { return targetNetwork.text; }
        set { targetNetwork.text = value; }
    }

    public string NextJump
    {
        get { return nextJump.text; }
        set { nextJump.text = value; }
    }

    public string OutInterface
    {
        set { outInterface.text = value; }
        get { return outInterface.text; }
    }

    public string RoutesID
    {
        set { routesID = value; }
        get { return routesID; }
    }

    public RouteItem() { }
    public RouteItem(RouteItemData data) {
        Initialize(data);
    }
    /// <summary>
    /// 初始化数据
    /// </summary>
    /// <param name="data"></param>
    public void Initialize(RouteItemData data) {
        RoutesID = data.routesID;
        OutInterface = data.OutInterface;
        NextJump = data.NextJump;
        TargetNetwork = data.TargetNetwork;
    }
}
