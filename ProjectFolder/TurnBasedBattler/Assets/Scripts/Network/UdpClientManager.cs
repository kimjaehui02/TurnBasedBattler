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

    #region ��ſ� ������
    //private const string ServerIp = "127.0.0.1"; // ���� IP
    //private const int ServerPort = 9090; // ���� ��Ʈ
    private UdpClient udpClient;
    //public int myId = -1;  // �� id, �������� �޾ƿ� ������ ����
    CancellationTokenSource cancellationTokenSource;

    private const float sendInterval = 0.02f; // ��ǥ ���� ����
    private float lastSendTime = 0f;

    [SerializeField]
    private bool isConnected = false;  // ���� ���� ���� ���� �߰�
    #endregion

    #region �����÷��̿� ������
    //public GameObject playerPrefab;  // �÷��̾� ������Ʈ�� �������� �����Ϳ��� �巡�� �� ������� ����
    //private Dictionary<int, GameObject> playerObjects = new Dictionary<int, GameObject>();
    //public GameObject myPlayerObject; // �� �÷��̾� ������Ʈ
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
            var message = new
            {
                connectionState = connectionState.ToString(), // Enum ���� ���ڿ��� ��ȯ
                data = messageData
            };

            var settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };

            string jsonMessage = JsonConvert.SerializeObject(message, Formatting.Indented, settings);
            Debug.Log($"������ JSON: {jsonMessage}");

            byte[] data = CompressionManager.CompressJson(jsonMessage);
            //byte[] data = Encoding.UTF8.GetBytes(jsonMessage);
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
    public void RunServerCoroutine(UdpClient udpServer, CancellationToken token)
    {
        Debug.Log($"1. RunServerCoroutine�������� : ");
        try
        {
            Debug.Log($"2. try : ");
            
            //StartCoroutine(ReceiveFromUDPServerCoroutine(udpServer, token));  // �ڷ�ƾ���� �񵿱� ó��

        }
        catch (Exception ex)
        {
            Debug.Log($"���� ���� �߻�: {ex.Message}");
        }
        Debug.Log($"4. RunServerCoroutine�������� : ");
    }


    public IEnumerator ReceiveFromUDPServerCoroutine(UdpClient udpServer, CancellationToken token)
    {
        Debug.Log("Start receiving data from UDP server...");

        while (!token.IsCancellationRequested)
        {

            // �񵿱������� UDP ������ �ޱ�
            var resultTask = udpServer.ReceiveAsync();

            // ����� �Ϸ�� ������ ���
            yield return new WaitUntil(() => resultTask.IsCompleted);

            // �񵿱� �۾��� �Ϸ�Ǹ� ������ ����
            UdpReceiveResult result = resultTask.Result;

            if (result.Buffer.Length > 0)
            {
                byte[] data = result.Buffer;  // ���� ������
                IPEndPoint remoteEP = result.RemoteEndPoint; // �۽��� ����

                //string json = Encoding.UTF8.GetString(data); // UTF-8�� ���ڿ� ��ȯ

                var json = CompressionManager.DecompressJson(data);
                Debug.Log($"Received jsondata: {json}");


                // JSON �Ľ�
                var message = JsonConvert.DeserializeObject<dynamic>(json);
                if (message != null)
                {
                    // �޽��� ó��
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


            // ���� �ð� ���� �� �ݺ�
            yield return null;  // ����ؼ� �����͸� ���
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

        // message�� �������� ����ϰ� �ʹٸ�
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

    #region ��� ����, ������ �ۼ���, ���� �˸�
    public IEnumerator StartConnectionCoroutine(string inputIp, int inputPort)
    {
        // ������ ���� ���°� �ƴ� ��쿡�� ���� ����
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

            // �������� ��� ����
            //yield return StartCoroutine(RunServerCoroutine(udpClient, cancellationTokenSource.Token));
            yield return StartCoroutine(ReceiveFromUDPServerCoroutine(udpClient, token));

            Debug.Log("������ �����.");
        }
        else
        {
            Debug.Log("������ �̹� ����Ǿ� ����.");
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
        StartCoroutine(StartConnectionCoroutine(inputIp, inputPort));  // �ڷ�ƾ�� ȣ���մϴ�.
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
