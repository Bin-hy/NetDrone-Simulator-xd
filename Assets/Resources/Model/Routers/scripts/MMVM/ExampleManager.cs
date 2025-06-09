using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class ExampleManager : MonoBehaviour
{
    //#region 单例

    //// 1. 静态实例
    //private static ExampleManager _instance;

    //// 2. 公共访问点
    //public static ExampleManager Instance
    //{
    //    get
    //    {
    //        if (_instance == null)
    //        {
    //            // 查找现有实例
    //            _instance = FindObjectOfType<ExampleManager>();

    //            // 如果没有则创建新实例
    //            if (_instance == null)
    //            {
    //                GameObject obj = new GameObject("ExampleManager");
    //                _instance = obj.AddComponent<ExampleManager>();
    //                DontDestroyOnLoad(obj); // 跨场景不销毁
    //            }
    //        }
    //        return _instance;
    //    }
    //}
    //#endregion

    //[SerializeField] private List<RouterController> routerControllerList;

    //private RoutersData _routerDatas;
    //public RoutersData routerDatas => _routerDatas;
    //private const string routersDataPath = "DataJson/routers-config";
    //// 获取当前所有的路由网络
    ////public 
    //private void Awake()
    //{
    //    LoadConfigData();
    //    UpdateData(_routerDatas, routerControllerList);
    //}

    //void LoadConfigData()
    //{
    //    var json = Resources.Load<TextAsset>(routersDataPath);
    //    _routerDatas = JsonUtility.FromJson<RoutersData>(json.text);
    //}

    //void UpdateData(RoutersData routerDatas, List<RouterController> routerControllerList)
    //{
    //    // 刷新游戏数据

    //    foreach(RouterController routerController in routerControllerList)
    //    {
            
    //    }


    //}
}
