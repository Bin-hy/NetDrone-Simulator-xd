using System.Collections;
using System.Collections.Generic;
using System.Net;
using TMPro;
using TMPro.EditorUtilities;
using UnityEngine;
using UnityEngine.UI;
using static TMPro.TMP_Dropdown;

// 握手发起方
public enum ClientTcpHandshakeState
{
    /// <summary>
    /// 客户端初始状态，准备发送 SYN 报文。
    /// </summary>
    Closed,
    /// <summary>
    /// 客户端已发送 SYN 报文，等待服务器的 SYN-ACK 报文。
    /// </summary>
    SynSent,
    /// <summary>
    /// 客户端已收到 SYN-ACK 报文，并发送 ACK 报文，连接建立中。
    /// </summary>
    Established
}

// 握手接收方
public enum ServerTcpHandshakeState
{
    /// <summary>
    /// 服务器初始状态，等待客户端的 SYN 报文。
    /// </summary>
    Listen,
    /// <summary>
    /// 服务器已收到 SYN 报文，并发送 SYN-ACK 报文，等待客户端的 ACK 报文。
    /// </summary>
    SynReceived,
    /// <summary>
    /// 服务器已收到 ACK 报文，连接已建立。
    /// </summary>
    Established
}

// 挥手发起方
public enum ClientTcpTeardownState
{
    /// <summary>
    /// 连接已建立，准备关闭。
    /// </summary>
    Established,
    /// <summary>
    /// 主动关闭方已发送 FIN 报文，等待对方的 ACK。
    /// </summary>
    FinWait1,
    /// <summary>
    /// 主动关闭方已收到对方的 ACK，等待对方的 FIN 报文。
    /// </summary>
    FinWait2,
    /// <summary>
    /// 主动关闭方已收到 FIN 报文，并发送 ACK 报文，等待一段时间后进入关闭状态。
    /// </summary>
    TimeWait,
    /// <summary>
    /// 连接已完全关闭。
    /// </summary>
    Closed
}

// 挥手接收方
public enum ServerTcpTeardownState
{
    /// <summary>
    /// 连接已建立，等待对方的 FIN 报文。
    /// </summary>
    Established,
    /// <summary>
    /// 被动关闭方已收到 FIN 报文，并发送 ACK 报文，等待应用程序关闭。
    /// </summary>
    CloseWait,
    /// <summary>
    /// 被动关闭方已发送 FIN 报文，等待对方的 ACK 报文。
    /// </summary>
    LastAck,
    /// <summary>
    /// 连接已完全关闭。
    /// </summary>
    Closed
}

public class TCPPanelView : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown endpointSelector;
    private List<EndPointDevice > _endpoints = new List<EndPointDevice>(); // 临时储存dropdown的数据

    [SerializeField] private GameObject localDetailParent;

    [SerializeField] private GameObject remoteDetailParent;

    [SerializeField] private TcpShowItem detailPrefab; // 日志细节 预制体 输出

    [SerializeField] private Button TCPConnectKnowledgeButton; // TCP连接知识介绍调出按钮
    [SerializeField] private Button TCPDisconnectKnowledgeButton;
    [SerializeField] private Button TCPKnowledgeButton;

    // 本地日志
    private List<TcpShowItem> _localLogDetails = new List<TcpShowItem>();
    public List<TcpShowItem> localLogDetails => _localLogDetails;
    // 远端日志
    private List<TcpShowItem> _remoteLogDetails = new List<TcpShowItem>() ;
    public List<TcpShowItem> remoteLogDetails => _remoteLogDetails ;

    // 清除日志
    public void cleanAllLogs() {
        // 移除 并删除
        cleanLogs(_localLogDetails, localDetailParent);
        cleanLogs(_remoteLogDetails, remoteDetailParent);
    }
    private void cleanLogs(List<TcpShowItem> logDetails, GameObject logParent)
    {
        
    }
    public void UpdateDropDown(List<EndPointDevice> endPoints)
    {
        _endpoints = endPoints;
        endpointSelector.ClearOptions(); // 清除选项
        List<OptionData> options = new List<OptionData>();// 创建选项列表
        foreach (var endpoint in _endpoints)
        {
            // 使用deviceName作为显示文本
            options.Add(new OptionData(endpoint.endpontBaseData.endpoinName));
            // 添加选项到Dropdown
        }
        endpointSelector.AddOptions(options); // 添加到选项表中
        endpointSelector.onValueChanged.AddListener(OnRemoteEndpointSelected);
    }
    private void OnRemoteEndpointSelected(int selectedIndex) {
        if (selectedIndex >= 0 && selectedIndex < _endpoints.Count)
        {
            EndPointDevice selectedEndpoint = _endpoints[selectedIndex];

            // 在这里可以对选中的Endpoint进行操作
            // HandleSelectedEndpoint(selectedEndpoint);
            
        }
    }

    public void UpdateLocalLog()
    {

    }

    public void UpdateRemoteLog()
    {

    }

    public void DeleteRemote()
    {

    }
    private void Awake()
    {
        TCPConnectKnowledgeButton.onClick.AddListener(() => {
            UIManager.Instance.OpenPopup("KnowledgeExplain_TCP Connect");
        });
        TCPDisconnectKnowledgeButton.onClick.AddListener(() => {
            UIManager.Instance.OpenPopup("KnowledgeExplain_TCP Disconnect");
        });
        TCPKnowledgeButton.onClick.AddListener(() => {
            UIManager.Instance.OpenPopup("KnowledgeExplain_TCP");
        });
    }

}
