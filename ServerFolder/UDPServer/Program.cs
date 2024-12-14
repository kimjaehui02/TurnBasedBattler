using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
//using System.Net.Sockets;

// 또하나의 송수신 철칙

// 1. RunServerAsync
// 2. Receive
// 3. Handle

// 수신은 3단계로 나누어지고
// 1단계는 2단계를 반복만
// 2단계는 3단계로 실행만
// 3단계는 데이터를 받아서 경우에따라 함수로 분배

class UdpServer
{
    private const string ServerIp = "127.0.0.1";  // 서버 IP
    private const int ServerPort = 8080;  // 서버 포트


    #region 통신용 변수들
    private const int Port = 9090; // 서버 포트
    #endregion

    #region 유저정보
    
    #endregion

    #region 게임정보
    // 게임 관련 정보가 필요한 경우 여기에 추가
    #endregion

    static async Task Main(string[] args)
    {
        Console.WriteLine("UDP 서버 시작...");
        PlayerManager playerManager = new PlayerManager();
        UDPServer.TcpConnection tcpConnection = new UDPServer.TcpConnection();
        UDPServer.UdpConnection udpConnection = new UDPServer.UdpConnection(playerManager);



        UdpClient udpServer = new UdpClient(Port); // 서버 포트 지정



        //IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0); // 클라이언트로부터 오는 모든 데이터를 수신

        // UdpServer 인스턴스 생성
        // UdpServer serverInstance = new UdpServer();

        #region udp연결구현
        // CancellationTokenSource 생성
        var cancellationTokenSource = new CancellationTokenSource();
        CancellationToken token = cancellationTokenSource.Token;

        // 서버 작업 비동기로 실행
        var serverTask = udpConnection.RunServerAsync(udpServer, token);

        #endregion

        // 서버가 종료될 때까지 대기
        Console.WriteLine("서버를 종료하려면 'Enter'를 누르세요.");
        Console.ReadLine();
        cancellationTokenSource.Cancel();

        // 서버 작업이 종료될 때까지 기다림
        await serverTask;
        Console.WriteLine("서버가 종료되었습니다.");
    }


}
