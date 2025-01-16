using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.Windows;
using System.Threading;
using System;
using System.Threading.Tasks;

public class UdpClientManager : MonoBehaviour
{

    #region 통신용 변수들
    [SerializeField]
    private TransformManager transformManager;

    private UdpClient udpClient;
    private UdpClient udpClientReceive;
    //private bool _isConnected;

    private const float sendInterval = 0.01f; // 좌표 전송 간격
    private float lastSendTime = 0f;

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


    public void OnApplicationQuit()
    {
        Quit();
    }

    void Update()
    {
        Updatecyle();
    }

    #region 기본통신

    // UDP 클라이언트 초기화
    public void ConnectServer(string ServerIp, int ServerPort)
    {
        if (udpClient != null)
        {
            Quit();
        }
        // 클라이언트 초기화 코드
        udpClient = new UdpClient(ServerIp, ServerPort);
        //_isConnected = true;
        SendToUDPServer(ConnectionState.Connecting, new { playerName = "Player1" });

        //CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        //CancellationToken tokens = cancellationTokenSource.Token;

        // 지속적인 통신 시작
        //yield return StartCoroutine(RunServerCoroutine(udpClient, cancellationTokenSource.Token));
        StartCoroutine(ReceiveFromUDPServerCoroutine());
        //yield return null;

        Debug.Log("서버에 연결됨.");
    }

    // UDP 데이터 송신
    public void SendToUDPServer(ConnectionState connectionState, object messageData)
    {
        // 데이터 송신 코드
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
            //Debug.Log($"보내는 JSON: {jsonMessage}");

            byte[] data = CompressionManager.CompressJson(jsonMessage);
            //byte[] data = Encoding.UTF8.GetBytes(jsonMessage);
            udpClient.Send(data, data.Length);
        }
        catch (Exception ex)
        {
            Debug.Log($"UDP 전송 중 오류 발생: {ex.Message}");
        }
    }

    

    // UDP 데이터 수신
    public IEnumerator ReceiveFromUDPServerCoroutine()
    {
        IPEndPoint remoteEP = null;
        byte[] data;

        // 데이터 수신 코드
        while (true)
        {

            if (udpClient.Available > 0)
            {

                try
                {
                    //udpClient.Client.ReceiveTimeout = 5;  // 1초 타임아웃 설정
                    data = udpClient.Receive(ref remoteEP);

                }
                catch
                {
                    Debug.Log("제하하하핳 이 에러는 뭐냐");
                    Debug.Log("제하하하핳 이 에러는 뭐냐");
                    Debug.Log("제하하하핳 이 에러는 뭐냐");
                    Debug.Log("제하하하핳 이 에러는 뭐냐");
                    continue;
                }

                //byte[] data = result.Buffer;  // 받은 데이터
                //IPEndPoint remoteEP = result.RemoteEndPoint; // 송신자 정보

                //string json = Encoding.UTF8.GetString(data); // UTF-8로 문자열 변환

                var json = CompressionManager.DecompressJson(data);
                Debug.Log($"Received jsondata: {json}");


                // JSON 파싱
                var message = JsonConvert.DeserializeObject<dynamic>(json);
                if (message != null)
                {
                    // 메시지 처리
                    HandleConnectionState(in message);
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
                //Debug.Log("Received empty buffer.");
            }


            // 일정 시간 간격 후 반복
            yield return null;  // 계속해서 데이터를 대기
        }
    }

    public void Quit()
    {
        try
        {
            SendToUDPServer(ConnectionState.Disconnecting, new { playerId = GameManager.Instance.GetPlayerId() });
            if (udpClient != null)
            {
                //_isConnected = false;
                udpClient.Close();
                udpClient = null;  // null로 초기화
                Debug.Log("D_isConnected and closed UDP client.");
            }
        }
        catch (Exception ex)
        {
            Debug.Log("Error during quit: " + ex.Message);
        }
    }


    public void Updatecyle()
    {
        //if (_isConnected == false)
        //{
        //    Debug.Log("(_isConnected == false)");
        //    //Debug.Log("(_isConnected == false)");
        //    //Debug.Log("(_isConnected == false)");
        //    //Debug.Log("(_isConnected == false)");

        //    return;
        //}

        if (GameManager.Instance.GetPlayerId() == -1)
        {
            Debug.Log("(GameManager.Instance.GetPlayerId() == -1)");
            //Debug.Log("(GameManager.Instance.GetPlayerId() == -1)");
            //Debug.Log("(GameManager.Instance.GetPlayerId() == -1)");
            //Debug.Log("(GameManager.Instance.GetPlayerId() == -1)");

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


    #endregion

    #region 핸들러


    private void HandleConnectionState(in dynamic message)
    {
        string connectionState = message.connectionState;

        switch (connectionState)
        {
            case "Connecting":
                HandleConnecting(in message);
                break;
            case "DataSyncing":
                HandleDataSyncing(in message);
                break;
            case "Disconnecting":
                HandleDisconnecting(in message);
                break;
            case "Error":
                HandleError(in message);
                break;
            default:
                Debug.Log($"Unknown connection state: {connectionState}");
                break;
        }
    }

    private void HandleConnecting(in dynamic message)
    {
        Debug.Log("Handling Connecting state...");
        //GameManager.Instance.SetPlayerId(message.data.playerId);

        Debug.Log($"Assigned myId: {GameManager.Instance.GetPlayerId()}");
    }

    private void HandleDataSyncing(in dynamic message)
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


        transformManager.OnObjectTransformsReceived2(in pairs);
    }

    private void HandleDisconnecting(in dynamic message)
    {
        Debug.Log("Handling Disconnecting state...");
    }

    private void HandleError(in dynamic message)
    {
        Debug.Log("Handling Error state...");
    }

    #endregion
}
