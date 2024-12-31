using System;
using System.Net;
using System.Net.Sockets;
using UDPServer;

class PlayerConnection
{
    // TcpClient 인스턴스를 private로 선언
    private TcpClient asyncToClient;

    // PlayerTransform 속성 추가
    public PlayerTransform Transform { get; set; }

    // 생성자
    public PlayerConnection(TcpClient asyncToClient)
    {
        this.asyncToClient = asyncToClient;

        // Transform 기본값 초기화
        Transform = new PlayerTransform(0, 0); // 초기 좌표 (0, 0)으로 설정
    }

    // asyncToClient에 대한 프로퍼티 (get, set)
    public TcpClient AsyncToClient
    {
        get { return asyncToClient; }
        set { asyncToClient = value; }
    }

    // ToString 메서드로 디버깅 및 로그용 정보 제공
    public override string ToString()
    {
        return $"Client Info: {asyncToClient.Client.RemoteEndPoint}, {Transform}";
    }
}
