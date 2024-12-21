using UnityEngine;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System;
using Newtonsoft.Json;
using System.Collections;
using UnityEngine.UIElements;
using System.Threading;
using System.Threading.Tasks;
using Unity.VisualScripting.Antlr3.Runtime;
using Newtonsoft.Json.Linq;

public class OldUdp : MonoBehaviour
{
    #region 통신용 변수들
    private const string ServerIp = "127.0.0.1"; // 서버 IP
    private const int ServerPort = 9090; // 서버 포트
    private UdpClient udpClient;
    public int myId = -1;  // 내 id, 서버에서 받아온 값으로 설정
    CancellationTokenSource cancellationTokenSource;

    private float sendInterval = 5f; // 좌표 전송 간격
    private float lastSendTime = 0f;
    #endregion

    #region 게임플레이용 변수들
    public GameObject playerPrefab;  // 플레이어 오브젝트의 프리팹을 에디터에서 드래그 앤 드롭으로 설정
    private Dictionary<int, GameObject> playerObjects = new Dictionary<int, GameObject>();
    public GameObject myPlayerObject; // 내 플레이어 오브젝트

    #endregion

    #region json선언부
    public enum ConnectionState
    {
        Default,      // 기본 상태
        Connecting,   // 연결 시도 중
        DataSyncing,  // 데이터 동기화 중
        Disconnecting,// 연결 종료 시도 중
        Error,        // 오류 발생
        TcpToUdp      // tcp에서 udp로 이동시킵니다
    }
    #endregion

    #region 송신함수
    public void SendToUDPServer(ConnectionState connectionState, object messageData)
    {

        Debug.Log($"SendToUDPServer실행 connectionState : {connectionState}, messageData : {messageData} ");

        try
        {
            // 메시지 구성
            var message = new
            {
                connectionState = connectionState.ToString(), // Enum 값을 문자열로 변환
                data = messageData
            };

            // 직렬화 시 무한 참조 방지 설정
            var settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };

            // 객체를 JSON 문자열로 직렬화
            string jsonMessage = JsonConvert.SerializeObject(message, Formatting.Indented, settings);

            // 디버깅용으로 JSON 출력
            Debug.Log($"보내는 JSON: {jsonMessage}");

            // UDP를 통해 서버로 메시지 전송
            byte[] data = Encoding.UTF8.GetBytes(jsonMessage);
            udpClient.Send(data, data.Length);
        }
        catch (Exception ex)
        {
            Debug.Log($"UDP 전송 중 오류 발생: {ex.Message}");
        }
    }
    #endregion

    #region 수신함수

    // 서버 실행 메서드
    public async Task RunServerAsync(UdpClient udpServer, CancellationToken token)
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
                await ReceiveFromUDPServer(udpServer);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"서버 오류 발생: {ex.Message}");
        }
        Console.WriteLine($"4. RunServerAsync종료지점 : ");
    }

    public async Task ReceiveFromUDPServer(UdpClient udpServer)
    {

        // 데이터 수신을 위한 끝점 정의
        //IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, ServerPort);
        Debug.Log($"public async Task ReceiveFromUDPServer(UdpClient udpServer)");
        try
        {
            Debug.Log($"try");
            // 데이터 수신
            UdpReceiveResult result = await udpServer.ReceiveAsync();
            Debug.Log($"UdpReceiveResult result = await udpServer.ReceiveAsync();");
            byte[] data = result.Buffer;  // UdpReceiveResult에서 Buffer 속성으로 데이터 추출
            Debug.Log($"byte[] data = result.Buffer;  // UdpReceiveResult에서 Buffer 속성으로 데이터 추출");
            IPEndPoint remoteEP = result.RemoteEndPoint;  // 클라이언트의 IP와 포트 번호를 가진 IPEndPoint
            Debug.Log($"IPEndPoint remoteEP = result.RemoteEndPoint;  // 클라이언트의 IP와 포트 번호를 가진 IPEndPoint");

            string json = Encoding.UTF8.GetString(data);

            // 수신한 JSON 데이터를 콘솔에 출력
            Debug.Log("Received data: " + json);

            // JSON 문자열을 Newtonsoft.Json으로 처리 (dynamic 사용)
            try
            {
                // JSON 파싱 (Newtonsoft.Json 사용)
                var message = JsonConvert.DeserializeObject<dynamic>(json);
                Debug.Log($"ReceiveFromUDPServer종료 message : {message}");
                if (message != null)
                {
                    // 'connectionState'에 따라 처리
                    HandleConnectionState(message);
                }
                else
                {
                    Debug.Log("Failed to parse JSON.");
                }
            }
            catch (Exception ex)
            {
                // JSON 파싱 예외 처리
                Debug.Log($"Error parsing JSON: {ex.Message}");
            }
        }
        catch (Exception ex)
        {
            // 예외 처리: 데이터 수신 중 오류 발생 시
            Debug.Log($"Error receiving data: {ex.Message}");
        }


        // 계속해서 데이터를 수신할 수 있도록 대기
        //udpClient.BeginReceive(ReceiveFromUDPServer, null);
    }

    private void HandleConnectionState(dynamic message)
    {
        string connectionState = message.connectionState;

        switch (connectionState)
        {
            case "Connecting":
                HandleConnecting(message);
                break;

            case "DataSyncing":
                HandleDataSyncing(message);
                break;

            case "Disconnecting":
                HandleDisconnecting(message);
                break;

            case "Error":
                HandleError(message);
                break;

            default:
                Debug.Log($"Unknown connection state: {connectionState}");
                break;
        }
    }


    private void HandleConnecting(dynamic message)
    {
        Debug.Log("Handling Connecting state...");
        myId = message.data.playerId; // message에서 playerId를 추출하여 myId에 할당
        Debug.Log($"Assigned myId: {myId}");
    }

    private void HandleDataSyncing(dynamic message)
    {
        Debug.Log("Handling Data Syncing state...");
    }

    private void HandleDisconnecting(dynamic message)
    {
        Debug.Log("Handling Disconnecting state...");
    }

    private void HandleError(dynamic message)
    {
        Debug.Log("Handling Error state...");
    }
    #endregion

    #region 통신 시작, 지속적 송수신, 종료 알림
    public async Task StartConnection()
    {
        // 서버와 연결 상태가 아닌 경우에만 새로 연결
        if (udpClient == null || udpClient.Client == null || udpClient.Client.Connected == false)
        {
            // 기존 클라이언트가 존재하면 종료하고 새로 연결
            if (udpClient != null)
            {
                Quit();
            }

            udpClient = new UdpClient(ServerIp, ServerPort);

            // 서버에 연결 요청 보내기
            SendToUDPServer(ConnectionState.Connecting, new { playerName = "Player1" });
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken token = cancellationTokenSource.Token;

            // 지속적인 통신 시작
            await RunServerAsync(udpClient, cancellationTokenSource.Token);


            Debug.Log("서버에 연결됨.");
        }
        else
        {
            // 이미 연결된 상태라면 연결 재설정 없이 유지
            Debug.Log("서버에 이미 연결되어 있음.");
        }
    }

    public void Updatecyle()
    {
        // 실제 경과 시간을 기준으로 비교
        float currentTime = Time.realtimeSinceStartup;

        // 일정 간격마다 데이터 전송
        if (currentTime - lastSendTime >= sendInterval)
        {
            // 데이터 전송 함수 호출
            SendToUDPServer(ConnectionState.DataSyncing, new
            {
                positionX = myPlayerObject.transform.position.x,
                positionY = myPlayerObject.transform.position.y,
            });

            // 마지막 전송 시간 갱신
            lastSendTime = currentTime;
        }
    }

    // 1. 서버 참여 요청
    void Start()
    {
        Application.runInBackground = true;

        cancellationTokenSource = new CancellationTokenSource(); ;
        CancellationToken token = cancellationTokenSource.Token;
        Task.Run(async () => await StartConnection());

    }

    //// 2. 초당 고정 횟수로 데이터를 주고받기
    //private float lastSendTime = 0f;  // 마지막 전송 시간
    //private float sendInterval = 0.05f; // 전송 간격 (초)

    void Update()
    {
        Updatecyle();
    }

    // 3. 종료 통신 (수동 종료 또는 강제 종료 시)
    public void OnApplicationQuit()
    {
        Quit();
    }

    public void Quit()
    {
        try
        {
            // 종료 알리기
            SendToUDPServer(ConnectionState.Disconnecting, new { playerId = myId });

            // UDP 클라이언트 종료
            if (udpClient != null)
            {
                udpClient.Close();
                cancellationTokenSource.Cancel();
                Debug.Log("Disconnected and closed UDP client.");
            }
        }
        catch (Exception ex)
        {
            Debug.Log("Error during quit: " + ex.Message);
        }
    }

    #endregion


    #region 코루틴
    private IEnumerator RunServerAsyncCoroutine()
    {

        yield return null; // 계속해서 기다림
    }
    #endregion
}
