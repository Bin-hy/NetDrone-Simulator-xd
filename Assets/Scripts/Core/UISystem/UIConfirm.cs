using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIConfirm : UIPopup
{
    [Header("Confirm Dialog Elements")]
    [SerializeField] private TextMeshProUGUI titleText;   // 弹窗标题
    [SerializeField] private TextMeshProUGUI contentText; // 弹窗内容
    [SerializeField] private Button CancelButton;  // 取消按钮
    [SerializeField] private Button ConfirmButton; // 确认按钮

    // 直接使用 Action 委托来存储回调，而不是事件
    private Action _onConfirmAction;
    private Action _onCancelAction;

    protected override void Awake()
    {
        base.Awake(); // Call UIPopup's Awake, which handles its own closeButton if present.

        CancelButton.onClick.AddListener(HandleCancelAndClose);
        ConfirmButton.onClick.AddListener(HandleConfirmAndClose);
    }

    private void OnDestroy()
    {
        CancelButton.onClick.RemoveListener(HandleCancelAndClose);
        ConfirmButton.onClick.RemoveListener(HandleConfirmAndClose);
    }

    private void HandleConfirmAndClose()
    {
        _onConfirmAction?.Invoke(); // 调用存储的回调
        Debug.Log("HandleConfirmAndClose 被调用");
        UIManager.Instance.CloseTopPopup(); // 使用 UIManager 关闭此弹窗并管理堆栈
    }

    private void HandleCancelAndClose()
    {
        _onCancelAction?.Invoke(); // 调用取消回调 (如果提供)
        Debug.Log("HandleConfirmAndClose 被调用");
        UIManager.Instance.CloseTopPopup(); // 使用 UIManager 关闭此弹窗并管理堆栈
    }

    /// <summary>
    /// Sets up the confirm dialog with title, content, and callback actions.
    /// </summary>
    /// <param name="title">The title text for the confirm dialog.</param>
    /// <param name="content">The main message/content text for the confirm dialog.</param>
    /// <param name="confirmCallback">The action to be invoked when the confirm button is pressed.</param>
    /// <param name="cancelCallback">Optional action to be invoked when the cancel button is pressed. If null, only the dialog closes.</param>
    public void SetupConfirm(string title, string content, Action confirmCallback, Action cancelCallback = null)
    {
        // Set the title and content text
        if (titleText != null)
        {
            titleText.text = title;
        }
        if (contentText != null)
        {
            contentText.text = content;
        }
        Debug.Log($"配置确认信息");
        // 直接赋值回调，不需要清空事件订阅者
        _onConfirmAction = confirmCallback;
        _onCancelAction = cancelCallback; // 直接赋值，如果为null则不执行
    }
}