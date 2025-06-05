using System;
using System.Collections.Generic;

[Serializable]
public class Session
{
    public string id;
    public long createdAt;
    public string state;
    public Cascade cascade;
}

[Serializable]
public class Cascade
{
    public string sourceUrl;
    public string sessionUrl;
    public string targetUrl;
}

[Serializable]
public class PublishSubscribeInfo
{
    public long leaveAt;
    public List<Session> sessions;
}

[Serializable]
public class StreamData
{
    public string id;
    public long createdAt;
    public PublishSubscribeInfo publish;
    public PublishSubscribeInfo subscribe;
}

[Serializable]
public class StreamList
{
    public List<StreamData> streams;
}

