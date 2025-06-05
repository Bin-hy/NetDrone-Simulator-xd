using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ControllerBase : MonoBehaviour
{   
    public bool isControled =false;
    // 处理控制逻辑
    protected abstract void HandleInput();
    protected abstract void LoadInput();
    protected virtual void Update()
    {
        if (!isControled) return;
        HandleInput();
    }
}
