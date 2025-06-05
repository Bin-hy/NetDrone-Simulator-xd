using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ExampleSubItemData
{
    // 存储 List 中的 项 
    public string TargetNetwork;
    public string NextJump;
    public string OutInterface;
    public string routesID;
}

[Serializable]
public class ExampleItemListWrapper
{
    // 包装List
    public List<ExampleSubItemData> Items;
}


[Serializable]
public class ERouterData
{
    // 单个路由器数据
    public RouterCoreStateData routerCoreStateData;
    public ExampleItemListWrapper routes;
}
[Serializable]
public class ERoutersData
{
    // 所有 路由的数据
    public List<RouterData> routersData;
}

public class ExampleModel : MonoBehaviour
{

    public event Action OnDataChanged; // 当数据改变的时候, View中监听数据改变渲染新数据。

    #region Model数据
    private ERouterData _eRouterData; // Model数据 ！
    public ERouterData eRouterData { get { return _eRouterData; } } // 限制get操作
    #endregion

    // 其他数据
    private string _eRouterId; // 编号
    public string eRouterId { get { return _eRouterId; } }

    // 路由字典
    private Dictionary<string, ExampleSubItemData> RoutesItemMap = new Dictionary<string, ExampleSubItemData>();
    // 路由信息 列表
    private List<ExampleSubItemData> RoutesItemList = new List<ExampleSubItemData>();

    public void Initialize(ERouterData initRouterData)
    {
        // 初始化数据
        _eRouterData = initRouterData;
    }

    #region CRUD 增删改
    // 添加路由表项
    public void addRouterItem()
    {

    }
    // 删除路由表项
    public void deleteRouterItem()
    {

    }
    // 修改路由表项目
    public void updateRouterItem()
    {

    }
    #endregion
}
