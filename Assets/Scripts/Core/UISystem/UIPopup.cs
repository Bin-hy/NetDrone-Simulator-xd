using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class UIPopup : UIManagedBase
{
    // 弹窗通常有额外的关闭按钮逻辑，可以在这里实现
    public override void Close()
    {
        base.Close();
        // 弹窗关闭时可以做一些清理工作
    }

    // 可以添加一个通用的关闭按钮引用，方便在Inspector中设置
    [SerializeField] private Button closeButton;

    protected override void Awake()
    {
        base.Awake();
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(OnCloseButtonClicked);
        }
    }

    protected virtual void OnCloseButtonClicked()
    {
        // 通知UIManager关闭自己 (UIManager会处理堆栈)
        UIManager.Instance.CloseTopPopup();
    }
}
