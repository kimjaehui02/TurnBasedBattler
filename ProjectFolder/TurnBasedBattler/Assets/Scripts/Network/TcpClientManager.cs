using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Windows;

public class TcpClientManager : MonoBehaviour
{

    #region ������

    private TcpClient _client;
    private NetworkStream _stream;
    //private bool _isConnected;

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


    public void OnApplicationQuit()
    {
        Quit();
    }

    #region �⺻���
    // UDP Ŭ���̾�Ʈ �ʱ�ȭ
    public void ConnectServer(string ServerIp, int ServerPort)
    {
        if(_client != null)
        {
            Debug.Log("if(_isConnected == true)if(_isConnected == true)if(_isConnected == true)");
            Debug.Log("if(_isConnected == true)if(_isConnected == true)if(_isConnected == true)");
            Debug.Log("if(_isConnected == true)if(_isConnected == true)if(_isConnected == true)");
            Debug.Log("if(_isConnected == true)if(_isConnected == true)if(_isConnected == true)");
            Quit();
        }


        #region ���ڵ�
        try
        {
            // TCP Ŭ���̾�Ʈ ����
            _client = new TcpClient(ServerIp, ServerPort);
            _stream = _client.GetStream();


            // ������ ���� �� �ʱ�ȭ �۾�
            SendToTcpServer(ConnectionState.Connecting, new { playerName = "client" });

            // TCP ������ ���� ����
            StartCoroutine(ReceiveFromTCPServerCoroutine(ServerIp, ServerPort));

        }
        catch (Exception e)
        {
            Debug.Log("���� ����ó����");
            Debug.Log("���� ����ó����");
            Debug.Log("���� ����ó����");
            Debug.Log("���� ����ó����");

            Debug.Log(e.ToString());
            StartCoroutine(ReStart(ServerIp, ServerPort));
        }

        #endregion



        // ����� ����� ������
        //try
        //{
        //    // �񵿱�� TcpClient ����
        //    Task.Run(async () =>
        //    {
        //        _client = await Task.Run(() => new TcpClient(ServerIp, ServerPort));
        //        _stream = _client.GetStream();
        //        Debug.Log("���� ����ó����????");
        //        Debug.Log("���� ����ó����????");
        //        Debug.Log("���� ����ó����????");
        //        Debug.Log("���� ����ó����????");
        //        Debug.Log("�۽���????");
        //        Debug.Log("�۽���????");
        //        Debug.Log("�۽���????");
        //        Debug.Log("�۽���????");
        //        // TCP ������ ���� ����
        //        Debug.Log("���ô�????");
        //        Debug.Log("���ô�????");
        //        Debug.Log("���ô�????");
        //        Debug.Log("���ô�????");

        //    });
        //    SendToTcpServer(ConnectionState.Connecting, new { playerName = "client" });
        //    StartCoroutine(ReceiveFromTCPServerCoroutine(ServerIp, ServerPort));

        //}
        //catch (Exception e)
        //{
        //    Debug.Log("���� ����ó����");
        //    Debug.Log("���� ����ó����");
        //    Debug.Log("���� ����ó����");
        //    Debug.Log("���� ����ó����");

        //    Debug.Log(e.ToString());
        //    StartCoroutine(ReStart(ServerIp, ServerPort));
        //}


    }

    private IEnumerator ReStart(string ServerIp, int ServerPort)
    {
        yield return new WaitForSeconds(5f);  // 5�� ��ٸ�
        ConnectServer(ServerIp, ServerPort);
    }

    // UDP ������ �۽�
    public void SendToTcpServer(ConnectionState connectionState, object messageData)
    {
        // ������ �۽� �ڵ�
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
            Debug.Log($"TCP ���� �� ���� �߻�: {ex.Message}");
        }
        

    }

    public IEnumerator ReceiveFromTCPServerCoroutine(string ServerIp, int ServerPort)
    {
        byte[] buffer = new byte[1024];
        bool isForceDisconnected = false;
        while (true)  // Ŭ���̾�Ʈ�� ����Ǿ� ���� ���� ������ ����
        {
            try
            {
                if (_client.Connected == false)
                {
                    Debug.Log("��������");
                    Debug.Log("��������");
                    Debug.Log("��������");
                    Debug.Log("��������");
                    Debug.Log("��������");

                    break;
                }
            }
            catch (Exception ex)  // ���ܸ� �а� ��� ���
            {
                Debug.Log("���� TC �� ���� ������ ������");
                Debug.Log("���� TC �� ���� ������ ������");
                Debug.Log($"���� �߻�: {ex.Message}");  // ���� �α�
                Debug.Log("���� TC �� ���� ������ ������");
                Debug.Log("���� TC �� ���� ������ ������");
                isForceDisconnected = true;
                break;
            }




            if (!_stream.DataAvailable)
            {
                yield return null;  // ������ �غ� ���¿����� �۾��� �����ϵ��� ��
                continue;
            }

            int bytesRead = _stream.Read(buffer, 0, buffer.Length);
            Debug.Log($"bytesRead: {bytesRead}");

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

            // �����͸� ó���ϰ� �� ������ ���
            yield return null;
        }

        if (isForceDisconnected == true)
        {
            yield return new WaitForSeconds(5f);  // 5�� ��ٸ�
            ConnectServer(ServerIp, ServerPort);
        }
    }

    // Ŭ���̾�Ʈ ����
    public void Quit()
    {
        try
        {
            SendToTcpServer(ConnectionState.Disconnecting, new { playerId = GameManager.Instance.GetPlayerId() });
            //_isConnected = false;
            _stream?.Close();
            _stream = null;
            _client?.Close();
            _client = null;
            GameManager.Instance.SetPlayerId(-1);


            Debug.Log("�������� ������ �����߽��ϴ�.");
            //starttoken?.Cancel();
        }
        catch (Exception ex)
        {
            Debug.LogError($"���� ���� �� ���� �߻�: {ex.Message}");
        }
    }
    #endregion




    #region �ڵ鷯

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
        Debug.Log($"{message}");

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

                    GameManager.Instance.SetServerUi(subServerList);

                    // ù ��° ���� ���� ����
                    //ServerInfo firstServer = subServerList[0];
                    //string ip = firstServer.ipAddress;
                    //int tcp = firstServer.tcpPort;
                    //int udp = firstServer.udpPort;
                    //GameManager.Instance.ConnectSubServer(ip, tcp, udp);

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
