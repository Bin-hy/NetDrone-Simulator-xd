using System;
using System.Linq;

[Serializable]
public class IPAddress
{
    public byte[] AddressBytes { get; private set; }

    public IPAddress(string ipAddressString)
    {
        var parts = ipAddressString.Split('.');
        if (parts.Length != 4 || !parts.All(p => byte.TryParse(p, out _)))
        {
            throw new ArgumentException("Invalid IP address format.");
        }
        AddressBytes = parts.Select(byte.Parse).ToArray();
    }

    public IPAddress(byte[] addressBytes)
    {
        if (addressBytes == null || addressBytes.Length != 4)
        {
            throw new ArgumentException("IP address must consist of 4 bytes.");
        }
        AddressBytes = addressBytes;
    }

    public static IPAddress Parse(string ipAddressString)
    {
        return new IPAddress(ipAddressString);
    }

    public override string ToString()
    {
        return string.Join(".", AddressBytes);
    }

    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }
        IPAddress other = (IPAddress)obj;
        return AddressBytes.SequenceEqual(other.AddressBytes);
    }

    public override int GetHashCode()
    {
        return BitConverter.ToInt32(AddressBytes, 0);
    }

    // 计算网络地址
    public IPAddress GetNetworkAddress(IPAddress subnetMask)
    {
        byte[] networkAddressBytes = new byte[4];
        for (int i = 0; i < 4; i++)
        {
            networkAddressBytes[i] = (byte)(AddressBytes[i] & subnetMask.AddressBytes[i]);
        }
        return new IPAddress(networkAddressBytes);
    }
}