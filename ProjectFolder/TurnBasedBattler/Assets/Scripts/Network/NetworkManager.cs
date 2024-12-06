using UnityEngine;
using System.Net.Sockets;
using System.Text;
using System;
using System.Collections;

public class NetworkManager : MonoBehaviour
{
    #region Singleton Pattern
    private static NetworkManager _instance;

    public static NetworkManager Instance => _instance;

    private void Awake()
    {
        if (_instance == null)
            _instance = this;
        else
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }
    #endregion

    private TcpClient _client;
    private NetworkStream _stream;
    private bool _isConnected = false;

    // ������ �����ϴ� �޼���
    public void ConnectToServer(string ip, int port)
    {
        try
        {
            _client = new TcpClient();
            _client.Connect(ip, port);
            _stream = _client.GetStream();
            _isConnected = true;
            Debug.Log("Connected to Server");
        }
        catch (Exception e)
        {
            Debug.LogError("Connection failed: " + e.Message);
        }
    }

    // ������ �޽����� �����ϴ� �޼���
    public void SendNetworkMessage(string type, string data)
    {
        if (_stream == null || !_isConnected) return;

        RequestMessage requestMessage = new RequestMessage(type, data);
        string jsonMessage = JsonUtility.ToJson(requestMessage);
        byte[] dataBytes = Encoding.UTF8.GetBytes(jsonMessage);
        _stream.Write(dataBytes, 0, dataBytes.Length);
        Debug.Log("Message Sent: " + jsonMessage);
    }

    // Update �޼��忡�� �����κ��� �޽����� �޴� ���
    private void Update()
    {
        if (_isConnected && _stream != null)
        {
            ReceiveMessages();
        }
    }

    // �����κ��� �޽����� �޴� �޼���
    private void ReceiveMessages()
    {
        if (_stream.DataAvailable)
        {
            byte[] buffer = new byte[1024];
            int bytesRead = _stream.Read(buffer, 0, buffer.Length);

            if (bytesRead > 0)
            {
                string receivedMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Debug.Log("Received message: " + receivedMessage);
                HandleServerResponse(receivedMessage);
            }
        }
    }

    // ���� ������ ó���ϴ� �޼���
    private void HandleServerResponse(string jsonMessage)
    {
        try
        {
            ResponseMessage responseMessage = JsonUtility.FromJson<ResponseMessage>(jsonMessage);

            switch (responseMessage.command)
            {
                case "PONG":
                    Debug.Log("Received PONG response from server");
                    break;

                case "RequestInitialData":
                    Debug.Log("Received initial data: " + responseMessage.data);
                    break;

                case "CustomCommand":
                    Debug.Log("Received custom command response: " + responseMessage.data);
                    break;

                default:
                    Debug.LogWarning("Unknown command: " + responseMessage.command);
                    break;
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error parsing server response: " + e.Message);
        }
    }

    // �������� ������ �����ϴ� �޼���
    public void Disconnect()
    {
        try
        {
            _isConnected = false;
            _stream?.Close();
            _client?.Close();
            Debug.Log("Disconnected from Server");
        }
        catch (Exception e)
        {
            Debug.LogError("Error during disconnect: " + e.Message);
        }
    }
    public bool IsConnected()
    {
        return _client != null && _client.Connected;
    }
}

[System.Serializable]
public class RequestMessage
{
    public string command;
    public string data;

    public RequestMessage(string command, string data)
    {
        this.command = command;
        this.data = data;
    }
}

[System.Serializable]
public class ResponseMessage
{
    public string command;
    public string data;
}
