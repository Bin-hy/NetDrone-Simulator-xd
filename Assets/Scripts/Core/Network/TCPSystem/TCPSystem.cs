using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 负责调用TCP 握手 和 TCP 挥手的工作
/// 通知本地和终端设备
/// </summary>
public class TCPSystem : MonoBehaviour
{
    #region 单例

    // 1. 静态实例
    private static TCPSystem _instance;

    // 2. 公共访问点
    public static TCPSystem Instance
    {
        get
        {
            if (_instance == null)
            {
                // 查找现有实例
                _instance = FindObjectOfType<TCPSystem>();

                // 如果没有则创建新实例
                if (_instance == null)
                {
                    GameObject obj = new GameObject("TCPSystem");
                    _instance = obj.AddComponent<TCPSystem>();
                    DontDestroyOnLoad(obj); // 跨场景不销毁
                }
            }
            return _instance;
        }
    }
    #endregion

    /// 运行连接协程
    public IEnumerator runConnect(DataStreamLaser lineClent,DataStreamLaser lineServer,EndPointDevice ConnectClient , EndPointDevice ConnectServer)
    {

        // 1.本地：选中网络中的终端设备，点击开始握手，此时本地设备向无人机发射一条蓝色的射线，大屏中显示 “正在发送SYN 数据” 
        
        // 2.无人机界面状态显示：按钮出现一个 ”确认接收“， 点击后出现 显示 "已接收 ACK 连接请求 "，同时， 显示 ”正在发送 SYN-ACK“ ， 1s后发射 一条 红色射线 ，

        // 3.本地：大屏中显示 “接收到SYN - ACK” ，按钮变成 ”确认连接端的确认“，点击按钮 ，1s后发射一条黄色的射线，显示 ”正在发送ACK“

        // 4.无人机界面显示：显示 ”已经收到 ACK“，间隔1s后

        // 5.本地向 无人机终端 ，发射一条绿色射线 ，双方均显示 ”正在建立连接“ ，”连接建立成功“

        // 6.同时双方界面上均多出一个按钮：断开连接

        // 7.任意一方显示断开连接，例如：本地

        // 发送客户端

        // 客户端显示“正在发送SYN 数据” 

        // 

        yield return null;
    }
}
