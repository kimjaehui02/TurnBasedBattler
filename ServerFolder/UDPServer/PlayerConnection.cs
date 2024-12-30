using System;
using System.Net;
using System.Net.Sockets;

class PlayerConnection
{
    // 클라이언트의 IP 및 포트 정보를 포함한 EndPoint
    //public IPEndPoint RemoteEndPoint { get; set; }

    // TcpClient 인스턴스를 private로 선언
    private TcpClient asyncToClient;

    // 생성자
    public PlayerConnection(TcpClient asyncToClient)
    {
        this.asyncToClient = asyncToClient;
    }

    // asyncToClient에 대한 프로퍼티 (get, set)
    public TcpClient AsyncToClient
    {
        get { return asyncToClient; }
        set { asyncToClient = value; }
    }

    // ToString 메서드로 디버깅 및 로그용 정보 제공
    //public override string ToString()
    //{
    //    return $"IP: {RemoteEndPoint.Address}, Port: {RemoteEndPoint.Port}";
    //}
}
