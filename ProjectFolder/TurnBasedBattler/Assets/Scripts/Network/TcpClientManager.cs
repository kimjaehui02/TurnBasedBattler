using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

using UnityEngine;
using UnityEngine.Windows;

public class TcpClientManager : MonoBehaviour
{

    #region 변수들

    private TcpClient _client;
    private NetworkStream _stream;
    private bool _isConnected;

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

    #region 기본통신
    // UDP 클라이언트 초기화
    public void ConnectServer(string ServerIp, int ServerPort)
    {

        // TCP 클라이언트 연결
        _client = new TcpClient(ServerIp, ServerPort);
        _stream = _client.GetStream();
        

        // 서버와 연결 후 초기화 작업
        SendToTcpServer(ConnectionState.Connecting, new { playerName = "client" });

        // TCP 데이터 수신 시작
        StartCoroutine(ReceiveFromTCPServerCoroutine());

    }

    // UDP 데이터 송신
    public void SendToTcpServer(ConnectionState connectionState, object messageData)
    {
        // 데이터 송신 코드
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

    public IEnumerator ReceiveFromTCPServerCoroutine()
    {
        byte[] buffer = new byte[1024];

        while (_client.Connected)  // 클라이언트가 연결되어 있을 때만 데이터 수신
        {
            if (!_stream.DataAvailable)
            {
                yield return null;  // 데이터 준비 상태에서만 작업을 진행하도록 함
                continue;
            }

            int bytesRead = _stream.Read(buffer, 0, buffer.Length);
            Debug.Log($"bytesRead: {bytesRead}");

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

            // 데이터를 처리하고 한 프레임 대기
            yield return null;
        }
    }

    // 클라이언트 종료
    public void Quit()
    {
        try
        {
            SendToTcpServer(ConnectionState.Disconnecting, new { playerId = GameManager.Instance.GetPlayerId() });
            if (_isConnected)
            {
                _isConnected = false;
                _stream?.Close();
                _client?.Close();

                Debug.Log("서버와의 연결을 종료했습니다.");
            }
            //starttoken?.Cancel();
        }
        catch (Exception ex)
        {
            Debug.LogError($"연결 종료 중 오류 발생: {ex.Message}");
        }
    }
    #endregion




    #region 핸들러

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

        try
        {
            // JSON을 JObject로 변환
            JObject jsonObject = JObject.Parse(message.ToString());

            // "data" 항목 확인
            if (jsonObject["data"] != null)
            {
                // playerId 항목 확인
                if (jsonObject["data"]["playerId"] != null)
                {
                    // playerId 값을 처리
                    int playerId = jsonObject["data"]["playerId"].ToObject<int>();
                    GameManager.Instance.SetPlayerId(playerId);
                    Debug.Log($"playerId found: {playerId}");
                }
                else
                {
                    Debug.Log("playerId not found in data.");
                }

                // "subServerList" 항목 확인
                if (jsonObject["data"]["subServerList"] != null && jsonObject["data"]["subServerList"].HasValues)
                {
                    // subServerList 값이 비어 있지 않으면 처리
                    List<ServerInfo> subServerList = jsonObject["data"]["subServerList"].ToObject<List<ServerInfo>>();

                    // 첫 번째 서버 정보 접근
                    ServerInfo firstServer = subServerList[0];
                    string ip = firstServer.ipAddress;
                    int tcp = firstServer.tcpPort;
                    int udp = firstServer.udpPort;
                    GameManager.Instance.ConnectSubServer(ip, tcp, udp);

                    Debug.Log($"subServerList found: {subServerList}");
                }
                else
                {
                    Debug.Log("subServerList not found or is empty in data.");
                }


            }
            else
            {
                Debug.Log("data not found in the message.");
            }
        }
        catch (System.Exception ex)
        {
            Debug.Log($"Error while parsing JSON: {ex.Message}");
        }

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


}
