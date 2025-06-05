using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
[Serializable]
public class RoutersData
{
    public List<RouterData> routersData;
}
public class RouterManager : MonoBehaviour
{
    #region 单例

    // 1. 静态实例
    private static RouterManager _instance;

    // 2. 公共访问点
    public static RouterManager Instance
    {
        get
        {
            if (_instance == null)
            {
                // 查找现有实例
                _instance = FindObjectOfType<RouterManager>();

                // 如果没有则创建新实例
                if (_instance == null)
                {
                    GameObject obj = new GameObject("RouterManager");
                    _instance = obj.AddComponent<RouterManager>();
                    DontDestroyOnLoad(obj); // 跨场景不销毁
                }
            }
            return _instance;
        }
    }
    #endregion

    [SerializeField] private List<RouterController> routerControllerList;
    
    private RoutersData routerDatas;
    private const string routersDataPath = "DataJson/routers-config";
    // 获取当前所有的路由网络
    //public 
    private void Awake()
    {
        LoadConfigData();
        UpdateData(routerDatas, routerControllerList);
    }

    void LoadConfigData()
    {
        var json = Resources.Load<TextAsset>(routersDataPath);
        routerDatas = JsonUtility.FromJson<RoutersData>(json.text);
    }

    void UpdateData(RoutersData routerDatas, List<RouterController> routerControllerList)
    {
        //routerControllerList
    }
}
