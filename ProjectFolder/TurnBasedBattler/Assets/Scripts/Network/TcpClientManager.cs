using UnityEngine;
using System.Net.Sockets;
using System.Text;
using System;
using System.Collections;
using Newtonsoft.Json;
using System.Threading;

public class TcpClientManager : MonoBehaviour
{
    #region 통신용 변수들
    private const string ServerIp = "127.0.0.1";  // 서버 IP
    private const int ServerPort = 9090;  // 서버 포트
    private TcpClient _client;
    private NetworkStream _stream;
    private bool _isConnected = false;

    CancellationTokenSource starttoken;
    #endregion

    #region 게임플레이용 변수들
    public GameObject myPlayerObject;  // 내 플레이어 오브젝트
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
    public void SendToTcpServer(ConnectionState connectionState, object messageData)
    {
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

            // TCP를 통해 서버로 메시지 전송
            byte[] data = Encoding.UTF8.GetBytes(jsonMessage);
            _stream.Write(data, 0, data.Length);
        }
        catch (Exception ex)
        {
            Debug.LogError($"TCP 전송 중 오류 발생: {ex.Message}");
        }
    }
    #endregion

    #region 수신함수
    public IEnumerator ReceiveFromTCPServerCoroutine()
    {
        byte[] buffer = new byte[1024];

        while (_client.Connected)  // 클라이언트가 연결되어 있을 때만 데이터 수신
        {
            if (starttoken.IsCancellationRequested)
            {
                Debug.Log("작업이 취소되었습니다.");
                yield break;
            }

            try
            {
                // 데이터 수신
                int bytesRead = _stream.Read(buffer, 0, buffer.Length);
                if (bytesRead > 0)
                {
                    byte[] data = new byte[bytesRead];
                    Array.Copy(buffer, 0, data, 0, bytesRead);

                    // 받은 데이터를 UTF-8 문자열로 변환
                    string json = Encoding.UTF8.GetString(data);

                    // JSON 처리
                    try
                    {
                        var message = JsonConvert.DeserializeObject<dynamic>(json);
                        if (message != null)
                        {
                            HandleConnectionState(message);  // 상태 처리
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
            }
            catch (Exception ex)
            {
                Debug.Log($"Error reading data: {ex.Message}");
                break; // 오류 발생 시 연결 종료
            }

            yield return null;
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

    #region 연결 및 종료 처리
    public IEnumerator StartConnectionCoroutine()
    {
        // TCP 클라이언트 연결
        _client = new TcpClient(ServerIp, ServerPort);
        _stream = _client.GetStream();
        _isConnected = true;
        Debug.Log("서버에 연결되었습니다.");

        // 서버와 연결 후 초기화 작업
        SendToTcpServer(ConnectionState.Connecting, new { playerName = "client" });

        // TCP 데이터 수신 시작
        yield return StartCoroutine(ReceiveFromTCPServerCoroutine());
        Debug.Log("TCP 데이터 수신 종료");
    }

    private void Start()
    {
        StartCoroutine(StartConnectionCoroutine());
    }

    // 연결된 서버와 통신을 계속할 수 있게 하는 메서드 (매 프레임마다 호출)
    void Update()
    {

    }

    // 서버와 연결을 종료하는 메서드
    public void Quit()
    {
        try
        {
            if (_isConnected)
            {
                _isConnected = false;
                _stream?.Close();
                _client?.Close();
                Debug.Log("서버와의 연결을 종료했습니다.");
            }
            starttoken?.Cancel();
        }
        catch (Exception ex)
        {
            Debug.LogError($"연결 종료 중 오류 발생: {ex.Message}");
        }
    }

    // 애플리케이션 종료 시 호출
    public void OnApplicationQuit()
    {
        Quit();
    }
    #endregion
}
