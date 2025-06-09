using System;
using System.Net; // For IPAddress class, though you can use string for simplicity if preferred

public static class IPAddressUtility
{
    // 将IP地址字符串转换为长整型 (IPv4)
    public static long IpAddressToLong(string ipAddress)
    {
        System.Net.IPAddress ip;
        if (System.Net.IPAddress.TryParse(ipAddress, out ip))
        {
            byte[] bytes = ip.GetAddressBytes();
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes); // For consistency, convert to big-endian
            }
            return BitConverter.ToUInt32(bytes, 0); // Use UInt32 and cast to long
        }
        return 0; // Invalid IP
    }

    // 将子网掩码字符串转换为长整型 (IPv4)
    public static long SubnetMaskToLong(string subnetMask)
    {
        return IpAddressToLong(subnetMask);
    }

    // 将CIDR转换为子网掩码字符串
    public static string CidrToSubnetMask(int cidr)
    {
        if (cidr < 0 || cidr > 32) return "Invalid CIDR";
        uint mask = 0xffffffff << (32 - cidr);
        byte[] bytes = BitConverter.GetBytes(mask);
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(bytes);
        }
        return new IPAddress(bytes).ToString();
    }

    // 判断两个IP地址是否在同一个子网 (通过网络地址和子网掩码)
    public static bool IsSameSubnet(string ip1, string subnetMask1, string ip2, string subnetMask2)
    {
        long ip1Long = IpAddressToLong(ip1);
        long subnetMask1Long = SubnetMaskToLong(subnetMask1);
        long networkAddress1 = ip1Long & subnetMask1Long;

        long ip2Long = IpAddressToLong(ip2);
        long subnetMask2Long = SubnetMaskToLong(subnetMask2);
        long networkAddress2 = ip2Long & subnetMask2Long;

        return networkAddress1 == networkAddress2 && subnetMask1Long == subnetMask2Long;
    }

    // 获取网络地址
    public static string GetNetworkAddress(string ipAddress, string subnetMask)
    {
        long ipLong = IpAddressToLong(ipAddress);
        long subnetMaskLong = SubnetMaskToLong(subnetMask);
        long networkAddressLong = ipLong & subnetMaskLong;
        byte[] bytes = BitConverter.GetBytes(networkAddressLong);
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(bytes);
        }
        return new IPAddress(bytes).ToString();
    }
}