using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

// 负责展现当前关联的文本的打字机效果
// 具有自己Update
public class TextTypingMachineEffect : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI typeText;
    [SerializeField] private int printSpeed;  //  打字速度
    
    private string text;// 需要打印的文本

    // 通用文本更新逻辑
    public void SetText( string text, bool isPrint)
    {
        if (isPrint)
        {
            StartCoroutine(PrintTextCoroutine(typeText, text));
        }
        else
        {
            typeText.text = text;
        }
    }

    void Start()
    {
        // 每次加载进入时候是打字机器效果
        StartCoroutine(PrintTextCoroutine(typeText,text));
    }

    private void OnEnable()
    {
        // 每次加载进入时候是打字机器效果
        StartCoroutine(PrintTextCoroutine(typeText, text));
    }


    // 打字机效果协程
    private IEnumerator PrintTextCoroutine(TextMeshProUGUI component, string text)
    {
        component.text = "";
        for (int i = 0; i < text.Length; i++)
        {
            component.text += text[i];
            yield return new WaitForSeconds(1f / printSpeed);

        }
        Debug.Log("打印完成");
    }
}
