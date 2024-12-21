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

public class OldUdp : MonoBehaviour
{
    #region ��ſ� ������
    private const string ServerIp = "127.0.0.1"; // ���� IP
    private const int ServerPort = 9090; // ���� ��Ʈ
    private UdpClient udpClient;
    public int myId = -1;  // �� id, �������� �޾ƿ� ������ ����
    CancellationTokenSource cancellationTokenSource;

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
        Error,        // ���� �߻�
        TcpToUdp      // tcp���� udp�� �̵���ŵ�ϴ�
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

    // ���� ���� �޼���
    public async Task RunServerAsync(UdpClient udpServer, CancellationToken token)
    {
        Console.WriteLine($"1. RunServerAsync�������� : ");
        try
        {
            Console.WriteLine($"2. try : ");
            while (!token.IsCancellationRequested)
            {

                Console.WriteLine($"3. while (!token.IsCancellationRequested) : ");
                // Ŭ���̾�Ʈ�κ��� ������ ����
                //UdpReceiveResult receivedResult = await udpServer.ReceiveAsync();

                // ������ ������ ���
                //string receivedMessage = Encoding.UTF8.GetString(receivedResult.Buffer);


                // ������ �����͸� ���������� ó��
                await ReceiveFromUDPServer(udpServer);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"���� ���� �߻�: {ex.Message}");
        }
        Console.WriteLine($"4. RunServerAsync�������� : ");
    }

    public async Task ReceiveFromUDPServer(UdpClient udpServer)
    {

        // ������ ������ ���� ���� ����
        //IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, ServerPort);
        Debug.Log($"public async Task ReceiveFromUDPServer(UdpClient udpServer)");
        try
        {
            Debug.Log($"try");
            // ������ ����
            UdpReceiveResult result = await udpServer.ReceiveAsync();
            Debug.Log($"UdpReceiveResult result = await udpServer.ReceiveAsync();");
            byte[] data = result.Buffer;  // UdpReceiveResult���� Buffer �Ӽ����� ������ ����
            Debug.Log($"byte[] data = result.Buffer;  // UdpReceiveResult���� Buffer �Ӽ����� ������ ����");
            IPEndPoint remoteEP = result.RemoteEndPoint;  // Ŭ���̾�Ʈ�� IP�� ��Ʈ ��ȣ�� ���� IPEndPoint
            Debug.Log($"IPEndPoint remoteEP = result.RemoteEndPoint;  // Ŭ���̾�Ʈ�� IP�� ��Ʈ ��ȣ�� ���� IPEndPoint");

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
        //udpClient.BeginReceive(ReceiveFromUDPServer, null);
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
    public async Task StartConnection()
    {
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
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken token = cancellationTokenSource.Token;

            // �������� ��� ����
            await RunServerAsync(udpClient, cancellationTokenSource.Token);


            Debug.Log("������ �����.");
        }
        else
        {
            // �̹� ����� ���¶�� ���� �缳�� ���� ����
            Debug.Log("������ �̹� ����Ǿ� ����.");
        }
    }

    public void Updatecyle()
    {
        // ���� ��� �ð��� �������� ��
        float currentTime = Time.realtimeSinceStartup;

        // ���� ���ݸ��� ������ ����
        if (currentTime - lastSendTime >= sendInterval)
        {
            // ������ ���� �Լ� ȣ��
            SendToUDPServer(ConnectionState.DataSyncing, new
            {
                positionX = myPlayerObject.transform.position.x,
                positionY = myPlayerObject.transform.position.y,
            });

            // ������ ���� �ð� ����
            lastSendTime = currentTime;
        }
    }

    // 1. ���� ���� ��û
    void Start()
    {
        Application.runInBackground = true;

        cancellationTokenSource = new CancellationTokenSource(); ;
        CancellationToken token = cancellationTokenSource.Token;
        Task.Run(async () => await StartConnection());

    }

    //// 2. �ʴ� ���� Ƚ���� �����͸� �ְ�ޱ�
    //private float lastSendTime = 0f;  // ������ ���� �ð�
    //private float sendInterval = 0.05f; // ���� ���� (��)

    void Update()
    {
        Updatecyle();
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


    #region �ڷ�ƾ
    private IEnumerator RunServerAsyncCoroutine()
    {

        yield return null; // ����ؼ� ��ٸ�
    }
    #endregion
}
