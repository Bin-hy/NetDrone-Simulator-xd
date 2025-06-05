using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine : MonoBehaviour
{

    // 1. 状态委托类型
    private delegate void StateDelegate();
    // 2. 状态字典
    private Dictionary<JoyStickState, StateDelegate> stateEnterDelegates;
    private Dictionary<JoyStickState, StateDelegate> stateUpdateDelegates;
    private Dictionary<JoyStickState, StateDelegate> stateExitDelegates;
    // 3. 当前状态
    private JoyStickState currentState;
    public JoyStickState CurrentState => currentState;

    // 4.状态改变事件
    public event Action<JoyStickState> OnStateEnter;
    public event Action<JoyStickState> OnStateExit;
    private void Awake()
    {
        stateEnterDelegates = new Dictionary<JoyStickState, StateDelegate>();
        stateUpdateDelegates = new Dictionary<JoyStickState, StateDelegate>();
        stateExitDelegates = new Dictionary<JoyStickState, StateDelegate>();

        
    }

    private void RegisterState(JoyStickState state,
                             StateDelegate enter,
                             StateDelegate update,
                             StateDelegate exit)
    {
        stateEnterDelegates[state] = enter;
        stateUpdateDelegates[state] = update;
        stateExitDelegates[state] = exit;
    }
    private void Start()
    {
        currentState = JoyStickState.Character;
    }

    public void changeState(JoyStickState newState)
    {
        if (currentState == newState) return;

        // 调用退出当前状态的方法
        stateEnterDelegates[currentState]?.Invoke();
        OnStateExit?.Invoke(currentState);
        // 更新状态
        currentState = newState;
        stateEnterDelegates[currentState]?.Invoke();
        OnStateEnter?.Invoke(currentState);
    }
    // 每帧更新
    void Update()
    {
        stateUpdateDelegates[currentState]?.Invoke();
    }

}
