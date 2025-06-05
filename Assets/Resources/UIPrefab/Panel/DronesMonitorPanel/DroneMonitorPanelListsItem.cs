using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DroneMonitorPanelListsItem : MonoBehaviour
{
    [SerializeField]private TextMeshProUGUI labelName;

    private DroneController controller;
    public RenderTexture selectDroneRenderTextue;
    public void setLabelName(string name)
    {
        labelName.text = name;
    }

    // 需要手动调用初始化
    public void Init(DroneController controller) {
        this.controller = controller;
        string name = controller.droneName;
        RenderTexture renderTexture = controller.cameraRenderTexture;
        labelName.name = name;
        labelName.text = name;
        if(renderTexture == null)
        {
            Debug.LogError("renderTexture is null!");
        }
        selectDroneRenderTextue = renderTexture;
    }
    public void OnButtonSelect()
    {
        // 点击选中 ， 加载监控
        DronesMonitorPanel.Instance.curDroneController = controller;
        DronesMonitorPanel.Instance.LoadDroneMonitor();
    }
}
