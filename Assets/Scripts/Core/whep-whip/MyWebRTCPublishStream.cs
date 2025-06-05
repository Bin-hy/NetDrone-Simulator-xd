using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.WebRTC;
using System.Net.WebSockets;
using System.Net.Http;
using UnityEngine.Networking;
using Unity.XR.PXR;

public class CandidateData
{
    public string type;
    public string label;
    //public int id;
    public string candidate;
}

public class SDPData
{
    public string type;
    public string sdp;
}
[System.Serializable]
public class MyObject
{
    public float x;
    public float y;
}
public class MessageData
{
    public MyObject joystick1;
    public MyObject joystick2;
}

public class MyWebRTCPublishStream : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField] private Camera cam;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private string live777ServerUrl = "https://live777.huai-xhy.site";
    [SerializeField] private string streamId = "unity";
#pragma warning restore 0649

    private bool enableCameraModeToggle = false;
    private bool connected;
    private WhipClient whipClient;
    private RTCSessionDescription? sdp;
    private RTCPeerConnection _pc;
    private MediaStream videoStream;
    //private MediaStream audioStream = new MediaStream();

    private static RTCConfiguration GetSelectedSdpSemantics()
    {
        RTCConfiguration config = default;
        var servers = new List<RTCIceServer>();
        string envIceServers = System.Environment.GetEnvironmentVariable("ICE_SERVERS");
        Debug.Log("Use WebRTC IceServers: " + envIceServers);
        if (!string.IsNullOrEmpty(envIceServers))
        {
            servers.Add(new RTCIceServer { urls = new[] { envIceServers } });
        }

        string envTurnHostname = System.Environment.GetEnvironmentVariable("TURN_HOSTNAME");
        Debug.Log("Use WebRTC Turn Server: " + envTurnHostname);
        if (!string.IsNullOrEmpty(envTurnHostname))
        {
            string envTurnUsername = System.Environment.GetEnvironmentVariable("TURN_USERNAME");
            string envTurnPassword = System.Environment.GetEnvironmentVariable("TURN_PASSWORD");
            servers.Add(new RTCIceServer
            {
                urls = new[] { envTurnHostname },
                username = envTurnUsername,
                credential = envTurnPassword,
            });
        }

        if (servers.Count > 0)
        {
            config.iceServers = servers.ToArray();
        }

        return config;
    }

    private IEnumerator OnCreateOffer(RTCPeerConnection pc, RTCSessionDescription desc)
    {
        var op = pc.SetRemoteDescription(ref desc);
        yield return op;
        if (op.IsError)
        {
            Debug.Log(op.Error);
            yield break;
        }
        yield return CreateAnswer(pc);
    }

    private IEnumerator CreateAnswer(RTCPeerConnection pc)
    {
        var op = pc.CreateAnswer();
        yield return op;
        if (!op.IsError)
        {
            yield return OnCreateAnswerSuccess(pc, op.Desc);
        }
        else
        {
            Debug.Log(op.Error);
            yield break;
        }
    }

    private IEnumerator OnCreateAnswerSuccess(RTCPeerConnection pc, RTCSessionDescription desc)
    {
        var op = pc.SetLocalDescription(ref desc);
        yield return op;

        if (!op.IsError)
        {
            SDPData obj = new SDPData()
            {
                type = "answer",
                sdp = desc.sdp,
            };
        }
        else
        {
            Debug.Log(op.Error);
            yield break;
        }
    }

    private IEnumerator AsyncWebRTCCoroutine()
    {
        Debug.Log("=== WebRTC Start ===");
        connected = false;

        _pc.OnIceCandidate = candidate =>
        {
            Debug.Log("ICE: " + candidate.Candidate);
        };

        RTCDataChannelInit conf = new RTCDataChannelInit();
        conf.negotiated = true;
        conf.id = 0;
        var dataChannel = _pc.CreateDataChannel("data", conf);
        dataChannel.OnOpen = () =>
        {

            // TODO: This has a weird problem
            // NOTE: Maybe this a bug for lib
            // Must onopen send a message from the Unity
            // dataChannel.Send("_");
            // === END ===

            Debug.Log("DataChannel Opened");
        };
        dataChannel.OnClose = () =>
        {
            Debug.Log("DataChannel Closed");
        };


        _pc.OnIceConnectionChange = state =>
        {
            Debug.Log($"IceConnectionState: {state}");
            if (state == RTCIceConnectionState.Disconnected)
            {
                connected = false;
            }
        };

        yield return 0;
    }

    void Start()
    {
        Debug.Log("=== Start !! ===");
        //string whipServerUrl = System.Environment.GetEnvironmentVariable("WHIP_SERVER_URL");
        Debug.Log(cam);
        
        videoStream =  cam.CaptureStream(1280, 720);
        audioSource = GetComponent<AudioSource>();

        StartCoroutine(WebRTC.Update());
        var configuration = GetSelectedSdpSemantics();
        _pc = new RTCPeerConnection(ref configuration);

        foreach (var track in videoStream.GetTracks())
        {
            Debug.Log("video track: " + track);
            _pc.AddTrack(track, videoStream);
        }
        //foreach (var track in audioStream.GetTracks())
        //{
        //    Debug.Log("audio track: " + track);
        //    _pc.AddTrack(track, audioStream);
        //}
        StartCoroutine(AsyncWebRTCCoroutine());

        StartWHIP();

        Debug.Log("=== Start END ===");
    }

    private void OnDestroy()
    {
        StartCoroutine(closeStream());
    }

    IEnumerator closeStream()
    {
        var destroyUrl = $"{live777ServerUrl}/api/streams/{streamId}";
        using (var httpClient = new UnityWebRequest(live777ServerUrl, "DELETE"))
        {
            yield return httpClient.SendWebRequest();
        }
        yield return 0;
    }
    void StartWHIP()
    {
        Debug.Log("=== StartWHIP ===");
        //string whipServerUrl = System.Environment.GetEnvironmentVariable("WHIP_SERVER_URL");
        string whipUrl = $"{live777ServerUrl}/whip/{streamId}";
        Debug.Log("whipServerUrl: " + whipUrl);
        whipClient = new WhipClient(this);
        StartCoroutine(whipClient.Publish(_pc, whipUrl, ""));
    }

}
