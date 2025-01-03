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
using UnityEngine.Windows;

public class UdpClientManager : MonoBehaviour
{
    [SerializeField]
    TransformManager transformManager;

    #region 통신용 변수들
    //private const string ServerIp = "127.0.0.1"; // 서버 IP
    //private const int ServerPort = 9090; // 서버 포트
    private UdpClient udpClient;
    //public int myId = -1;  // 내 id, 서버에서 받아온 값으로 설정
    CancellationTokenSource cancellationTokenSource;

    private const float sendInterval = 0.02f; // 좌표 전송 간격
    private float lastSendTime = 0f;

    [SerializeField]
    private bool isConnected = false;  // 연결 상태 추적 변수 추가
    #endregion

    #region 게임플레이용 변수들
    //public GameObject playerPrefab;  // 플레이어 오브젝트의 프리팹을 에디터에서 드래그 앤 드롭으로 설정
    //private Dictionary<int, GameObject> playerObjects = new Dictionary<int, GameObject>();
    //public GameObject myPlayerObject; // 내 플레이어 오브젝트
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

            byte[] data = CompressionManager.CompressJson(jsonMessage);
            //byte[] data = Encoding.UTF8.GetBytes(jsonMessage);
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
            
            //StartCoroutine(ReceiveFromUDPServerCoroutine(udpServer, token));  // 코루틴으로 비동기 처리

        }
        catch (Exception ex)
        {
            Debug.Log($"서버 오류 발생: {ex.Message}");
        }
        Debug.Log($"4. RunServerCoroutine종료지점 : ");
    }


    public IEnumerator ReceiveFromUDPServerCoroutine(UdpClient udpServer, CancellationToken token)
    {
        Debug.Log("Start receiving data from UDP server...");

        while (!token.IsCancellationRequested)
        {

            // 비동기적으로 UDP 데이터 받기
            var resultTask = udpServer.ReceiveAsync();

            // 결과가 완료될 때까지 대기
            yield return new WaitUntil(() => resultTask.IsCompleted);

            // 비동기 작업이 완료되면 데이터 추출
            UdpReceiveResult result = resultTask.Result;

            if (result.Buffer.Length > 0)
            {
                byte[] data = result.Buffer;  // 받은 데이터
                IPEndPoint remoteEP = result.RemoteEndPoint; // 송신자 정보

                //string json = Encoding.UTF8.GetString(data); // UTF-8로 문자열 변환

                var json = CompressionManager.DecompressJson(data);
                Debug.Log($"Received jsondata: {json}");


                // JSON 파싱
                var message = JsonConvert.DeserializeObject<dynamic>(json);
                if (message != null)
                {
                    // 메시지 처리
                    HandleConnectionState(message);
                }
                else
                {
                    Debug.Log("Failed to parse JSON.");
                }
                //try
                //{

                //}
                //catch (Exception ex)
                //{
                //    Debug.Log($"Error parsing JSON: {ex.Message}");
                    
                //}
            }
            else
            {
                Debug.Log("Received empty buffer.");
            }


            // 일정 시간 간격 후 반복
            yield return null;  // 계속해서 데이터를 대기
        }

        Debug.Log("Stopped receiving data.");
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
        //GameManager.Instance.SetPlayerId(message.data.playerId);
        
        Debug.Log($"Assigned myId: {GameManager.Instance.GetPlayerId()}");
    }

    private void HandleDataSyncing(dynamic message)
    {
        Debug.Log("Handling Data Syncing state...");

        // message가 무엇인지 출력하고 싶다면
        Debug.Log($"Received allmessage: {message}");

        Debug.Log($"Raw message data: {message.data.GetType()}");
        Debug.Log($"Received ObjectTransformmessage: {message.data}");
        string jsonData = (message.data as JValue)?.ToString();

        Dictionary<string, Dictionary<string, ObjectTransform>> pairs = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, ObjectTransform>>>(jsonData);
        Debug.Log($"Successfully deserialized data: {pairs}");

        //Dictionary<string, Dictionary<string, ObjectTransform>> pairs = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, ObjectTransform>>>(message.data);


        transformManager.OnObjectTransformsReceived2(pairs);
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
    public IEnumerator StartConnectionCoroutine(string inputIp, int inputPort)
    {
        // 서버와 연결 상태가 아닌 경우에만 새로 연결
        if (udpClient == null || udpClient.Client == null || udpClient.Client.Connected == false)
        {
            if (udpClient != null)
            {
                Quit();
            }

            udpClient = new UdpClient(inputIp, inputPort);
            isConnected = true;
            SendToUDPServer(ConnectionState.Connecting, new { playerName = "Player1" });

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken token = cancellationTokenSource.Token;

            // 지속적인 통신 시작
            //yield return StartCoroutine(RunServerCoroutine(udpClient, cancellationTokenSource.Token));
            yield return StartCoroutine(ReceiveFromUDPServerCoroutine(udpClient, token));

            Debug.Log("서버에 연결됨.");
        }
        else
        {
            Debug.Log("서버에 이미 연결되어 있음.");
        }
    }

    public void Updatecyle()
    {
        if(isConnected == false)
        {
            Debug.Log("(isConnected == false)");
            Debug.Log("(isConnected == false)");
            Debug.Log("(isConnected == false)");
            Debug.Log("(isConnected == false)");

            return;
        }

        if (GameManager.Instance.GetPlayerId() == -1)
        {
            Debug.Log("(GameManager.Instance.GetPlayerId() == -1)");
            Debug.Log("(GameManager.Instance.GetPlayerId() == -1)");
            Debug.Log("(GameManager.Instance.GetPlayerId() == -1)");
            Debug.Log("(GameManager.Instance.GetPlayerId() == -1)");

            return;
        }

        float currentTime = Time.realtimeSinceStartup;
        if (currentTime - lastSendTime >= sendInterval)
        {
            Dictionary<int, ObjectTransform> ObjectTransforms = TransformConverter.ConvertGameObjectsToTransforms(GameManager.Instance.gameObjects);

            var message = new
            {
                PlayerId = GameManager.Instance.GetPlayerId(),
                data = JsonConvert.SerializeObject(ObjectTransforms, Formatting.Indented)
            };


            SendToUDPServer(ConnectionState.DataSyncing, message);

            lastSendTime = currentTime;
        }
    }

    public void ConnectServer(string inputIp, int inputPort)
    {
        cancellationTokenSource = new CancellationTokenSource();
        CancellationToken token = cancellationTokenSource.Token;
        StartCoroutine(StartConnectionCoroutine(inputIp, inputPort));  // 코루틴을 호출합니다.
    }


    public void Quit()
    {
        try
        {
            SendToUDPServer(ConnectionState.Disconnecting, new { playerId = GameManager.Instance.GetPlayerId() });
            if (udpClient != null)
            {
                isConnected = false;
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

    void Update()
    {
        Updatecyle();
    }

    public void OnApplicationQuit()
    {
        Quit();
    }
}
