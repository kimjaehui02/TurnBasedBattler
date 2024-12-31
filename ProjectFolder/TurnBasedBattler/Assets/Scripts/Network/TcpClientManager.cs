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
    #region ��ſ� ������
    //private const string ServerIp = "127.0.0.1";  // ���� IP
    //private const int ServerPort = 9090;  // ���� ��Ʈ
    private TcpClient _client_main;
    private NetworkStream _stream_main;
    private TcpClient _client_sub;
    private NetworkStream _stream_sub;



    private bool _isConnected_main = false;
    private bool _isConnected_sub = false;

    CancellationTokenSource starttoken = new CancellationTokenSource();
    

    #endregion

    #region �����÷��̿� ������
    public GameObject myPlayerObject;  // �� �÷��̾� ������Ʈ
    #endregion

    #region json�����
    public enum ConnectionState
    {
        Default,      // �⺻ ����
        Connecting,   // ���� �õ� ��
        DataSyncing,  // ������ ����ȭ ��
        Disconnecting,// ���� ���� �õ� ��
        Error,        // ���� �߻�
        TcpToUdp      // tcp���� udp�� �̵���ŵ�ϴ�
    }
    #endregion

    #region �۽��Լ�
    public void SendToTcpServer(ConnectionState connectionState, object messageData, bool tomain)
    {
        try
        {
            // �޽��� ����
            var message = new
            {
                connectionState = connectionState.ToString(), // Enum ���� ���ڿ��� ��ȯ
                data = messageData
            };

            // ����ȭ �� ���� ���� ���� ����
            var settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };

            // ��ü�� JSON ���ڿ��� ����ȭ
            string jsonMessage = JsonConvert.SerializeObject(message, Formatting.Indented, settings);

            // ���������� JSON ���
            Debug.Log($"������ JSON: {jsonMessage}");

            // TCP�� ���� ������ �޽��� ����
            byte[] data = Encoding.UTF8.GetBytes(jsonMessage);
            if (tomain == true)
            {
                _stream_main.Write(data, 0, data.Length);

            }
            else
            {
                _stream_sub.Write(data, 0, data.Length);

            }

        }
        catch (Exception ex)
        {
            Debug.LogError($"TCP ���� �� ���� �߻�: {ex.Message}");
        }
        Debug.Log($" SendToTcpServer(ConnectionState connectionState, object messageData)�� ����");

    }
    #endregion

    #region �����Լ�

    public IEnumerator ReceiveFromTCPServerCoroutine()
    {
        byte[] buffer = new byte[1024];
        //int testingnumb = 0;

        //yield return null;  // �ʱ�ȭ �� �� ������ ���
        yield return new WaitUntil(() => _stream_main.DataAvailable);  // �����Ͱ� ���� ������ ��ٸ�


        while (_client_main.Connected)  // Ŭ���̾�Ʈ�� ����Ǿ� ���� ���� ������ ����
        {
            //testingnumb++;


            if (starttoken.IsCancellationRequested)
            {
                Debug.Log("�۾��� ��ҵǾ����ϴ�.");
                yield break;
            }


            // �����͸� ���� �غ� �Ǿ��ٸ�
            yield return new WaitUntil(() => _stream_main.DataAvailable);  // �����Ͱ� ���� ������ ��ٸ�




            try
            {
                int bytesRead = _stream_main.Read(buffer, 0, buffer.Length);


                // ������ ����
                //int bytesRead = 0;//_stream.Read(buffer, 0, buffer.Length);
                if (bytesRead > 0)
                {
                    byte[] data = new byte[bytesRead];
                    Array.Copy(buffer, 0, data, 0, bytesRead);

                    // ���� �����͸� UTF-8 ���ڿ��� ��ȯ
                    string json = Encoding.UTF8.GetString(data);

                    // JSON ó��
                    try
                    {
                        var message = JsonConvert.DeserializeObject<dynamic>(json);
                        if (message != null)
                        {
                            HandleConnectionState(message);  // ���� ó��
                            
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

            // �����͸� ó���ϰ� �� ������ ���
            yield return null;
        }
    }
    
    public IEnumerator ReceiveFromTCPSubServerCoroutine()
    {
        byte[] buffer = new byte[1024];
        //int testingnumb = 0;

        //yield return null;  // �ʱ�ȭ �� �� ������ ���
        yield return new WaitUntil(() => _stream_sub.DataAvailable);  // �����Ͱ� ���� ������ ��ٸ�


        while (_client_sub.Connected)  // Ŭ���̾�Ʈ�� ����Ǿ� ���� ���� ������ ����
        {
            //testingnumb++;


            if (starttoken.IsCancellationRequested)
            {
                Debug.Log("�۾��� ��ҵǾ����ϴ�.");
                yield break;
            }


            // �����͸� ���� �غ� �Ǿ��ٸ�
            yield return new WaitUntil(() => _stream_sub.DataAvailable);  // �����Ͱ� ���� ������ ��ٸ�




            try
            {
                int bytesRead = _stream_sub.Read(buffer, 0, buffer.Length);


                // ������ ����
                //int bytesRead = 0;//_stream.Read(buffer, 0, buffer.Length);
                if (bytesRead > 0)
                {
                    byte[] data = new byte[bytesRead];
                    Array.Copy(buffer, 0, data, 0, bytesRead);

                    // ���� �����͸� UTF-8 ���ڿ��� ��ȯ
                    string json = Encoding.UTF8.GetString(data);

                    // JSON ó��
                    try
                    {
                        var message = JsonConvert.DeserializeObject<dynamic>(json);
                        if (message != null)
                        {
                            HandleConnectionState(message);  // ���� ó��

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

            // �����͸� ó���ϰ� �� ������ ���
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
            // JSON�� JObject�� ��ȯ
            JObject jsonObject = JObject.Parse(message.ToString());

            // "data" �׸� Ȯ��
            if (jsonObject["data"] != null)
            {
                // playerId �׸� Ȯ��
                if (jsonObject["data"]["playerId"] != null)
                {
                    // playerId ���� ó��
                    int playerId = jsonObject["data"]["playerId"].ToObject<int>();
                    GameManager.Instance.SetPlayerId(playerId);
                    Debug.Log($"playerId found: {playerId}");
                }
                else
                {
                    Debug.Log("playerId not found in data.");
                }

                // "subServerList" �׸� Ȯ��
                if (jsonObject["data"]["subServerList"] != null && jsonObject["data"]["subServerList"].HasValues)
                {
                    // subServerList ���� ��� ���� ������ ó��
                    List<ServerInfo> subServerList = jsonObject["data"]["subServerList"].ToObject<List<ServerInfo>>();

                    // ù ��° ���� ���� ����
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

    #region ���� �� ���� ó��
    public IEnumerator StartConnectionCoroutine(string inputIp, int inputPort)
    {
        // TCP Ŭ���̾�Ʈ ����
        _client_main = new TcpClient(inputIp, inputPort);
        _stream_main = _client_main.GetStream();


        _isConnected_main = true;

        // ������ ���� �� �ʱ�ȭ �۾�
        SendToTcpServer(ConnectionState.Connecting, new { playerName = "client" }, true);

        // TCP ������ ���� ����
        yield return StartCoroutine(ReceiveFromTCPServerCoroutine());
        //yield return null;
        Debug.Log("TCP ������ ���� ����");
    }

    public void ConnectServer(string inputIp, int inputPort)
    {

        StartCoroutine(StartConnectionCoroutine(inputIp, inputPort));
    }

    public IEnumerator StartSubConnectionCoroutine(string inputIp, int inputPort)
    {

        // TCP Ŭ���̾�Ʈ ����);


        _client_sub = new TcpClient(inputIp, inputPort);
        _stream_sub = _client_sub.GetStream();


        _isConnected_sub = true;
        // ������ ���� �� �ʱ�ȭ �۾�

        SendToTcpServer(ConnectionState.Connecting, new { playerName = "client" }, false);


        // TCP ������ ���� ����
        yield return StartCoroutine(ReceiveFromTCPSubServerCoroutine());
        //yield return null;

    }

    public void ConnectSubServer(string inputIp, int inputPort)
    {

        StartCoroutine(StartSubConnectionCoroutine(inputIp, inputPort));
    }




    // ������ ������ �����ϴ� �޼���
    public void Quit()
    {
        try
        {
            if (_isConnected_main)
            {
                _isConnected_main = false;
                _isConnected_sub = false;
                _stream_main?.Close();
                _client_main?.Close();
                _stream_sub?.Close();
                _client_sub?.Close();
                Debug.Log("�������� ������ �����߽��ϴ�.");
            }
            starttoken?.Cancel();
        }
        catch (Exception ex)
        {
            Debug.LogError($"���� ���� �� ���� �߻�: {ex.Message}");
        }
    }

    // ���ø����̼� ���� �� ȣ��
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

    // JSON ���ڿ��� �ʱ�ȭ
    public ServerList(string jsonData)
    {
        servers = JsonConvert.DeserializeObject<List<ServerInfo>>(jsonData);
    }

    // Ư�� ���� ���� ��������
    public (string ipAddress, int tcpPort, int udpPort) GetServerInfo(int index)
    {
        if (index < 0 || index >= servers.Count)
        {
            throw new ArgumentOutOfRangeException("Invalid index for server list");
        }

        var server = servers[index];
        return (server.ipAddress, server.tcpPort, server.udpPort);
    }

    // ���� �� ��������
    public int ServerCount => servers.Count;
}