using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerInfo
{
    public string ipAddress;
    public int tcpPort;
    public int udpPort;

    public ServerInfo(string ip, int tcp, int udp)
    {
        ipAddress = ip;
        tcpPort = tcp;
        udpPort = udp;
    }
}
