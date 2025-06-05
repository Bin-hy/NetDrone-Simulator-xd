using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[Serializable]
public class RouterData
{
    public RouterCoreStateData routerCoreStateData;
    public RouteItemListWrapper routes;
}

public class RouterController : MonoBehaviour
{
    /// <summary>
    /// 需要给 Go 挂载交互脚本
    /// </summary>
    [SerializeField] private XRSimpleInteractable interactableCom;
    [SerializeField] private RouterView routerView;
    private RouterData routerData;
    private bool _isSelected = false;
    
    private void Awake()
    {
        interactableCom.hoverEntered.AddListener(OnHoverEnter);
        interactableCom.hoverExited.AddListener(OnHoverExit);
        interactableCom.selectEntered.AddListener(OnSelectEnter);
    }

    public RouterController(RouterCoreState baseState)
    {
        routerView.BaseState = baseState; // 初始化基本数据
    }

    public void UpdataView(RouterData routerData)
    {
        this.routerData = routerData;
        //routerView.updateView(routerView);
        //routerView.BaseState = routerData.routerCoreStateData;
    }

    void OnHoverEnter(HoverEnterEventArgs args) {
        if (_isSelected) return;
        gameObject.SetActive(true);
    }
    void OnHoverExit(HoverExitEventArgs args) {
        if (_isSelected) return;
        gameObject.SetActive(false);
    }
    void OnSelectEnter(SelectEnterEventArgs args) 
    {
        var isActive = gameObject.activeSelf;
        gameObject.SetActive(!isActive);
    }
    private void OnDestroy()
    {
        interactableCom.hoverEntered.RemoveListener(OnHoverEnter);
        interactableCom.hoverExited.RemoveListener(OnHoverExit);
        interactableCom.selectEntered.RemoveListener(OnSelectEnter);
    }

}
