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
    //private const int Port = 9090; // 서버 포트
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

        (int tcpPort, int udpPort) = FindAvailablePorts();



        #region tcp연결구현
        Console.WriteLine("TCP 연결 시작...");
        var tcpTask = Task.Run(() => tcpConnection.StartConnection(ServerIp, ServerPort, tcpPort, udpPort));
        #endregion tcp연결구현 끝



        #region udp연결구현
        // CancellationTokenSource 생성
        var cancellationTokenSource = new CancellationTokenSource();
        CancellationToken token = cancellationTokenSource.Token;
        

        // 서버 작업 비동기로 실행
        var serverTask = Task.Run(() => udpConnection.StartConnection(ServerIp, ServerPort, udpPort));
        Console.WriteLine("var serverTask = udpConnection.RunServerAsync(udpServer, token);");
        #endregion udp연결구현 끝

        // 모든 비동기 작업을 병렬로 실행
        Console.WriteLine("서버 작업이 동시에 실행됩니다.");
        await Task.WhenAll(tcpTask, serverTask); // 모든 비동기 작업 완료 대기

        Console.WriteLine("서버가 종료되었습니다.");
    }

    public static (int TcpPort, int UdpPort) FindAvailablePorts()
    {
        int tcpPort = FindAvailablePort();  // TCP 포트 찾기
        int udpPort = FindAvailablePort();  // UDP 포트 찾기 (다른 포트 번호를 반환)

        return (tcpPort, udpPort);
    }

    private static int FindAvailablePort()
    {
        // 1024부터 65535까지의 포트 번호 중 사용 가능한 포트를 찾습니다.
        for (int port = 1024; port <= 65535; port++)
        {
            if (IsPortAvailable(port))
            {
                return port;  // 사용 가능한 포트 번호 반환
            }
        }
        throw new Exception("No available port found.");
    }

    private static bool IsPortAvailable(int port)
    {
        var tcpListener = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Any, port);
        try
        {
            // 포트가 사용 가능한지 확인하기 위해 시작하고 바로 멈추기
            tcpListener.Start();
            return true;  // 포트가 사용 가능함
        }
        catch
        {
            // 예외가 발생하면 포트가 이미 사용 중이므로 사용할 수 없음
            return false;
        }
        finally
        {
            // tcpListener가 IDisposable을 구현하므로 명시적으로 Stop 호출
            tcpListener.Stop();
        }
    }


}
