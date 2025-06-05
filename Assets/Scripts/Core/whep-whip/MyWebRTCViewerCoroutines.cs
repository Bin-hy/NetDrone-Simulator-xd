using System.Collections;
using UnityEngine;
using Unity.WebRTC;
// using UnityEngine.Video; // 移除此引用，因为不再直接使用 VideoPlayer

public class MyWebRTCViewerCoroutines : MonoBehaviour
{
    // http://localhost:8080/whep
    // https://live777-guangzhou.binbat.com/whep/web-stream1
    public string whepUrl = "https://live777-guangzhou.binbat.com/whep/web-stream1"; // 替换为你的 WHEP 服务器 URL
    public string authToken = ""; // 你的授权令牌（如果有的话）

    private WHEPClient whepClient;
    private RTCPeerConnection peerConnection;
    [Tooltip("渲染的目标对象，例如一个 Quad")]
    public GameObject videoTarget;

    private Material _videoMaterial; // 用于渲染视频的材质

    IEnumerator Start()
    {
        // 根据官方文档，WebRTC.Update() 仍然需要手动通过协程调用
        // 建议将其放在 Start() 方法的开头
        StartCoroutine(WebRTC.Update());
        Debug.Log("WebRTC.Update() 协程已启动。");

        whepClient = GetComponent<WHEPClient>();
        if (whepClient == null)
        {
            Debug.LogError("在此 GameObject 上找不到 WHEPClient 组件。");
            yield break;
        }

        // 创建一个新的 PeerConnection
        RTCConfiguration config = default;
        // config.iceServers = new RTCIceServer[] { new RTCIceServer { urls = new string[] { "stun:stun.l.google.com:19302" } } }; // 如果需要STUN/TURN服务器
        peerConnection = new RTCPeerConnection(ref config);

        // **关键步骤：注册 OnTrack 事件来处理传入的媒体轨道**
        peerConnection.OnTrack += (RTCTrackEvent e) =>
        {
            if (e.Track.Kind == TrackKind.Video)
            {
                Debug.Log("收到视频轨道！");
                // 转换为 VideoStreamTrack
                VideoStreamTrack videoTrack = (VideoStreamTrack)e.Track;

                if (videoTrack != null)
                {
                    // **根据官方文档，直接使用 VideoStreamTrack.Texture 属性获取渲染纹理**
                    Texture receivedVideoTexture = videoTrack.Texture;

                    if (receivedVideoTexture != null)
                    {
                        // 将 receivedVideoTexture 应用到 videoTarget 的材质上
                        if (videoTarget != null)
                        {
                            Renderer targetRenderer = videoTarget.GetComponent<Renderer>();
                            if (targetRenderer != null)
                            {
                                // 如果材质为空或不是自定义创建的，则创建一个新的 Unlit 材质
                                // 避免修改共享材质实例，并确保使用简单的着色器以优化VR性能
                                if (_videoMaterial == null || targetRenderer.sharedMaterial != _videoMaterial)
                                {
                                    _videoMaterial = new Material(Shader.Find("Unlit/Texture"));
                                    if (_videoMaterial == null)
                                    {
                                        Debug.LogError("找不到 'Unlit/Texture' 着色器，请确保它存在于项目中。");
                                        return; // 无法继续，着色器丢失
                                    }
                                    targetRenderer.material = _videoMaterial; // 将新材质实例分配给渲染器
                                    Debug.Log($"创建新的 Unlit/Texture 材质并分配给 {videoTarget.name}.");
                                }

                                _videoMaterial.mainTexture = receivedVideoTexture; // 设置主纹理
                                Debug.Log($"视频流已连接到 {videoTarget.name} 的材质。");
                            }
                            else
                            {
                                Debug.LogError("videoTarget 没有 Renderer 组件，无法显示视频。请确保它是一个带有 MeshRenderer 的 3D 对象。");
                            }
                        }
                        else
                        {
                            Debug.LogError("videoTarget 未设置。请在 Inspector 中指定一个 GameObject (例如一个 Quad)。");
                        }

                        Debug.Log("WebRTC 视频流已准备好在 3D 对象上显示。");
                    }
                    else
                    {
                        // 初始时 OutputTexture 可能尚未准备好，但这通常是内部管理，随后会填充。
                        // 如果长时间为空，则可能存在问题。
                        Debug.LogWarning("VideoStreamTrack 的 Texture 为空，可能视频流尚未完全准备好。");
                    }
                }
            }
            else if (e.Track.Kind == TrackKind.Audio)
            {
                Debug.Log("收到音频轨道！");
                // Unity.WebRTC 默认情况下会将音频路由到系统默认设备。
                // 如果需要更精细的控制，可以研究 AudioStreamTrack.SetOutputAudioSource() 方法。
                // 例如：
                // AudioStreamTrack audioTrack = (AudioStreamTrack)e.Track;
                // AudioSource audioSource = GetComponent<AudioSource>(); // 确保 GameObject 上有 AudioSource
                // if (audioSource != null)
                // {
                //     audioTrack.SetOutputAudioSource(audioSource);
                //     Debug.Log("音频流已连接到 AudioSource。");
                // }
                // else
                // {
                //     Debug.LogWarning("未找到 AudioSource 组件，无法连接音频流。");
                // }
            }
        };

        peerConnection.OnIceConnectionChange += (RTCIceConnectionState state) =>
        {
            Debug.Log($"ICE 连接状态改变: {state}");
        };
        peerConnection.OnConnectionStateChange += (RTCPeerConnectionState state) =>
        {
            Debug.Log($"对等连接状态改变: {state}");
        };

        // 如果需要在发送/接收 SDP 之前修改 SDP，请设置 SDP 操作回调
        whepClient.OnOffer = (sdp) =>
        {
            Debug.Log("修改传出 offer SDP...");
            return sdp;
        };
        whepClient.OnAnswer = (sdp) =>
        {
            Debug.Log("修改传入 answer SDP...");
            return sdp;
        };

        // 订阅 WHEPClient 事件（一旦 SSE 完全实现）
        whepClient.OnMessage += HandleWHEPMessage;

        yield return StartWHEP();
    }

    /// <summary>
    /// 拉流
    /// </summary>
    /// <returns></returns>
    public IEnumerator StartWHEP()
    {
        try
        {
            Debug.Log("尝试观看 WHEP 流...");
            // whepClient.View 方法内部会触发 SDP 交换和 ICE 收集，最终导致 OnTrack 事件被触发
           StartCoroutine(whepClient.View(peerConnection, whepUrl, authToken));
            Debug.Log("WHEP 观看成功启动。");
        }
        catch (WHEPClient.WHEPClientException ex)
        {
            Debug.LogError($"WHEP 客户端错误: {ex.Message} (HTTP 状态: {ex.Response?.responseCode})"); 
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"发生意外错误: {ex.Message}");
        }
        yield return 0;
    }

    private void HandleWHEPMessage(string message)
    {
        Debug.Log($"收到 WHEP SSE 消息: {message}");
        // 在此处处理 SSE 消息内容
    }

    // 使用 OnDestroy 来确保资源在 GameObject 销毁时被清理
    void OnDestroy()
    {
        StartCoroutine(IEDestroy());
    }

    IEnumerator IEDestroy()
    {
        // 销毁动态创建的材质实例
        if (_videoMaterial != null)
        {
            Destroy(_videoMaterial);
            _videoMaterial = null;
        }

        // 停止 WHEP 客户端连接
        if (whepClient != null)
        {
            yield return StartCoroutine(whepClient.Stop());
        }

        // 关闭并释放 PeerConnection
        if (peerConnection != null)
        {
            peerConnection.Close();
            peerConnection.Dispose();
            peerConnection = null;
        }
    }
}