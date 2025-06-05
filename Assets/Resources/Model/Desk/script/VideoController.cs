using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class VideoController : MonoBehaviour
{
    private VideoPlayer videoPlayer;
    void Awake()
    {
        // 获取组件引用
        videoPlayer = GetComponent<VideoPlayer>();

        // 设置VideoPlayer初始配置
        videoPlayer.playOnAwake = true;
        videoPlayer.isLooping = true;
        videoPlayer.renderMode = VideoRenderMode.MaterialOverride;
        videoPlayer.targetMaterialRenderer = GetComponent<Renderer>();
        videoPlayer.targetMaterialProperty = "_MainTex";
        // 设置循环播放
        videoPlayer.isLooping = true;

        // 准备并播放视频
        videoPlayer.Prepare();
        videoPlayer.prepareCompleted += (VideoPlayer vp) => {
            vp.Play();
        };

    }
}
