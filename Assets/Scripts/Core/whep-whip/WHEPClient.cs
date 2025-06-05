using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Unity.WebRTC;
using System.Linq; // 假设你已导入 Unity.WebRTC 包

/// <summary>
/// 定义 WHEP 核心扩展 URN。
/// </summary>
public static class Extensions
{
    public static class Core
    {
        public const string ServerSentEvents = "urn:ietf:params:whep:ext:core:server-sent-events";
        public const string Layer = "urn:ietf:params:whep:ext:core:layer";
    }
}


/// <summary>
/// 表示一个用于 Unity 的 WHEP (WebRTC-HTTP Egress Protocol) 客户端。
/// </summary>
public class WHEPClient : MonoBehaviour
{
    // Ice 属性
    private string iceUsername = null;
    private string icePassword = null;

    // 待处理的候选者
    private List<RTCIceCandidate> candidates = new List<RTCIceCandidate>();
    private bool endOfCandidates = false;
    private Coroutine iceTrickleTimeoutCoroutine;
    private DateTime? restartIceTime = null;

    // WebRTC Peer Connection
    private RTCPeerConnection pc;
    private string token;
    private Uri resourceURL;
    private Uri eventsUrl;
    private Uri layerUrl;
    private string etag;

    // SDP 操作的回调
    public Func<string, string> OnOffer;
    public Func<string, string> OnAnswer;

    // 事件
    public event Action<string> OnMessage; // SSE 消息的占位符

    private void Awake()
    {
        // 如果未设置，则初始化默认回调
        OnOffer = (offer) => offer;
        OnAnswer = (answer) => answer;
    }

    /// <summary>
    /// 包含 UnityWebRequest 错误详情的自定义异常。
    /// </summary>
    public class WHEPClientException : Exception
    {
        public UnityWebRequest Response { get; private set; }

        public WHEPClientException(string message, UnityWebRequest response) : base(message)
        {
            Response = response;
        }
    }
    private void ThrowWithResponse(string msg, UnityWebRequest response)
    {
        throw new WHEPClientException(msg, response);
    }

    /// <summary>
    /// 启动 WHEP 观看过程。
    /// </summary>
    /// <param name="peerConnection">要使用的 RTCPeerConnection。</param>
    /// <param name="url">WHEP 端点 URL。</param>
    /// <param name="authToken">可选的授权令牌。</param>
    public IEnumerator View(RTCPeerConnection peerConnection, string url, string authToken = null)
    {
        if (this.pc != null)
        {
            throw new InvalidOperationException("已经在观看。");
        }

        this.token = authToken;
        this.pc = peerConnection;

        // 监听 ICE 候选者
        pc.OnIceCandidate += (candidate) =>
        {
            if (candidate.Candidate != null)
            {
                // 忽略非来自第一个 m-line 的候选者
                if (candidate.SdpMLineIndex > 0)
                {
                    return;
                }
                candidates.Add(candidate);
            }
            else
            {
                // 没有更多候选者
                endOfCandidates = true;
            }

            // 如果没有正在运行的计时器或未进行重新启动，则安排在下一个 tick 进行补丁
            if (iceTrickleTimeoutCoroutine == null && !restartIceTime.HasValue)
            {
                iceTrickleTimeoutCoroutine = StartCoroutine(SchedulePatchOnNextTick());
            }
        };

        // 创建 SDP offer
        var createOfferOperation = pc.CreateOffer();
        yield return createOfferOperation; // 等待创建 offer 操作完成

        RTCSessionDescription offerResult = createOfferOperation.Desc;
        if (offerResult.sdp != null)
        {
            offerResult.sdp = OnOffer(offerResult.sdp);
        }
        else
        {
            throw new Exception("创建 SDP offer 失败。");
        }

        // 准备 HTTP 头
        var headers = new Dictionary<string, string>
        {
            { "Content-Type", "application/sdp" }
        };
        if (!string.IsNullOrEmpty(token))
        {
            headers["Authorization"] = "Bearer " + token;
        }

        // 向 WHEP 端点发送 POST 请求
        using (var request = UnityWebRequest.Post(url, offerResult.sdp, "application/sdp"))
        {
            foreach (var header in headers)
            {
                request.SetRequestHeader(header.Key, header.Value);
            }

            yield return request.SendWebRequest(); // 等待请求完成

            if (request.result != UnityWebRequest.Result.Success)
            {
                ThrowWithResponse($"请求被状态 {request.responseCode} 拒绝: {request.error}", request);
            }

            string locationHeader = request.GetResponseHeader("location");
            if (string.IsNullOrEmpty(locationHeader))
            {
                ThrowWithResponse("响应缺少 location 头", request);
            }

            this.resourceURL = new Uri(new Uri(url), locationHeader);

            // 解析 Link 头
            var links = ParseLinkHeaders(request.GetResponseHeader("link"), url);

            if (links.ContainsKey(Extensions.Core.ServerSentEvents))
            {
                this.eventsUrl = new Uri(new Uri(url), links[Extensions.Core.ServerSentEvents][0].url);
            }
            if (links.ContainsKey(Extensions.Core.Layer))
            {
                this.layerUrl = new Uri(new Uri(url), links[Extensions.Core.Layer][0].url);
            }

            // 处理 Server-Sent Events
            if (this.eventsUrl != null)
            {
                var events = new List<string> { "active", "inactive", "layers", "viewercount" };
                if (links.ContainsKey(Extensions.Core.ServerSentEvents) && links[Extensions.Core.ServerSentEvents][0].Params.TryGetValue("events", out string eventParam))
                {
                    events = new List<string>(eventParam.Split(','));
                }

                var sseHeaders = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                };
                if (!string.IsNullOrEmpty(this.token))
                {
                    sseHeaders["Authorization"] = "Bearer " + this.token;
                }
                //string str = JsonUtility.ToJson(new StringArrayWrapper(events.ToArray()));
                //Encoding.UTF8.GetBytes(str);
                using (var sseRequest = UnityWebRequest.Post(this.eventsUrl.ToString(), JsonUtility.ToJson(new StringArrayWrapper(events.ToArray())), "application/json"))
                {
                    foreach (var header in sseHeaders)
                    {
                        sseRequest.SetRequestHeader(header.Key, header.Value);
                    }

                    yield return sseRequest.SendWebRequest(); // 等待 SSE 请求完成

                    if (sseRequest.result == UnityWebRequest.Result.Success)
                    {
                        string sseLocation = sseRequest.GetResponseHeader("location");
                        if (!string.IsNullOrEmpty(sseLocation))
                        {
                            Uri sseFinalUrl = new Uri(this.eventsUrl, sseLocation);
                            Debug.Log($"WHEP: SSE URL: {sseFinalUrl}");
                            // TODO: 为 Unity 实现实际的 SSE 客户端。
                            // 这是一个占位符。你通常会在这里打开一个长轮询连接或 WebSocket。
                            StartCoroutine(SimulateSSE(sseFinalUrl));
                        }
                    }
                }
            }

            // 如果 WHEP 端点提供 ICE 服务器信息且客户端未覆盖，则更新 ICE 服务器
            var config = pc.GetConfiguration();
            if ((config.iceServers == null || config.iceServers.Length == 0) && links.ContainsKey("ice-server"))
            {
                var iceServersList = new List<RTCIceServer>();
                foreach (var serverLink in links["ice-server"])
                {
                    var iceServer = new RTCIceServer
                    {
                        urls = new string[] { serverLink.url }
                    };

                    if (serverLink.Params.TryGetValue("username", out string username))
                    {
                        iceServer.username = username;
                    }
                    if (serverLink.Params.TryGetValue("credential", out string credential))
                    {
                        iceServer.credential = credential;
                    }
                    iceServersList.Add(iceServer);
                }

                if (iceServersList.Count > 0)
                {
                    config.iceServers = iceServersList.ToArray();
                    pc.SetConfiguration(ref config);
                }
            }

            string answerSdp = request.downloadHandler.text;

            // 在下一个 tick 调度 trickle
            if (iceTrickleTimeoutCoroutine == null)
            {
                iceTrickleTimeoutCoroutine = StartCoroutine(SchedulePatchOnNextTick());
            }

            var setLocalDescriptionOperation = pc.SetLocalDescription(ref offerResult);
            yield return setLocalDescriptionOperation; // 等待设置本地描述完成

            // 从本地描述中提取 ICE 属性
            var sdpLines = offerResult.sdp.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in sdpLines)
            {
                if (line.StartsWith("a=ice-ufrag:"))
                {
                    iceUsername = line.Substring("a=ice-ufrag:".Length);
                }
                else if (line.StartsWith("a=ice-pwd:"))
                {
                    icePassword = line.Substring("a=ice-pwd:".Length);
                }
            }

            var answerDescription = new RTCSessionDescription { type = RTCSdpType.Answer, sdp = OnAnswer(answerSdp) };
            var setRemoteDescriptionOperation = pc.SetRemoteDescription(ref answerDescription);
            yield return setRemoteDescriptionOperation; // 等待设置远程描述完成
        }
    }

    /// <summary>
    /// 重新启动当前连接的 ICE。
    /// </summary>
    public IEnumerator Restart()
    {
        if (iceTrickleTimeoutCoroutine != null)
        {
            StopCoroutine(iceTrickleTimeoutCoroutine);
            iceTrickleTimeoutCoroutine = null;
        }

        candidates.Clear();
        endOfCandidates = false;

        pc.RestartIce();
        var rtcOfferAnswerOps = new RTCOfferAnswerOptions { iceRestart = true };
        var createOfferOperation = pc.CreateOffer(ref rtcOfferAnswerOps);
        yield return createOfferOperation; // 等待创建 offer 操作完成

        RTCSessionDescription offerResult = createOfferOperation.Desc;

        var sdpLines = offerResult.sdp.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var line in sdpLines)
        {
            if (line.StartsWith("a=ice-ufrag:"))
            {
                iceUsername = line.Substring("a=ice-ufrag:".Length);
            }
            else if (line.StartsWith("a=ice-pwd:"))
            {
                icePassword = line.Substring("a=ice-pwd:".Length);
            }
        }

        var setLocalDescriptionOperation = pc.SetLocalDescription(ref offerResult);
        yield return setLocalDescriptionOperation; // 等待设置本地描述完成

        restartIceTime = DateTime.UtcNow;

        if (iceTrickleTimeoutCoroutine != null)
        {
            StopCoroutine(iceTrickleTimeoutCoroutine);
            iceTrickleTimeoutCoroutine = null;
        }

        yield return StartCoroutine(Patch()); // 调用 Patch 协程
    }

    /// <summary>
    /// 发送 ICE 候选者或执行 ICE 重新启动补丁。
    /// </summary>
    public IEnumerator Patch()
    {
        if (iceTrickleTimeoutCoroutine != null)
        {
            StopCoroutine(iceTrickleTimeoutCoroutine);
            iceTrickleTimeoutCoroutine = null;
        }

        if (!(candidates.Count > 0 || endOfCandidates || restartIceTime.HasValue) || resourceURL == null)
        {
            yield break; // 什么都不做
        }

        var currentCandidates = new List<RTCIceCandidate>(candidates);
        bool currentEndOfCandidates = endOfCandidates;
        DateTime? currentRestartIceTime = restartIceTime;

        candidates.Clear();
        endOfCandidates = false;

        var fragmentBuilder = new StringBuilder();
        fragmentBuilder.AppendLine($"a=ice-ufrag:{iceUsername}");
        fragmentBuilder.AppendLine($"a=ice-pwd:{icePassword}");

        // 将迭代器转换为列表，以便我们可以多次遍历或按索引访问（如果需要）
        // 尽管这里我们主要通过 Mid 查找，但转换为列表更安全
        var transceiversList = pc.GetTransceivers().ToList(); // 修正：使用 ToList()

        var medias = new Dictionary<string, MediaInfo>();

        // 如果有候选者或结束标志，尝试为第一个收发器创建媒体信息
        if (currentCandidates.Count > 0 || currentEndOfCandidates)
        {
            // 修正：确保 transceiversList 不为空，并获取第一个元素
            if (transceiversList.Any()) // 检查列表是否包含任何元素
            {
                var firstTransceiver = transceiversList.First(); // 获取第一个元素
                medias[firstTransceiver.Mid] = new MediaInfo
                {
                    Mid = firstTransceiver.Mid,
                    Kind = firstTransceiver.Receiver.Track.Kind.ToString(),
                    Candidates = new List<RTCIceCandidate>()
                };
            }
        }

        foreach (var candidate in currentCandidates)
        {
            if (!medias.TryGetValue(candidate.SdpMid, out MediaInfo media))
            {
                // 修正：在 transceiversList 中查找
                var transceiver = transceiversList.Find(t => t.Mid == candidate.SdpMid);
                if (transceiver != null)
                {
                    media = new MediaInfo
                    {
                        Mid = transceiver.Mid,
                        Kind = transceiver.Receiver.Track.Kind.ToString(),
                        Candidates = new List<RTCIceCandidate>()
                    };
                    medias[candidate.SdpMid] = media;
                }
            }
            media?.Candidates.Add(candidate);
        }

        foreach (var media in medias.Values)
        {
            fragmentBuilder.AppendLine($"m={media.Kind} 9 UDP/TLS/RTP/SAVPF 0");
            fragmentBuilder.AppendLine($"a=mid:{media.Mid}");
            foreach (var candidate in media.Candidates)
            {
                fragmentBuilder.AppendLine($"a={candidate.Candidate}");
            }
            if (currentEndOfCandidates)
            {
                fragmentBuilder.AppendLine("a=end-of-candidates");
            }
        }

        var headers = new Dictionary<string, string>
        {
            { "Content-Type", "application/trickle-ice-sdpfrag" }
        };

        if (currentRestartIceTime.HasValue)
        {
            headers["If-Match"] = "*";
        }
        else if (!string.IsNullOrEmpty(etag))
        {
            headers["If-Match"] = etag;
        }

        if (!string.IsNullOrEmpty(token))
        {
            headers["Authorization"] = "Bearer " + token;
        }

        using (var request = UnityWebRequest.Put(resourceURL.ToString(), Encoding.UTF8.GetBytes(fragmentBuilder.ToString())))
        {
            request.method = "PATCH"; // UnityWebRequest.Put 默认为 PUT，手动改为 PATCH
            foreach (var header in headers)
            {
                request.SetRequestHeader(header.Key, header.Value);
            }

            yield return request.SendWebRequest(); // 等待请求完成

            if (request.result != UnityWebRequest.Result.Success && request.responseCode != 501 && request.responseCode != 405)
            {
                ThrowWithResponse($"请求被状态 {request.responseCode} 拒绝: {request.error}", request);
            }

            // 处理 ICE 重新启动响应
            if (currentRestartIceTime.HasValue && request.responseCode == 200)
            {
                etag = request.GetResponseHeader("etag");
                string answerSdp = request.downloadHandler.text;

                string newIceUsername = ExtractSdpValue(answerSdp, "a=ice-ufrag:");
                string newIcePassword = ExtractSdpValue(answerSdp, "a=ice-pwd:");
                var remoteCandidates = ExtractSdpCandidates(answerSdp);

                var remoteDescription = pc.RemoteDescription;
                string updatedSdp = remoteDescription.sdp;

                // 替换 ICE ufrag 和 pwd
                updatedSdp = System.Text.RegularExpressions.Regex.Replace(updatedSdp, @"a=ice-ufrag:(.*)\r\n", $"a=ice-ufrag:{newIceUsername}\r\n");
                updatedSdp = System.Text.RegularExpressions.Regex.Replace(updatedSdp, @"a=ice-pwd:(.*)\r\n", $"a=ice-pwd:{newIcePassword}\r\n");

                // 删除现有候选者并添加新候选者
                updatedSdp = System.Text.RegularExpressions.Regex.Replace(updatedSdp, @"a=candidate:.*\r\n", "");
                updatedSdp = System.Text.RegularExpressions.Regex.Replace(updatedSdp, @"(m=.*\r\n)", $"$1{string.Join("", remoteCandidates)}");

                var newRemoteDescription = new RTCSessionDescription { type = RTCSdpType.Answer, sdp = updatedSdp };
                var setRemoteDescriptionOperation = pc.SetRemoteDescription(ref newRemoteDescription);
                yield return setRemoteDescriptionOperation; // 等待设置远程描述完成

                if (this.restartIceTime == currentRestartIceTime)
                {
                    this.restartIceTime = null;
                    if (candidates.Count > 0 || endOfCandidates)
                    {
                        iceTrickleTimeoutCoroutine = StartCoroutine(SchedulePatchOnNextTick());
                    }
                }
            }
        }
    }

    /// <summary>
    /// 静音/取消静音客户端的音频/视频。
    /// </summary>
    /// <param name="muted">true 为静音，false 为取消静音。</param>
    public IEnumerator Mute(bool muted)
    {
        if (resourceURL == null)
        {
            Debug.LogWarning("WHEP 资源 URL 不可用，无法静音。");
            yield break;
        }

        var headers = new Dictionary<string, string>
        {
            { "Content-Type", "application/json" }
        };
        if (!string.IsNullOrEmpty(token))
        {
            headers["Authorization"] = "Bearer " + token;
        }

        using (var request = UnityWebRequest.Post(resourceURL.ToString(), JsonUtility.ToJson(muted), "application/json"))
        {
            foreach (var header in headers)
            {
                request.SetRequestHeader(header.Key, header.Value);
            }
            yield return request.SendWebRequest(); // 等待请求完成
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"静音请求失败: {request.responseCode} - {request.error}");
            }
        }
    }

    /// <summary>
    /// 选择用于视频流的特定层。
    /// </summary>
    /// <param name="layer">要选择的层对象。</param>
    public IEnumerator SelectLayer(object layer) // 使用 object 或自定义结构/类作为“layer”
    {
        if (layerUrl == null)
        {
            throw new InvalidOperationException("WHEP 资源不支持层选择。");
        }

        var headers = new Dictionary<string, string>
        {
            { "Content-Type", "application/json" }
        };
        if (!string.IsNullOrEmpty(token))
        {
            headers["Authorization"] = "Bearer " + token;
        }

        string jsonBody = JsonUtility.ToJson(layer); // 确保“layer”可序列化
        using (var request = UnityWebRequest.Post(layerUrl.ToString(), jsonBody, "application/json"))
        {
            foreach (var header in headers)
            {
                request.SetRequestHeader(header.Key, header.Value);
            }
            yield return request.SendWebRequest(); // 等待请求完成
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"选择层请求失败: {request.responseCode} - {request.error}");
            }
        }
    }

    /// <summary>
    /// 取消选择任何活动层。
    /// </summary>
    public IEnumerator UnselectLayer()
    {
        if (layerUrl == null)
        {
            throw new InvalidOperationException("WHEP 资源不支持层选择。");
        }

        var headers = new Dictionary<string, string>();
        if (!string.IsNullOrEmpty(token))
        {
            headers["Authorization"] = "Bearer " + token;
        }

        using (var request = UnityWebRequest.Delete(layerUrl.ToString()))
        {
            foreach (var header in headers)
            {
                request.SetRequestHeader(header.Key, header.Value);
            }
            yield return request.SendWebRequest(); // 等待请求完成
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"取消选择层请求失败: {request.responseCode} - {request.error}");
            }
        }
    }

    /// <summary>
    /// 停止 WHEP 会话并关闭对等连接。
    /// </summary>
    public IEnumerator Stop()
    {
        if (pc == null)
        {
            // 已经停止
            yield break;
        }

        // 取消任何待处理的超时
        if (iceTrickleTimeoutCoroutine != null)
        {
            StopCoroutine(iceTrickleTimeoutCoroutine);
            iceTrickleTimeoutCoroutine = null;
        }

        pc.Close();
        pc = null;

        if (resourceURL == null)
        {
            throw new InvalidOperationException("WHEP 资源 URL 尚不可用，无法停止。");
        }

        var headers = new Dictionary<string, string>();
        if (!string.IsNullOrEmpty(token))
        {
            headers["Authorization"] = "Bearer " + token;
        }

        using (var request = UnityWebRequest.Delete(resourceURL.ToString()))
        {
            foreach (var header in headers)
            {
                request.SetRequestHeader(header.Key, header.Value);
            }
            yield return request.SendWebRequest(); // 等待请求完成
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"停止请求失败: {request.responseCode} - {request.error}");
            }
        }
    }

    // --- 辅助方法 ---

    private IEnumerator SchedulePatchOnNextTick()
    {
        yield return null; // 等待一帧
        StartCoroutine(Patch()); // 异步调用 Patch 协程
        iceTrickleTimeoutCoroutine = null;
    }

    private Dictionary<string, List<LinkHeader>> ParseLinkHeaders(string linkHeaderValue, string baseUrl)
    {
        var links = new Dictionary<string, List<LinkHeader>>();
        if (string.IsNullOrEmpty(linkHeaderValue))
        {
            return links;
        }

        string[] linkHeaders = linkHeaderValue.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
        foreach (string header in linkHeaders)
        {
            try
            {
                string[] parts = header.Split(';');
                string urlPart = parts[0].Trim().Replace("<", "").Replace(">", "");
                string rel = null;
                var parameters = new Dictionary<string, string>();

                for (int i = 1; i < parts.Length; i++)
                {
                    string[] subItems = parts[i].Split(new[] { '=' }, 2);
                    if (subItems.Length == 2)
                    {
                        string key = subItems[0].Trim();
                        string value = subItems[1].Trim().Trim('"', '\'');
                        if (key == "rel")
                        {
                            rel = value;
                        }
                        else
                        {
                            parameters[key] = value;
                        }
                    }
                }

                if (!string.IsNullOrEmpty(rel))
                {
                    if (!links.ContainsKey(rel))
                    {
                        links[rel] = new List<LinkHeader>();
                    }
                    links[rel].Add(new LinkHeader { url = new Uri(new Uri(baseUrl), urlPart).ToString(), Params = parameters });
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"解析 Link 头部分 '{header}' 时出错: {e.Message}");
            }
        }
        return links;
    }

    private string ExtractSdpValue(string sdp, string prefix)
    {
        var match = System.Text.RegularExpressions.Regex.Match(sdp, $@"{System.Text.RegularExpressions.Regex.Escape(prefix)}(.*)\r\n");
        return match.Success ? match.Groups[1].Value : null;
    }

    private List<string> ExtractSdpCandidates(string sdp)
    {
        var matches = System.Text.RegularExpressions.Regex.Matches(sdp, @"(a=candidate:.*\r\n)");
        var candidatesList = new List<string>();
        foreach (System.Text.RegularExpressions.Match match in matches)
        {
            candidatesList.Add(match.Groups[1].Value);
        }
        return candidatesList;
    }

    // SSE 的简单占位符。在实际应用中，你将使用更健壮的 SSE 客户端库或实现持久 HTTP/WebSocket 连接。
    private IEnumerator SimulateSSE(Uri sseUrl)
    {
        // 这是一个非常基本的模拟。
        // 真正的 SSE 实现将涉及长轮询或 WebSockets。
        while (pc != null && pc.ConnectionState != RTCPeerConnectionState.Closed)
        {
            // 在实际场景中，你将在这里发出请求并解析事件。
            // 目前，只是一个调试消息。
            // Debug.Log($"模拟 SSE 连接到: {sseUrl}");
            yield return new WaitForSeconds(5f); // 模拟定期检查
            // OnMessage?.Invoke("模拟 WHEP 事件！"); // 用虚拟数据调用事件
        }
    }

    [System.Serializable]
    private class StringArrayWrapper
    {
        public string[] events;
        public StringArrayWrapper(string[] events)
        {
            this.events = events;
        }
    }

    private class LinkHeader
    {
        public string url;
        public Dictionary<string, string> Params;
    }

    private class MediaInfo
    {
        public string Mid;
        public string Kind;
        public List<RTCIceCandidate> Candidates;
    }
}