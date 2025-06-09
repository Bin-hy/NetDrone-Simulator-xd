using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

// 实现一个同时播放音频 和 同速度的打字机的脚本 ，给TextMeshProGUI 使用
public class TextAudioTypingEffect : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI typeText; // 需要输出的文本节点
    [SerializeField] AudioClip myAudio; // 对应的播放音频

    private float printSpeed;  //  打字速度 根据音频和文字时长计算
    IEnumerator _printEnumerator; //

    private string text; // 需要打印的文本 固定不动的. (此处改为[SerializeField]以便在Inspector中设置)

    // SetText 函数已移除，因为您不关注它

    // 停止打字协程和相关音频
    void StopPrintCoroutine()
    {
        if (_printEnumerator == null) return; //
        StopCoroutine(_printEnumerator); // 结束协程

        // 停止BGM通道的音频
        if (AudioManager.Instance != null) // 确保 AudioManager 实例存在
        {
            AudioManager.Instance.StopBGM();
        }
    }
    private void Awake()
    {
        text = this.typeText.text;
    }
    void Start()
    {
        // 每次加载进入时，如果所有必要引用都已设置，则自动开始打字机和音频播放效果
        // 停止当前本地的协程（防止OnEnable或SetText之前的协程冲突）
        StopPrintCoroutine(); // 调用新的停止方法，确保停止音频
        if (typeText != null && !string.IsNullOrEmpty(text))
        {
            _printEnumerator = PrintTextCoroutine(myAudio, typeText, text); // 重新赋值
            StartCoroutine(_printEnumerator);
        }
        else
        {
            Debug.LogWarning("TextAudioTypingEffect: Audio Clip, TextMeshProUGUI or Text is not assigned in Inspector. Auto-start aborted.");
        }
    }

    private void OnEnable()
    {
        // 每次启用时，如果所有必要引用都已设置，则自动开始打字机效果
        StopPrintCoroutine(); // 调用新的停止方法，确保停止音频
        if (typeText != null && !string.IsNullOrEmpty(text))
        {
            _printEnumerator = PrintTextCoroutine(myAudio, typeText, text); // 重新赋值
            StartCoroutine(_printEnumerator);
        }
        // 不需要 else 警告，因为 Start 已经发出了警告
    }

    private void OnDisable()
    {
        // 当组件被禁用时，取消打字协程并停止音频
        StopPrintCoroutine();
    }

    // 音频打字机效果协程
    private IEnumerator PrintTextCoroutine(AudioClip audioClip, TextMeshProUGUI component, string textToPrint)
    {
        if (audioClip == null)
        {
            Debug.LogWarning("AudioClip is null. Cannot play audio for typing effect.");
            yield break; // 退出协程
        }

        var audioTime = audioClip.length; // 计算出音频剪辑的时长

        if (audioTime > 0 && textToPrint.Length > 0)
        {
            // 每个字符的平均时间 = 音频总时长 / 字符总数
            float timePerCharacter = audioTime / textToPrint.Length;
            // printSpeed 表示每秒打印多少个字符，所以是 1 / timePerCharacter
            printSpeed = 1f / timePerCharacter;

            // 确保 printSpeed 不为零或过小，避免 WaitForSeconds 出现问题
            if (printSpeed <= 0) printSpeed = 1f; // 至少每秒打印一个字符
        }
        else
        {
            // 如果没有音频或文本为空，使用一个默认速度
            printSpeed = 20f;
            Debug.LogWarning("Audio clip length or text length is zero. Using default print speed of " + printSpeed);
        }

        // 播放语音
        if (AudioManager.Instance != null) // 确保 AudioManager 实例存在
        {
            AudioManager.Instance.PlayBGM(audioClip, false); // 播放语音
        }
        else
        {
            Debug.LogError("AudioManager.Instance is not found. Cannot play audio. Ensure AudioManager is in the scene.");
            yield break; // 如果没有 AudioManager，就无法播放音频，退出协程
        }

        component.text = ""; // 清空文本，准备开始打字
        for (int i = 0; i < textToPrint.Length; i++)
        {
            component.text += textToPrint[i];
            yield return new WaitForSeconds(1f / printSpeed); //
        }
        Debug.Log("打印完成");

        // 确保打字完成后停止BGM，以防其继续循环或长时间播放
        AudioManager.Instance.StopBGM();
    }
}