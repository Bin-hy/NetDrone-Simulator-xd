using System;
using UnityEngine;

/// <summary>
/// 
/// </summary>
public abstract class UIManagedBase : MonoBehaviour
{
    // 用于控制整个UI界面的CanvasGroup，方便控制透明度、交互性、射线阻挡
    [SerializeField] protected CanvasGroup canvasGroup;

    // UI的唯一ID，用于通过代码查找和管理
    public string ScreenID { get; private set; }

    // 界面显示/隐藏时的回调事件
    public event Action OnScreenOpened;
    public event Action OnScreenClosed;

    protected virtual void Awake()
    {
        // 确保CanvasGroup存在
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                // 如果没有，就添加一个
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }
    }

    // 初始化方法，由UIManager调用
    public void Initialize(string id)
    {
        ScreenID = id;
        gameObject.SetActive(false); // 默认隐藏
    }

    // 抽象方法，子类必须实现：显示界面
    public virtual void Open()
    {
        gameObject.SetActive(true);
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true; // 默认阻挡射线
        canvasGroup.interactable = true;   // 默认可交互
        OnScreenOpened?.Invoke();
    }

    // 抽象方法，子类必须实现：隐藏界面
    public virtual void Close()
    {
        gameObject.SetActive(false);
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
        OnScreenClosed?.Invoke();
    }

    // 辅助方法：设置UI是否可交互（可能在弹窗出现时禁用下层UI）
    public void SetInteractable(bool interactable)
    {
        if (canvasGroup != null)
        {
            canvasGroup.interactable = interactable;
            // 注意：blocksRaycasts通常保持true，除非你希望它被穿透
            // canvasGroup.blocksRaycasts = interactable; // 如果希望不可交互时也不阻挡射线
        }
    }
}
