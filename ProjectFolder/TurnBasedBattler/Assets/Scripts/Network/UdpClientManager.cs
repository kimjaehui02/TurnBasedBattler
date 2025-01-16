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

    #region ��ſ� ������
    [SerializeField]
    private TransformManager transformManager;

    private UdpClient udpClient;
    private UdpClient udpClientReceive;
    //private bool _isConnected;

    private const float sendInterval = 0.01f; // ��ǥ ���� ����
    private float lastSendTime = 0f;

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

    void Update()
    {
        Updatecyle();
    }

    #region �⺻���

    // UDP Ŭ���̾�Ʈ �ʱ�ȭ
    public void ConnectServer(string ServerIp, int ServerPort)
    {
        if (udpClient != null)
        {
            Quit();
        }
        // Ŭ���̾�Ʈ �ʱ�ȭ �ڵ�
        udpClient = new UdpClient(ServerIp, ServerPort);
        //_isConnected = true;
        SendToUDPServer(ConnectionState.Connecting, new { playerName = "Player1" });

        //CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        //CancellationToken tokens = cancellationTokenSource.Token;

        // �������� ��� ����
        //yield return StartCoroutine(RunServerCoroutine(udpClient, cancellationTokenSource.Token));
        StartCoroutine(ReceiveFromUDPServerCoroutine());
        //yield return null;

        Debug.Log("������ �����.");
    }

    // UDP ������ �۽�
    public void SendToUDPServer(ConnectionState connectionState, object messageData)
    {
        // ������ �۽� �ڵ�
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
            //Debug.Log($"������ JSON: {jsonMessage}");

            byte[] data = CompressionManager.CompressJson(jsonMessage);
            //byte[] data = Encoding.UTF8.GetBytes(jsonMessage);
            udpClient.Send(data, data.Length);
        }
        catch (Exception ex)
        {
            Debug.Log($"UDP ���� �� ���� �߻�: {ex.Message}");
        }
    }

    

    // UDP ������ ����
    public IEnumerator ReceiveFromUDPServerCoroutine()
    {
        IPEndPoint remoteEP = null;
        byte[] data;

        // ������ ���� �ڵ�
        while (true)
        {

            if (udpClient.Available > 0)
            {

                try
                {
                    //udpClient.Client.ReceiveTimeout = 5;  // 1�� Ÿ�Ӿƿ� ����
                    data = udpClient.Receive(ref remoteEP);

                }
                catch
                {
                    Debug.Log("���������K �� ������ ����");
                    Debug.Log("���������K �� ������ ����");
                    Debug.Log("���������K �� ������ ����");
                    Debug.Log("���������K �� ������ ����");
                    continue;
                }

                //byte[] data = result.Buffer;  // ���� ������
                //IPEndPoint remoteEP = result.RemoteEndPoint; // �۽��� ����

                //string json = Encoding.UTF8.GetString(data); // UTF-8�� ���ڿ� ��ȯ

                var json = CompressionManager.DecompressJson(data);
                Debug.Log($"Received jsondata: {json}");


                // JSON �Ľ�
                var message = JsonConvert.DeserializeObject<dynamic>(json);
                if (message != null)
                {
                    // �޽��� ó��
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


            // ���� �ð� ���� �� �ݺ�
            yield return null;  // ����ؼ� �����͸� ���
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
                udpClient = null;  // null�� �ʱ�ȭ
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

    #region �ڵ鷯


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

        // message�� �������� ����ϰ� �ʹٸ�
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
