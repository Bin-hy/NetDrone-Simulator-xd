using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TCPController : MonoBehaviour
{
    [SerializeField] private Button localConnectButton; // 本地连接按钮
    [SerializeField] private Button remoteDisconnectButton; // 远端断开按钮
    [SerializeField] private Button localDisconnectButton; // 本地断开按钮

    [SerializeField] private EndPointDevice _localDevice;  // 本地设备
    [SerializeField] private EndPointDevice _remoteDevice; // 远端设备

    // 接收的节点
    [SerializeField] private Transform receivedPoint; // 接收射线的接收点
    [SerializeField] private Transform sendPoint; // 发射射线的发射点

    private EndPointDevice pointDevice;
    bool _isOperating = false;
    //bool _isConnecting = false;
    //bool _isDisconnecting = false;

    [SerializeField] private TCPPanelView panelView;

    private void Awake()
    {
        // 本地发起连接
        localConnectButton.onClick.AddListener(() =>
        {
            if (_isOperating)
            {
                UIManager.Instance.OpenConfirm("ConfirmToolTip", "操作提醒", "连接或者中断正在进行，请先完成当前操作", () => {});
                return;
            }
            // 连接提醒
            UIManager.Instance.OpenConfirm("ConfirmToolTip", "是否连接", "", () => {
                // StartCoroutine();
                //StartCoroutine(TCPSystem.Instance.runConnect());
            });
        });

        // 远端发起断开
        remoteDisconnectButton.onClick.AddListener(() => {
            if (_isOperating)
            {
                UIManager.Instance.OpenConfirm("ConfirmToolTip", "操作提醒", "连接或者中断正在进行，请先完成当前操作", () => { });
                return;
            }

            // 连接提醒
            UIManager.Instance.OpenConfirm("ConfirmToolTip", "是否中断", "", () => {
                // StartCoroutine();
            });
        });

        // 本地发起断开
        localDisconnectButton.onClick.AddListener(() =>
        {
            if (_isOperating)
            {
                UIManager.Instance.OpenConfirm("ConfirmToolTip", "操作提醒", "连接或者中断正在进行，请先完成当前操作", () => { });
                return;
            }
            // 连接提醒
            UIManager.Instance.OpenConfirm("ConfirmToolTip", "是否中断", "", () => {
                // StartCoroutine();
            });
        });

    }

    private void Start()
    {
        var endpointDevices =SubNetworkManager.Instance.allendpointlist;
        panelView.UpdateDropDown(endpointDevices);
    }




}
