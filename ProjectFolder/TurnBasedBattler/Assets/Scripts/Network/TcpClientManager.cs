using UnityEngine;
using System.Net.Sockets;
using System.Text;
using System;
using System.Collections;
using Newtonsoft.Json;
using System.Threading.Tasks;

public class TcpClientManager : MonoBehaviour
{
    #region ��ſ� ������
    private const string ServerIp = "127.0.0.1";  // ���� IP
    private const int ServerPort = 8080;  // ���� ��Ʈ
    private TcpClient _client;
    private NetworkStream _stream;
    private bool _isConnected = false;
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
    }
    #endregion

    #region �����Լ�
    public async Task ReceiveFromTCPServerAsync()
    {
        try
        {
            byte[] buffer = new byte[1024];
            while (_client.Connected)  // Ŭ���̾�Ʈ�� ����Ǿ� ���� ���� ������ ����
            {
                try
                {
                    int bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length); // �񵿱������� ������ ����

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
                    break; // ���� �߻� �� ���� ����
                }
            }
        }
        catch (Exception ex)
        {
            Debug.Log($"Error receiving data: {ex.Message}");
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

    #region ���� �� ���� ó��
    // ������ ������ �����ϴ� �޼���
    public async Task StartConnection()
    {
        try
        {
            // TCP Ŭ���̾�Ʈ ����
            _client = new TcpClient(ServerIp, ServerPort);
            _stream = _client.GetStream();
            _isConnected = true;
            Debug.Log("������ ����Ǿ����ϴ�.");

            // ������ ���� �� �ʱ�ȭ �۾�
            SendToTcpServer(ConnectionState.Connecting, new { playerName = "Player1" });

            // TCP ������ ���� ����
            await ReceiveFromTCPServerAsync();
            Debug.Log("TCP ������ ���� ����");
        }
        catch (Exception ex)
        {
            Debug.LogError($"���� ���� �� ���� �߻�: {ex.Message}");
        }
    }


    private async void Start()
    {
        await StartConnection();
    }

    // ����� ������ ����� ����� �� �ְ� �ϴ� �޼��� (�� �����Ӹ��� ȣ��)
    void Update()
    {

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
