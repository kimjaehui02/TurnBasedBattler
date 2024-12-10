using UnityEngine;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System;
using Newtonsoft.Json;
using System.Collections;
using UnityEngine.UIElements;
//using UnityEditor.VersionControl;

public class UdpClientManager : MonoBehaviour
{
    #region ��ſ� ������
    private const string ServerIp = "127.0.0.1"; // ���� IP
    private const int ServerPort = 7777; // ���� ��Ʈ
    private UdpClient udpClient;
    public int myId = -1;  // �� id, �������� �޾ƿ� ������ ����

    private float sendInterval = 5f; // ��ǥ ���� ����
    private float lastSendTime = 0f;
    #endregion

    #region �����÷��̿� ������
    public GameObject playerPrefab;  // �÷��̾� ������Ʈ�� �������� �����Ϳ��� �巡�� �� ������� ����
    private Dictionary<int, GameObject> playerObjects = new Dictionary<int, GameObject>();
    public GameObject myPlayerObject; // �� �÷��̾� ������Ʈ

    #endregion

    #region json�����
    public enum ConnectionState
    {
        Default,      // �⺻ ����
        Connecting,   // ���� �õ� ��
        DataSyncing,  // ������ ����ȭ ��
        Disconnecting,// ���� ���� �õ� ��
        Error         // ���� �߻�
    }
    #endregion

    #region �۽��Լ�
    public void SendToUDPServer(ConnectionState connectionState, object messageData)
    {

        Debug.Log($"SendToUDPServer���� connectionState : {connectionState}, messageData : {messageData} ");

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

            // UDP�� ���� ������ �޽��� ����
            byte[] data = Encoding.UTF8.GetBytes(jsonMessage);
            udpClient.Send(data, data.Length);
        }
        catch (Exception ex)
        {
            Debug.Log($"UDP ���� �� ���� �߻�: {ex.Message}");
        }
    }
    #endregion

    #region �����Լ�
    public void ReceiveFromUDPServer(IAsyncResult ar)
    {
        
        // ������ ������ ���� ���� ����
        IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, ServerPort);

        try
        {
            // UDP Ŭ���̾�Ʈ�� ����Ͽ� ������ ����
            byte[] data = udpClient.EndReceive(ar, ref endPoint);

            // ����Ʈ �迭�� UTF-8 ���ڿ��� ��ȯ
            string json = Encoding.UTF8.GetString(data);

            // ������ JSON �����͸� �ֿܼ� ���
            Debug.Log("Received data: " + json);

            // JSON ���ڿ��� Newtonsoft.Json���� ó�� (dynamic ���)
            try
            {
                // JSON �Ľ� (Newtonsoft.Json ���)
                var message = JsonConvert.DeserializeObject<dynamic>(json);
                Debug.Log($"ReceiveFromUDPServer���� message : {message}");
                if (message != null)
                {
                    // 'connectionState'�� ���� ó��
                    HandleConnectionState(message);
                }
                else
                {
                    Debug.Log("Failed to parse JSON.");
                }
            }
            catch (Exception ex)
            {
                // JSON �Ľ� ���� ó��
                Debug.Log($"Error parsing JSON: {ex.Message}");
            }
        }
        catch (Exception ex)
        {
            // ���� ó��: ������ ���� �� ���� �߻� ��
            Debug.Log($"Error receiving data: {ex.Message}");
        }

        
        // ����ؼ� �����͸� ������ �� �ֵ��� ���
        udpClient.BeginReceive(ReceiveFromUDPServer, null);
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
        myId = message.data.playerId; // message���� playerId�� �����Ͽ� myId�� �Ҵ�
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

    #region ��� ����, ������ �ۼ���, ���� �˸�
    // 1. ���� ���� ��û
    void Start()
    {
        Application.runInBackground = true;
        // ������ ���� ���°� �ƴ� ��쿡�� ���� ����
        if (udpClient == null || udpClient.Client == null || udpClient.Client.Connected == false)
        {
            // ���� Ŭ���̾�Ʈ�� �����ϸ� �����ϰ� ���� ����
            if (udpClient != null)
            {
                Quit();
            }

            udpClient = new UdpClient(ServerIp, ServerPort);

            // ������ ���� ��û ������
            SendToUDPServer(ConnectionState.Connecting, new { playerName = "Player1" });

            // �������� ��� ����
            udpClient.BeginReceive(ReceiveFromUDPServer, null);

            Debug.Log("������ �����.");
        }
        else
        {
            // �̹� ����� ���¶�� ���� �缳�� ���� ����
            Debug.Log("������ �̹� ����Ǿ� ����.");
        }
    }

    //// 2. �ʴ� ���� Ƚ���� �����͸� �ְ�ޱ�
    //private float lastSendTime = 0f;  // ������ ���� �ð�
    //private float sendInterval = 0.05f; // ���� ���� (��)

    void Update()
    {
        // ���� ��� �ð��� �������� ��
        float currentTime = Time.realtimeSinceStartup;

        // ���� ���ݸ��� ������ ����
        if (currentTime - lastSendTime >= sendInterval)
        {
            // ������ ���� �Լ� ȣ��
            SendToUDPServer(ConnectionState.DataSyncing, new { 
                positionX = myPlayerObject.transform.position.x,
                positionY = myPlayerObject.transform.position.y,
            });

            // ������ ���� �ð� ����
            lastSendTime = currentTime;
        }
    }

    // 3. ���� ��� (���� ���� �Ǵ� ���� ���� ��)
    public void OnApplicationQuit()
    {
        Quit();
    }

    public void Quit()
    {
        try
        {
            // ���� �˸���
            SendToUDPServer(ConnectionState.Disconnecting, new { playerId = myId });

            // UDP Ŭ���̾�Ʈ ����
            if (udpClient != null)
            {
                udpClient.Close();
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
