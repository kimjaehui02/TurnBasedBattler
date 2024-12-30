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
    private TcpClient _client;
    private NetworkStream _stream;



    private bool _isConnected = false;

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
    public void SendToTcpServer(ConnectionState connectionState, object messageData)
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
            _stream.Write(data, 0, data.Length);
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
        yield return new WaitUntil(() => _stream.DataAvailable);  // �����Ͱ� ���� ������ ��ٸ�


        while (_client.Connected)  // Ŭ���̾�Ʈ�� ����Ǿ� ���� ���� ������ ����
        {
            //testingnumb++;


            if (starttoken.IsCancellationRequested)
            {
                Debug.Log("�۾��� ��ҵǾ����ϴ�.");
                yield break;
            }

            //Debug.Log("if (starttoken.IsCancellationRequested)");

            Debug.Log($"_stream.DataAvailable: {_stream.DataAvailable}");
            // �����͸� ���� �غ� �Ǿ��ٸ�
            yield return new WaitUntil(() => _stream.DataAvailable);  // �����Ͱ� ���� ������ ��ٸ�




            int bytesRead = _stream.Read(buffer, 0, buffer.Length);
            Debug.Log($"bytesRead: {bytesRead}");

            try
            {

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
                //if (jsonObject["data"]["playerId"] != null)
                //{
                //    // playerId ���� ó��
                //    int playerId = jsonObject["data"]["playerId"].ToObject<int>();
                //    GameManager.Instance.SetPlayerId(playerId);
                //    Debug.Log($"playerId found: {playerId}");
                //}
                //else
                //{
                //    Debug.Log("playerId not found in data.");
                //}

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


        Debug.Log($"StartConnectionCoroutine {inputIp}, {inputPort}");
        // TCP Ŭ���̾�Ʈ ����
        _client = new TcpClient(inputIp, inputPort);
        _stream = _client.GetStream();
        _isConnected = true;
        Debug.Log("������ ����Ǿ����ϴ�.");
        Debug.Log($"yield return new WaitUntil(() => _stream.DataAvailable);  : {_stream.DataAvailable}");
        Debug.Log($"yield return new WaitUntil(() => _stream.DataAvailable);  : {_stream.DataAvailable}");
        Debug.Log($"yield return new WaitUntil(() => _stream.DataAvailable);  : {_stream.DataAvailable}");
        Debug.Log($"yield return new WaitUntil(() => _stream.DataAvailable);  : {_stream.DataAvailable}");
        Debug.Log($"yield return new WaitUntil(() => _stream.DataAvailable);  : {_stream.DataAvailable}");
        Debug.Log($"yield return new WaitUntil(() => _stream.DataAvailable);  : {_stream.DataAvailable}");
        Debug.Log($"yield return new WaitUntil(() => _stream.DataAvailable);  : {_stream.DataAvailable}");
        // ������ ���� �� �ʱ�ȭ �۾�
        SendToTcpServer(ConnectionState.Connecting, new { playerName = "client" });
        Debug.Log("������ ���� �� �ʱ�ȭ �۾� ��");
        Debug.Log($"yield return new WaitUntil(() => _stream.DataAvailable);  : {_stream.DataAvailable}");

        // TCP ������ ���� ����
        yield return StartCoroutine(ReceiveFromTCPServerCoroutine());
        //yield return null;
        Debug.Log("TCP ������ ���� ����");
    }

    public void ConnectServer(string inputIp, int inputPort)
    {

        StartCoroutine(StartConnectionCoroutine(inputIp, inputPort));
    }




    // ������ ������ �����ϴ� �޼���
    public void Quit()
    {
        try
        {
            if (_isConnected)
            {
                _isConnected = false;
                _stream?.Close();
                _client?.Close();
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