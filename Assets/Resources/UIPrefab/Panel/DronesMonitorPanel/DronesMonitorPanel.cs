using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DronesMonitorPanel : MonoBehaviour
{
    #region 单例

    // 1. 静态实例
    private static DronesMonitorPanel _instance;

    // 2. 公共访问点
    public static DronesMonitorPanel Instance
    {
        get
        {
            if (_instance == null)
            {
                // 查找现有实例
                _instance = FindObjectOfType<DronesMonitorPanel>();

                // 如果没有则创建新实例
                if (_instance == null)
                {
                    GameObject obj = new GameObject("DronesMonitorPanel");
                    _instance = obj.AddComponent<DronesMonitorPanel>();
                    DontDestroyOnLoad(obj); // 跨场景不销毁
                }
            }
            return _instance;
        }
    }
    #endregion

    // 监控渲染的 贴图
    [SerializeField] private RawImage videoRawImage;
    [SerializeField] private List<DroneController> droneList; // 无人机列表
    public GameObject droneListItem;
    public Transform droneListContent;
    public DroneController curDroneController; // 选中的无人机控制器
    [SerializeField] private Button controlButton;
    // 加载无人机监控
    public void LoadDroneMonitor()
    {
        if (!curDroneController)
        {
            Debug.LogError("没有选中无人机，无法加载数据");
            return;
        }

        videoRawImage.texture = curDroneController.cameraRenderTexture;

    }

    // 加载界面
    void LoadDronesListView()
    {
        // 加载无人机数据
        var list = DroneManager.Instance._droneList;
        // 推荐方法1：倒序遍历
        for (int i = droneListContent.childCount - 1; i >= 0; i--)
        {
            // 获取子Transform
            Transform child = droneListContent.GetChild(i);
            // 销毁子GameObject
            Destroy(child.gameObject);
        }
        foreach (DroneController drone in list)
        {
            var item = Instantiate(droneListItem, droneListContent);
            var com = item.GetComponent<DroneMonitorPanelListsItem>();
            com.Init(drone);
        }
    }

    private void Start()
    {
        LoadDronesListView();
        DroneManager.Instance.OnDroneCountChange += LoadDronesListView;
        controlButton.onClick.AddListener(OnClickContorlDevice);
    }
    private void OnClickContorlDevice()
    {
        UIManager.Instance.OpenConfirm("ConfirmToolTip", "提示", $"控制无人机设备\n{curDroneController.droneName}\n中,按住左手 <color=red>X</color> 取消控制。", () => {
            ControllerManager.Instance.SetDroneController(curDroneController);
            ControllerManager.Instance.SwitchController(ControlMode.DroneControl);
        }, () => {
        });
    }
    private void OnDestroy()
    {
        DroneManager.Instance.OnDroneCountChange -= LoadDronesListView;

    }

 
}
