using TMPro;
using Unity.XR.PXR;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using static UnityEngine.Rendering.DebugUI;

/// <summary>
/// 
/// </summary>
public class KnowledgeView : MonoBehaviour
{
    // 设置标题
    [SerializeField] private TextMeshProUGUI _title;

    // 设置音频
    [SerializeField] private UnityEngine.UI.Button _audioButton;
    // 设置内容
    [SerializeField] private TextMeshProUGUI _text;
    [Tooltip("播放的音频")]
    public AudioClip audioClip;

    bool isPlaying = false;
    private string _audioClipName;
    public string audioClipName {
        set {
            audioClipName = value;   
        }
        get {  return _audioClipName; }
    } // 播放音频的名称

    
    public string Title
    {
        set { 
            _title.text = value;
        }
    }

    public string ExplainText {
        set
        {
            _text.text = value;
        }
    }
    // 根据音频播放音乐
    public AudioClip AudioClipKnowledge {
        set {
            _audioButton.onClick.RemoveAllListeners(); // 移除
            _audioButton.onClick.AddListener(() =>
            {
                // 播放音乐
                AudioManager.Instance.PlayMusic(audioClipName,value,false);
            });

        }
    }

    /// <summary>
    ///  shezhi 
    /// </summary>
    /// <param name="title"></param>
    /// <param name="explainText"></param>
    /// <param name="audioClip"></param>
    public void SetUp(string title , string explainText , AudioClip audioClip)
    {
        AudioClipKnowledge= audioClip;
        Title = title;
        ExplainText = explainText;
    }

    
    // 按钮调用播放
    public void OnButtonClickPlayAudio()
    {
        if (this.audioClip == null)
        {
            Debug.LogError($"audioclip is null .");
            return;
        }
        Debug.Log($"{audioClip.name}");

        isPlaying = !isPlaying;
        if (isPlaying) { 
            AudioManager.Instance.PlayOneBgMusic(this.audioClip, false);
        }
        else
        {
            AudioManager.Instance.StopBGM();
        }

    }
}
