using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIInitializer : MonoBehaviour
{
    [Header("UI 界面预制体")]
    public List<UIManagedBase> uiScreenPrefabs;

    private void Start()
    {
        InitializedUI();
    }
    private void InitializedUI() { 
        
        foreach(UIManagedBase  prefab in uiScreenPrefabs)
        {
            UIManagedBase screenInstance = Instantiate(prefab);

            UIManager.Instance.RegisterScreen(screenInstance, screenInstance.name.Replace("(Clone)", ""));
        }

        // 打开面板示例
        //UIManager.Instance.OpenPanel("MainMenuPannel");
    }

}
