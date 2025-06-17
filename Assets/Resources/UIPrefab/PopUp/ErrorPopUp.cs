using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ErrorPopUp : UIPopup
{

    // 错误信息
    [SerializeField] private TextMeshProUGUI errorMsgCom;//

    // 修改 内容
    public void SetUpErrorMsg(string errorMsg) { 
        errorMsgCom.text = errorMsg;
    }

    public void OnOpen()
    {
        //  发出警告音效
        AudioManager.Instance.PlayMusic(Sound_Effect_Name.ErrorEffect, Resources.Load<AudioClip>(Sound_Effect_Name.Path_AudioEffect + Sound_Effect_Name.ErrorEffect), false);
        // 2s后自动关闭
        CallWaitForSecond(() => {
            UIManager.Instance.CloseTopPopup();
        }, 2f);
    }
    public  override void Open()
    {
        base.Open();
        OnOpen();
    }
    public void  CallWaitForSecond(Action callback,float second)
    {
        StartCoroutine(DoFunc(callback, second));
        IEnumerator DoFunc(Action callback, float second)
        {
            yield return new WaitForSeconds(second);
            callback();
        }
    }
}
