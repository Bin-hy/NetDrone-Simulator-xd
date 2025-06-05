using System.Collections;
using System.Collections.Generic;
using System.Net.Mail;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DroneManagerPanelItem : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _targetNetwork;
    //[SerializeField] TextMeshProUGUI _subNetwork;
    [SerializeField] TextMeshProUGUI _macAddress;
    [SerializeField] TextMeshProUGUI _deviceName;

    [SerializeField] Button _deleteButton;
    
    private EndPointDevice _endPointDevice;
    private void Awake()
    {
        _deleteButton.onClick.AddListener(PopUpConfirm);
    }
    public void Init(EndPointDevice endPointDevice)
    {
        var data = endPointDevice.endpontBaseData;
        _targetNetwork.text = data?.network;
        _macAddress.text = data?.macAddress ;
        _deviceName.text = data?.endpoinName;
        _endPointDevice = endPointDevice;
    }

    private void PopUpConfirm()
    {
        UIManager.Instance.OpenConfirm("ConfirmToolTip", "确认删除", "点击确认删除该无人机终端", ConfirmDelete);
    }
    void ConfirmDelete()
    {
        Debug.Log($"尝试删除{_endPointDevice.endpontBaseData.endpoinName}");
        if (_endPointDevice != null)
        {

            // 获取当前设备所在的网络名称
            string networkName = _endPointDevice.endpontBaseData.networkName;

            // 调用 SubNetworkManager 的方法删除设备
            bool isDeleted = SubNetworkManager.Instance.DeleteNetworkDevice(networkName, _endPointDevice);

            if (isDeleted)
            {
                Debug.Log($"成功从网络 {networkName} 删除设备: {_endPointDevice.endpontBaseData.endpoinName}");

            }
            else
            {
                Debug.LogWarning($"未能从网络 {networkName} 删除设备: {_endPointDevice.endpontBaseData.endpoinName}，可能未找到或删除失败。");
            }
        }
        else {
            Debug.Log($"尝试删除空对象错误");
        }
    }
    private void OnDestroy()
    {
        _deleteButton.onClick.RemoveListener(PopUpConfirm);
    }
}
