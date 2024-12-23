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

public class UdpClientManager : MonoBehaviour
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
            var message = new
            {
                connectionState = connectionState.ToString(), // Enum 값을 문자열로 변환
                data = messageData
            };

            var settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };

            string jsonMessage = JsonConvert.SerializeObject(message, Formatting.Indented, settings);
            Debug.Log($"보내는 JSON: {jsonMessage}");
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
    public void RunServerCoroutine(UdpClient udpServer, CancellationToken token)
    {
        Debug.Log($"1. RunServerCoroutine시작지점 : ");
        try
        {
            Debug.Log($"2. try : ");
            StartCoroutine(ReceiveFromUDPServerCoroutine(udpServer, token));  // 코루틴으로 비동기 처리
        }
        catch (Exception ex)
        {
            Debug.Log($"서버 오류 발생: {ex.Message}");
        }
        Debug.Log($"4. RunServerCoroutine종료지점 : ");
    }



    public IEnumerator ReceiveFromUDPServerCoroutine(UdpClient udpServer, CancellationToken token)
    {
        Debug.Log($"public IEnumerator ReceiveFromUDPServerCoroutine(UdpClient udpServer, CancellationToken token)");
        while (!token.IsCancellationRequested)
        {
            // 데이터 수신 대기
            UdpReceiveResult? result = null; // Nullable로 변경

            // 데이터 수신 대기
            yield return new WaitUntil(() =>
            {
                try
                {
                    // ReceiveAsync를 비동기적으로 호출하고 결과를 받음
                    result = udpServer.ReceiveAsync().Result;
                    return result.HasValue; // result가 null이 아니면 true 반환
                }
                catch (Exception ex)
                {
                    Debug.Log($"Error receiving data: {ex.Message}");
                    return false;
                }
            });

            if (result.HasValue)
            {
                byte[] data = result.Value.Buffer;  // result.Value를 사용하여 데이터 추출
                IPEndPoint remoteEP = result.Value.RemoteEndPoint;

                string json = Encoding.UTF8.GetString(data);
                Debug.Log("Received data: " + json);

                try
                {
                    var message = JsonConvert.DeserializeObject<dynamic>(json);
                    Debug.Log($"ReceiveFromUDPServer종료 message : {message}");
                    if (message != null)
                    {
                        HandleConnectionState(message);
                    }
                    else
                    {
                        Debug.Log("Failed to parse JSON.");
                    }
                }
                catch (Exception ex)
                {
                    Debug.Log($"Error parsing JSON: {ex.Message}");
                }
            }

            yield return null;  // 계속해서 대기
        }
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
        myId = message.data.playerId;
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
    public void StartConnectionCoroutine()
    {
        // 서버와 연결 상태가 아닌 경우에만 새로 연결
        if (udpClient == null || udpClient.Client == null || udpClient.Client.Connected == false)
        {
            if (udpClient != null)
            {
                Quit();
            }

            udpClient = new UdpClient(ServerIp, ServerPort);

            SendToUDPServer(ConnectionState.Connecting, new { playerName = "Player1" });

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken token = cancellationTokenSource.Token;

            // 지속적인 통신 시작
            RunServerCoroutine(udpClient, cancellationTokenSource.Token);
            Debug.Log("서버에 연결됨.");
        }
        else
        {
            Debug.Log("서버에 이미 연결되어 있음.");
        }
    }

    public void Updatecyle()
    {
        float currentTime = Time.realtimeSinceStartup;
        if (currentTime - lastSendTime >= sendInterval)
        {
            SendToUDPServer(ConnectionState.DataSyncing, new
            {
                positionX = myPlayerObject.transform.position.x,
                positionY = myPlayerObject.transform.position.y,
            });

            lastSendTime = currentTime;
        }
    }

    void Start()
    {
        Application.runInBackground = true;

        cancellationTokenSource = new CancellationTokenSource();
        CancellationToken token = cancellationTokenSource.Token;
        StartConnectionCoroutine();  // 코루틴을 호출합니다.
    }

    void Update()
    {
        Updatecyle();
    }

    public void OnApplicationQuit()
    {
        Quit();
    }

    public void Quit()
    {
        try
        {
            SendToUDPServer(ConnectionState.Disconnecting, new { playerId = myId });
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
}
