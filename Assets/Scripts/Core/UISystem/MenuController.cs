using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    [Header("导航栏")]
    [SerializeField] public Button MenuShowButton;
    [SerializeField] public Button DroneShowButton;
    [SerializeField] public Button DroneManageButton;
    [SerializeField] public Button CRCLearnButton;
    [SerializeField] public Button RouteLearnButton;
    [SerializeField] public Button TCPLearnButton;



    public void Awake()
    {
        Initialized();
    }
    private void Start()
    {
        // 默认打开MenuPanel界面
        UIManager.Instance.OpenPanel("MenuPanel");
    }

    private void Initialized() {
        MenuShowButton.onClick.AddListener(() => {
            UIManager.Instance.OpenPanel("MenuPanel");
        });
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
            UIManager.Instance.OpenPanel("TCPPanel");
        });

    }
}
