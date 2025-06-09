public class RouteStep
{
    public string RouterID;         // 当前处理的路由器ID
    public string IncomingInterface; // 数据包进入的接口 (如果适用)
    public string OutgoingInterface; // 数据包离开的接口
    public string NextHop;          // 下一跳地址
    public string Destination;      // 目标网络或设备IP
    public string Action;           // 路由器的动作 (e.g., "查找路由表", "直接转发", "丢弃")
    public string Reason;           // 动作的原因 (e.g., "找到最长匹配路由", "目标直连", "无匹配路由")
}