using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ExampleController : MonoBehaviour
{
    [SerializeField] private ExampleView _view;
    public ExampleView view => _view;

    [SerializeField] private ExampleModel _model;
    public ExampleModel model => _model;

    /// <summary>
    /// 需要给交互物体挂载 XRSimpleInteractable 交互脚本
    /// </summary>
    [SerializeField] private XRSimpleInteractable interactableCom;


    [SerializeField] private string _routerId;
    public string routerId => _routerId; // 需要编辑器中编辑好 ， Manager根据这个加载数据

    bool _isSelected = false; // 是否显示界面

    public void Initialize(ExampleModel model, ExampleView view)
    {
        // 引用绑定
        _model = model;
        _view = view;

        // 注册模型的交互事件
        interactableCom.hoverEntered.AddListener(OnHoverEnter);
        interactableCom.hoverExited.AddListener(OnHoverExit);
        interactableCom.selectEntered.AddListener(OnSelectEnter);

        // 注册CRUD事件
        _view.OnAdded += OnAddedEvent;
        _view.OnDeleted += OnDeletedEvent;
        _view.OnUpdated += OnUpdatedEvent;

    }

    private void OnAddedEvent()
    {

    }

    private void OnDeletedEvent()
    {

    }

    private void OnUpdatedEvent()
    {

    }


    void OnHoverEnter(HoverEnterEventArgs args)
    {
        if (_isSelected) return;
        gameObject.SetActive(true);
    }
    void OnHoverExit(HoverExitEventArgs args)
    {
        if (_isSelected) return;
        gameObject.SetActive(false);
    }
    void OnSelectEnter(SelectEnterEventArgs args)
    {
        var isActive = gameObject.activeSelf;
        gameObject.SetActive(!isActive);
    }

    // 当GameObject销毁时取消订阅，防止内存泄漏
    private void OnDestroy()
    {
        //interactableCom.hoverEntered.RemoveListener(OnHoverEnter);
        //interactableCom.hoverExited.RemoveListener(OnHoverExit);
        //interactableCom.selectEntered.RemoveListener(OnSelectEnter);

        interactableCom.hoverEntered.RemoveAllListeners();
        interactableCom.hoverExited.RemoveAllListeners();
        interactableCom.selectEntered.RemoveAllListeners();
    }
}
