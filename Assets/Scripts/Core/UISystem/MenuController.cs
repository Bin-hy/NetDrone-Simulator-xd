using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    [Header("导航栏")]
    public Button DroneShowButton;
    public Button DroneManageButton;
    public Button CRCLearnButton;
    public Button RouteLearnButton;
    public Button TCPLearnButton;



    public void Awake()
    {
        Initialized();
    }

    private void Initialized() {
        // 无人机视频流界面
        DroneShowButton.onClick.AddListener(() =>
        {
            UIManager.Instance.OpenPanel("DronesMonitorPanel");
        });
        // 无人机管理界面
        DroneManageButton.onClick.AddListener(() =>
        {
            UIManager.Instance.OpenPanel("DronesManagerPanel");
        });
        // CRC校验界面
        CRCLearnButton.onClick.AddListener(() =>
        {
            UIManager.Instance.OpenPanel("CRCPanel");
        });

        // 路由学习界面
        RouteLearnButton.onClick.AddListener(() =>
        {
            UIManager.Instance.OpenPanel("RoutePanel");
        });

        // TCP握手学习界面
        TCPLearnButton.onClick.AddListener(() =>
        {

        });

    }
}
