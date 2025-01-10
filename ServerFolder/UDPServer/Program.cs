using System;
using System.Net;
using System.Net.Sockets;

using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UDPServer;
using UDPServer.Manager;

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
/*        JsonCompressionManager testing = new JsonCompressionManager();

        testing.test();*/
        
        
        Console.WriteLine("UDP 서버 시작...");
        PlayerManager playerManager = new PlayerManager();
        ObjectTransformManager objectTransformManager = new ObjectTransformManager();
        //UDPServer.TcpConnection tcpConnection = new UDPServer.TcpConnection(playerManager, objectTransformManager);
        TcpConnectionManager tcpConnectionManager = new TcpConnectionManager(playerManager, objectTransformManager);
        UDPServer.UdpConnection udpConnection = new UDPServer.UdpConnection(playerManager, objectTransformManager);

        (int tcpPort, int udpPort) = FindAvailablePorts();
        Console.WriteLine($"tcpPort : {tcpPort}, udpPort : {udpPort}");



        #region tcp연결구현
        Console.WriteLine("TCP 연결 시작...");
        //var tcpTask = Task.Run(() => tcpConnection.StartConnection(ServerIp, ServerPort, tcpPort, udpPort));
        var tcpTask = Task.Run(() => tcpConnectionManager.StartConnection(ServerIp, ServerPort, tcpPort, udpPort));
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
        // UDP 포트를 먼저 찾고, 그 다음 TCP 포트를 찾습니다.
        int udpPort = FindAvailablePort(true);  // UDP 포트 찾기
        int tcpPort = FindAvailablePort(false); // TCP 포트 찾기 (UDP와 충돌하지 않는 포트 번호 반환)

        //return (tcpPort, udpPort);
        return (udpPort, udpPort);
    }

    private static int FindAvailablePort(bool isUdp)
    {
        // 1024부터 65535까지의 포트 번호 중 사용 가능한 포트를 찾습니다.
        for (int port = 1024; port <= 65535; port++)
        {
            if (IsPortAvailable(port, isUdp))
            {
                Console.WriteLine($"사용 가능한 포트 발견: {port} ({(isUdp ? "UDP" : "TCP")})");
                return port;  // 사용 가능한 포트 번호 반환
            }
        }
        throw new Exception("사용 가능한 포트를 찾을 수 없습니다.");
    }

    private static bool IsPortAvailable(int port, bool isUdp)
    {
        try
        {
            if (isUdp)
            {
                // UDP 포트가 사용 가능한지 확인
                using (UdpClient udpClient = new UdpClient())
                {
                    udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, port));  // UDP 포트에 바인딩 시도
                    udpClient.Close();  // 바인딩이 성공하면 바로 닫음
                }
            }
            else
            {
                // TCP 포트가 사용 가능한지 확인
                using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                {
                    socket.Bind(new IPEndPoint(IPAddress.Any, port));  // TCP 포트에 바인딩 시도
                    socket.Close();  // 바인딩이 성공하면 바로 소켓을 닫음
                }
            }
            return true;  // 포트가 사용 가능
        }
        catch (SocketException ex)
        {
            // 포트를 바인딩할 수 없으면 이미 사용 중인 포트임
            Console.WriteLine($"포트 {port} 바인딩 실패: {ex.Message}");
            return false;
        }
    }





}
