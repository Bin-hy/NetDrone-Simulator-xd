using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{

    [Header("UI 父级容器 (用于排序)")]
    public Transform panelParent;  // 普通面板的父级
    public Transform popupParent;  // 弹窗的父级
    public Transform screenPanelParent; // 用户屏幕的Panel父级
    public GameObject uiMaskPrefab; // 遮罩预制体 (一个全屏半透明Image)

    // 存储所有已注册的UI界面
    private Dictionary<string, UIManagedBase> registeredScreens = new Dictionary<string, UIManagedBase>();

    // 弹窗堆栈 (用于管理弹窗的层级和关闭顺序)
    private Stack<UIPopup> popupStack = new Stack<UIPopup>();

    private GameObject currentUIMask; // 当前激活的遮罩实例

    #region 单例
    private static UIManager _instance = null;
    public static UIManager Instance
    {
        get
        {
            if (_instance == null)
            {
                if ((_instance = FindObjectOfType<UIManager>()) == null)
                {
                    GameObject go = new GameObject(typeof(UIManager).ToString()); // 创建一个对应的节点
                    _instance = go.AddComponent<UIManager>();
                    DontDestroyOnLoad(go); // 让UIManager在场景切换时不被销毁
                }
            }
            return _instance;
        }
    }
    #endregion

    // --- UI 注册/初始化 ---
    public void RegisterScreen(UIManagedBase screen, string id)
    {
        if (registeredScreens.ContainsKey(id))
        {
            Debug.LogWarning($"UI Screen with ID '{id}' already registered.");
            return;
        }
        screen.Initialize(id);
        registeredScreens.Add(id, screen);

        // 设置父级容器
        if (screen is UIPanel)
        {
            screen.transform.SetParent(panelParent, false); // false表示不保留世界坐标
        }
        else if (screen is UIPopup)
        {
            screen.transform.SetParent(popupParent, false);
        }
        else if(screen is UIConfirm)
        {
            screen.transform.SetParent(popupParent, false);
        }else if(screen is UIScreenPanel)
        {
            screen.transform.SetParent(screenPanelParent, false);

        }
    }

    #region 屏幕面板

    // -- 打开屏幕面板 --
    public void OpenScreenPanel(string panelID)
    {
        if (registeredScreens.TryGetValue(panelID, out UIManagedBase screen))
        {
            if (screen is UIScreenPanel screenPanel)
            {
                // Optionally close other active UIScreenPanels if only one should be open at a time
                foreach (var entry in registeredScreens)
                {
                    if (entry.Value is UIScreenPanel existingScreenPanel && existingScreenPanel != screenPanel && existingScreenPanel.gameObject.activeSelf)
                    {
                        existingScreenPanel.Close();
                    }
                }
                screenPanel.Open();
                Debug.Log($"Opened Screen Panel: {panelID}");
            }
            else
            {
                Debug.LogWarning($"Attempted to open '{panelID}' as a screen panel, but it is of type {screen.GetType().Name}. Use appropriate Open method.");
            }
        }
        else
        {
            Debug.LogError($"Screen Panel with ID '{panelID}' not registered.");
        }
    }
    // -- 关闭屏幕面板
    public void CloseScreenPanel(string panelID)
    {
        if (registeredScreens.TryGetValue(panelID, out UIManagedBase screen))
        {
            if (screen is UIScreenPanel screenPanel)
            {
                screenPanel.Close();
                Debug.Log($"Closed Screen Panel: {panelID}");
            }
            else
            {
                Debug.LogWarning($"Attempted to close '{panelID}' as a screen panel, but it is of type {screen.GetType().Name}.");
            }
        }
        else
        {
            Debug.LogWarning($"Screen Panel with ID '{panelID}' not registered or already closed.");
        }
    }
    #endregion

    // --- 打开面板 ---
    public void OpenPanel(string panelID)
    {
        if (registeredScreens.TryGetValue(panelID, out UIManagedBase screen))
        {
            if (screen is UIPanel panel)
            {
                // 关闭当前激活的所有其他Panel (如果需要)
                foreach (var entry in registeredScreens)
                {
                    if (entry.Value is UIPanel existingPanel && existingPanel != panel && existingPanel.gameObject.activeSelf)
                    {
                        existingPanel.Close();
                    }
                }
                panel.Open();
                Debug.Log($"Opened Panel: {panelID}");
            }
            else
            {
                Debug.LogWarning($"Attempted to open popup '{panelID}' as a panel. Use OpenPopup instead.");
            }
        }
        else
        {
            Debug.LogError($"Panel with ID '{panelID}' not registered.");
        }
    }

    // --- 打开弹窗 ---
    public void OpenPopup(string popupID)
    {
        if (registeredScreens.TryGetValue(popupID, out UIManagedBase screen))
        {
            if (screen is UIPopup popup)
            {
                // 如果当前有其他弹窗，使其不可交互
                if (popupStack.Count > 0)
                {
                    popupStack.Peek().SetInteractable(false);
                }

                // 显示遮罩
                ShowMask();

                // 压入堆栈并打开弹窗
                popupStack.Push(popup);
                popup.Open();
                Debug.Log($"Opened Popup: {popupID}. Stack size: {popupStack.Count}");
            }
            else
            {
                Debug.LogWarning($"Attempted to open panel '{popupID}' as a popup. Use OpenPanel instead.");
            }
        }
        else
        {
            Debug.LogError($"Popup with ID '{popupID}' not registered.");
        }
    }

    // --- 关闭顶层弹窗 ---
    public void CloseTopPopup()
    {
        if (popupStack.Count > 0)
        {
            UIPopup topPopup = popupStack.Pop();
            topPopup.Close();
            Debug.Log($"Closed Popup: {topPopup.ScreenID}. Stack size: {popupStack.Count}");

            // 如果没有弹窗了，隐藏遮罩
            if (popupStack.Count == 0)
            {
                HideMask();
            }
            else
            {
                // 如果还有其他弹窗，让下一个顶层弹窗可交互
                popupStack.Peek().SetInteractable(true);
            }
        }
        else
        {
            Debug.LogWarning("No popups to close.");
        }
    }
    // --- 打开确认弹窗 (UIConfirm) ---
    // This method is specifically for UIConfirm instances to pass title, content, and callbacks.
    public void OpenConfirm(string confirmID, string title, string content, System.Action onConfirm, System.Action onCancel = null)
    {
        if (registeredScreens.TryGetValue(confirmID, out UIManagedBase screen))
        {
            if (screen is UIConfirm confirmDialog)
            {
                // If there's an existing popup, make it non-interactive
                if (popupStack.Count > 0)
                {
                    popupStack.Peek().SetInteractable(false);
                }

                ShowMask(); // Show the mask

                // Push the UIConfirm instance onto the popup stack
                popupStack.Push(confirmDialog);

                // Setup the confirm dialog with title, content, and callbacks
                confirmDialog.SetupConfirm(title, content, onConfirm, onCancel);

                // Open the dialog (make it active, interactive, etc.)
                confirmDialog.Open();

                Debug.Log($"Opened Confirm Dialog: {confirmID}. Stack size: {popupStack.Count}");
            }
            else
            {
                Debug.LogWarning($"Attempted to open screen '{confirmID}' as a confirm dialog. It's of type {screen.GetType().Name}. Use appropriate Open method.");
            }
        }
        else
        {
            Debug.LogError($"Confirm Dialog with ID '{confirmID}' not registered.");
        }
    }

    // --- 获取已注册的界面 ---
    public T GetScreen<T>(string id) where T : UIManagedBase
    {
        if (registeredScreens.TryGetValue(id, out UIManagedBase screen))
        {
            return screen as T;
        }
        return null;
    }
    // --- 遮罩管理 ---
    private void ShowMask()
    {
        if (currentUIMask == null && uiMaskPrefab != null)
        {
            // 实例化遮罩
            currentUIMask = Instantiate(uiMaskPrefab, popupParent); // 放在弹窗父级下
            currentUIMask.name = "UI_Mask_Instance";
            // 确保遮罩在所有已激活的弹窗之下
            // 这可以通过调整 Instantiate 的位置或手动 SetSiblingIndex 实现
            // For simple case, just set it at the bottom of popupParent
            currentUIMask.transform.SetAsFirstSibling();
            currentUIMask.SetActive(true);
        }
        else if (currentUIMask != null)
        {
            currentUIMask.SetActive(true);
            currentUIMask.transform.SetAsFirstSibling(); // 确保总是在最底层
        }
    }

    private void HideMask()
    {
        if (currentUIMask != null)
        {
            currentUIMask.SetActive(false);
        }
    }
}
