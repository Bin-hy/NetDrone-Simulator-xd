using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class LiveController : MonoBehaviour
{
    [Tooltip("live777服务器地址")]
    public string baseUrl = "https://live777.huai-xhy.site";

    private StreamData[] roomStreams; // 当前房间的视频流
    private bool isLoading = false; // 是否正在加载中
    public StreamData[] RoomSteams
    {
        get
        {
            Debug.Log($"current roomStreams is :{roomStreams}");
            //StartCoroutine(GetRooms());
            if(roomStreams == null)
            {
                Debug.LogError("Please call sync function 'LoadRoomStreams' first to get data.");
                return new StreamData[0] ;
            }
            return roomStreams;
        }
    }

    public IEnumerator LoadRoomStreams()
    {
        if (!isLoading)
        {
            yield return  StartCoroutine(GetRooms());
        }
        isLoading = true;
        yield return 0;
    }
    // 获取房间信息
    IEnumerator Start()
    {
        yield return GetRooms();
    }
    public IEnumerator GetRooms()
    {
        string reqUrl = $"{baseUrl}/api/streams/";
        using(var request = UnityWebRequest.Get(reqUrl))
        {
            yield return request.SendWebRequest();
            Debug.Log("开始请求房间信息");
            // 检查错误
            if (request.result == UnityWebRequest.Result.ConnectionError ||
                request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"请求 {request.url} 被状态 {request.responseCode} 拒绝: {request.error},");
                yield break;
            }
            var result = request.result;
            if (request.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = request.downloadHandler.text;
                roomStreams = ProcessStreamData(jsonResponse);
            }
        }
        StreamData[] ProcessStreamData(string json)
        {
            try
            {
                // 使用辅助类解析JSON数组
                StreamData[] streams = JsonHelper.FromJson<StreamData>(json);

                // 处理解析后的数据
                foreach (var stream in streams)
                {
                    Debug.Log($"Stream ID: {stream.id}");

                    // 处理publish sessions
                    if (stream.publish != null && stream.publish.sessions != null)
                    {
                        foreach (var session in stream.publish.sessions)
                        {
                            Debug.Log($"Publish Session: {session.id}");
                            if (session.cascade != null)
                            {
                                Debug.Log($"Cascade Source: {session.cascade.sourceUrl}");
                            }
                        }
                    }

                    // 处理subscribe sessions
                    if (stream.subscribe != null && stream.subscribe.sessions != null)
                    {
                        foreach (var session in stream.subscribe.sessions)
                        {
                            Debug.Log($"Subscribe Session: {session.id}");
                            if (session.cascade != null)
                            {
                                Debug.Log($"Cascade Target: {session.cascade.targetUrl}");
                            }
                        }
                    }
                }

                return streams;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"JsonUtility解析错误: {e.Message}");
                return new StreamData[0];
            }
        }
    }
}
