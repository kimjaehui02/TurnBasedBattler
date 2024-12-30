using UnityEngine;
using System.Net.Sockets;
using System.Text;
using System;
using System.Collections;
using Newtonsoft.Json;
using System.Threading;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;


public class TcpClientManager : MonoBehaviour
{
    #region 통신용 변수들
    //private const string ServerIp = "127.0.0.1";  // 서버 IP
    //private const int ServerPort = 9090;  // 서버 포트
    private TcpClient _client;
    private NetworkStream _stream;



    private bool _isConnected = false;

    CancellationTokenSource starttoken = new CancellationTokenSource();
    

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
        Debug.Log($" SendToTcpServer(ConnectionState connectionState, object messageData)의 종료");

    }
    #endregion

    #region 수신함수

    public IEnumerator ReceiveFromTCPServerCoroutine()
    {
        byte[] buffer = new byte[1024];
        //int testingnumb = 0;

        //yield return null;  // 초기화 후 한 프레임 대기
        yield return new WaitUntil(() => _stream.DataAvailable);  // 데이터가 있을 때까지 기다림


        while (_client.Connected)  // 클라이언트가 연결되어 있을 때만 데이터 수신
        {
            //testingnumb++;


            if (starttoken.IsCancellationRequested)
            {
                Debug.Log("작업이 취소되었습니다.");
                yield break;
            }

            //Debug.Log("if (starttoken.IsCancellationRequested)");

            Debug.Log($"_stream.DataAvailable: {_stream.DataAvailable}");
            // 데이터를 읽을 준비가 되었다면
            yield return new WaitUntil(() => _stream.DataAvailable);  // 데이터가 있을 때까지 기다림




            int bytesRead = _stream.Read(buffer, 0, buffer.Length);
            Debug.Log($"bytesRead: {bytesRead}");

            try
            {

                // 데이터 수신
                //int bytesRead = 0;//_stream.Read(buffer, 0, buffer.Length);
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
            }

            // 데이터를 처리하고 한 프레임 대기
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

        try
        {
            // JSON을 JObject로 변환
            JObject jsonObject = JObject.Parse(message.ToString());

            // "data" 항목 확인
            if (jsonObject["data"] != null)
            {
                // playerId 항목 확인
                //if (jsonObject["data"]["playerId"] != null)
                //{
                //    // playerId 값을 처리
                //    int playerId = jsonObject["data"]["playerId"].ToObject<int>();
                //    GameManager.Instance.SetPlayerId(playerId);
                //    Debug.Log($"playerId found: {playerId}");
                //}
                //else
                //{
                //    Debug.Log("playerId not found in data.");
                //}

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

    #region 연결 및 종료 처리
    public IEnumerator StartConnectionCoroutine(string inputIp, int inputPort)
    {


        Debug.Log($"StartConnectionCoroutine {inputIp}, {inputPort}");
        // TCP 클라이언트 연결
        _client = new TcpClient(inputIp, inputPort);
        _stream = _client.GetStream();
        _isConnected = true;
        Debug.Log("서버에 연결되었습니다.");
        Debug.Log($"yield return new WaitUntil(() => _stream.DataAvailable);  : {_stream.DataAvailable}");
        Debug.Log($"yield return new WaitUntil(() => _stream.DataAvailable);  : {_stream.DataAvailable}");
        Debug.Log($"yield return new WaitUntil(() => _stream.DataAvailable);  : {_stream.DataAvailable}");
        Debug.Log($"yield return new WaitUntil(() => _stream.DataAvailable);  : {_stream.DataAvailable}");
        Debug.Log($"yield return new WaitUntil(() => _stream.DataAvailable);  : {_stream.DataAvailable}");
        Debug.Log($"yield return new WaitUntil(() => _stream.DataAvailable);  : {_stream.DataAvailable}");
        Debug.Log($"yield return new WaitUntil(() => _stream.DataAvailable);  : {_stream.DataAvailable}");
        // 서버와 연결 후 초기화 작업
        SendToTcpServer(ConnectionState.Connecting, new { playerName = "client" });
        Debug.Log("서버와 연결 후 초기화 작업 끝");
        Debug.Log($"yield return new WaitUntil(() => _stream.DataAvailable);  : {_stream.DataAvailable}");

        // TCP 데이터 수신 시작
        yield return StartCoroutine(ReceiveFromTCPServerCoroutine());
        //yield return null;
        Debug.Log("TCP 데이터 수신 종료");
    }

    public void ConnectServer(string inputIp, int inputPort)
    {

        StartCoroutine(StartConnectionCoroutine(inputIp, inputPort));
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

public class ServerInfo
{
    public string ipAddress;
    public int tcpPort;
    public int udpPort;

    public ServerInfo(string ip, int tcp, int udp)
    {
        ipAddress = ip;
        tcpPort = tcp;
        udpPort = udp;
    }
}

public class ServerList
{
    private List<ServerInfo> servers;

    // JSON 문자열로 초기화
    public ServerList(string jsonData)
    {
        servers = JsonConvert.DeserializeObject<List<ServerInfo>>(jsonData);
    }

    // 특정 서버 정보 가져오기
    public (string ipAddress, int tcpPort, int udpPort) GetServerInfo(int index)
    {
        if (index < 0 || index >= servers.Count)
        {
            throw new ArgumentOutOfRangeException("Invalid index for server list");
        }

        var server = servers[index];
        return (server.ipAddress, server.tcpPort, server.udpPort);
    }

    // 서버 수 가져오기
    public int ServerCount => servers.Count;
}