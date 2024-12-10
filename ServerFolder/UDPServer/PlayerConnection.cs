using System;
using System.Net;

class PlayerConnection
{
    // 클라이언트의 IP 및 포트 정보를 포함한 EndPoint
    public IPEndPoint RemoteEndPoint { get; set; }

    // 생성자
    public PlayerConnection(IPEndPoint iPEndPoint)
    {
        RemoteEndPoint = new IPEndPoint(iPEndPoint.Address, iPEndPoint.Port);
    }

    // ToString 메서드로 디버깅 및 로그용 정보 제공
    public override string ToString()
    {
        return $"IP: {RemoteEndPoint.Address}, Port: {RemoteEndPoint.Port}";
    }
}
