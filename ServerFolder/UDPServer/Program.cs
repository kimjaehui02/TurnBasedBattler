using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

class UdpServer
{
    #region 통신용 변수들
    private const int Port = 7777; // 서버 포트
    #endregion

    #region 유저정보
    private PlayerManager playerManager = new PlayerManager();
    #endregion

    #region 게임정보
    // 게임 관련 정보가 필요한 경우 여기에 추가
    #endregion

    static async Task Main(string[] args)
    {
        Console.WriteLine("UDP 서버 시작...");
        UdpClient udpServer = new UdpClient(Port); // 서버 포트 지정
        //IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0); // 클라이언트로부터 오는 모든 데이터를 수신

        // UdpServer 인스턴스 생성
        UdpServer serverInstance = new UdpServer();

        // CancellationTokenSource 생성
        var cancellationTokenSource = new CancellationTokenSource();
        CancellationToken token = cancellationTokenSource.Token;

        // 서버 작업 비동기로 실행
        var serverTask = RunServerAsync(udpServer, token, serverInstance);

        // 서버가 종료될 때까지 대기
        Console.WriteLine("서버를 종료하려면 'Enter'를 누르세요.");
        Console.ReadLine();
        cancellationTokenSource.Cancel();

        // 서버 작업이 종료될 때까지 기다림
        await serverTask;
        Console.WriteLine("서버가 종료되었습니다.");
    }

    #region json선언부
    public enum ConnectionState
    {
        Default,      // 기본 상태
        Connecting,   // 연결 시도 중
        DataSyncing,  // 데이터 동기화 중
        Disconnecting,// 연결 종료 시도 중
        Error         // 오류 발생
    }
    #endregion

    #region 송신부
    // 서버에서 클라이언트로 데이터를 송신하는 메서드
    public void SendToUDPClient(UdpClient udpClient, IPEndPoint remoteEP, ConnectionState connectionState, object messageData)
    {
        Console.WriteLine($"SendToUDPClient의 보내는 remoteEP: {remoteEP}");

        try
        {
            // 메시지 구성
            var message = new
            {
                connectionState = connectionState.ToString(),
                data = messageData
            };

            // 객체를 JSON 문자열로 직렬화
            string jsonMessage = JsonConvert.SerializeObject(message, Formatting.Indented);

            // 디버깅용으로 JSON 출력
            Console.WriteLine($"보내는 JSON: {jsonMessage}");

            // UDP를 통해 클라이언트로 메시지 전송
            byte[] data = Encoding.UTF8.GetBytes(jsonMessage);
            udpClient.Send(data, data.Length, remoteEP);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"UDP 전송 중 오류 발생: {ex.Message}");
        }
    }
    #endregion

    #region 수신부
    // 클라이언트로부터 데이터를 수신하는 메서드
    public async Task ReceiveFromUDPClient(UdpClient udpServer)
    {
        Console.WriteLine($"ReceiveFromUDPClient시작점 : ");

        try
        {
            // 데이터 수신
            UdpReceiveResult result = await udpServer.ReceiveAsync();
            byte[] data = result.Buffer;  // UdpReceiveResult에서 Buffer 속성으로 데이터 추출
            IPEndPoint remoteEP = result.RemoteEndPoint;  // 클라이언트의 IP와 포트 번호를 가진 IPEndPoint

            string json = Encoding.UTF8.GetString(data);

            // 수신한 JSON 데이터를 콘솔에 출력
            Console.WriteLine("Received data: " + json);

            // JSON 문자열을 Newtonsoft.Json으로 처리
            try
            {
                var message = JsonConvert.DeserializeObject<dynamic>(json);
                Console.WriteLine($"ReceiveFromUDPClient트라이성공 message : {message}");

                if (message != null)
                {
                    // 'connectionState'에 따라 처리
                    HandleConnectionState(udpServer, remoteEP, message);
                }
                else
                {
                    Console.WriteLine("Failed to parse JSON.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing JSON: {ex.Message}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error receiving data: {ex.Message}");
        }
        Console.WriteLine($"ReceiveFromUDPClient종료지점 : ");
    }
    #endregion

    #region 데이터 처리
    // 클라이언트에서 보낸 메시지의 connectionState에 따라 처리
    private void HandleConnectionState(UdpClient udpClient, IPEndPoint remoteEP, dynamic message)
    {
        string connectionState = message.connectionState;

        Console.WriteLine($"HandleConnectionState : {connectionState}");
        Console.WriteLine($"HandleConnectionState : {connectionState}");
        Console.WriteLine($"HandleConnectionState : {connectionState}");

        Console.WriteLine($"HandleConnectionState의 보내는 remoteEP: {remoteEP}");
        switch (connectionState)
        {
            case "Connecting":
                HandleConnecting(udpClient, remoteEP, message);
                break;

            case "DataSyncing":
                HandleDataSyncing(udpClient, remoteEP, message);
                break;

            case "Disconnecting":
                HandleDisconnecting(udpClient, remoteEP, message);
                break;

            case "Error":
                HandleError(udpClient, remoteEP, message);
                break;

            default:
                Console.WriteLine($"Unknown connection state: {connectionState}");
                break;
        }
    }

    private void HandleConnecting(UdpClient udpClient, IPEndPoint remoteEP, dynamic message)
    {
        Console.WriteLine("Handling Connecting state...");
        // 여기에서 플레이어 연결 처리 로직 추가
        int id = playerManager.AddPlayer(remoteEP);
        Console.WriteLine($"HandleConnecting의 보내는 remoteEP: {remoteEP}");
        SendToUDPClient(udpClient, remoteEP, ConnectionState.Connecting, new { playerId = id });
    }

    private void HandleDataSyncing(UdpClient udpClient, IPEndPoint remoteEP, dynamic message)
    {
        Console.WriteLine("Handling Data Syncing state...");
        // 데이터 동기화 처리 로직 추가
    }

    private void HandleDisconnecting(UdpClient udpClient, IPEndPoint remoteEP, dynamic message)
    {
        Console.WriteLine("Handling Disconnecting state...");
        Console.WriteLine($"message  ...{message}");
        // 연결 종료 처리 로직 추가
        // message.data.playerId가 실제로 int로 변환되는지 확인
        int playerId = Convert.ToInt32(message.data.playerId);  // 강제 형변환

        // 연결 종료 처리 로직
        playerManager.DeletePlayer(playerId);
    }

    private void HandleError(UdpClient udpClient, IPEndPoint remoteEP, dynamic message)
    {
        Console.WriteLine("Handling Error state...");
        // 오류 처리 로직 추가
    }
    #endregion

    #region 서버 실행

    // 서버 실행 메서드
    static async Task RunServerAsync(UdpClient udpServer, CancellationToken token, UdpServer serverInstance)
    {
        Console.WriteLine($"1. RunServerAsync시작지점 : ");
        try
        {
            Console.WriteLine($"2. try : ");
            while (!token.IsCancellationRequested)
            {
                Console.WriteLine($"3. while (!token.IsCancellationRequested) : ");
                // 클라이언트로부터 데이터 수신
                //UdpReceiveResult receivedResult = await udpServer.ReceiveAsync();

                // 수신한 데이터 출력
                //string receivedMessage = Encoding.UTF8.GetString(receivedResult.Buffer);


                // 수신한 데이터를 세부적으로 처리
                await serverInstance.ReceiveFromUDPClient(udpServer);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"서버 오류 발생: {ex.Message}");
        }
        Console.WriteLine($"4. RunServerAsync종료지점 : ");
    }

    #endregion
}
