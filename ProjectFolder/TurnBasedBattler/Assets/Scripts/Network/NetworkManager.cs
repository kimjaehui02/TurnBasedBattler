using System.Net.Sockets;  // TCP ��Ʈ��ũ ������ ���� ���Ǵ� ���ӽ����̽�
using System.Text;         // ���ڿ��� ����Ʈ �迭�� ��ȯ�ϱ� ���� ���ӽ����̽�
using UnityEngine;         // Unity ���� ����� ����ϱ� ���� ���ӽ����̽�
using System;



public class NetworkManager : MonoBehaviour
{
    #region Singleton Pattern
    // NetworkManager Ŭ������ �ν��Ͻ��� �����ϴ� ���� ���� (�̱��� ����)
    private static NetworkManager _instance;

    // �ܺο��� NetworkManager�� �ν��Ͻ��� ������ �� �ְ� �ϴ� ������Ƽ
    public static NetworkManager Instance => _instance;

    // �� ��ũ��Ʈ�� ù ����� �� ȣ��Ǵ� �޼���
    private void Awake()
    {
        // Singleton ������ �����Ͽ� NetworkManager�� �ϳ��� �����ϵ��� ����
        if (_instance == null)
            _instance = this;  // ù ��° �ν��Ͻ��� ����
        else
            Destroy(gameObject);  // �� ��° ���� �ν��Ͻ��� �ı�

        // �� ��ȯ �ÿ��� �� ��ü�� �ı����� �ʵ��� ����
        DontDestroyOnLoad(gameObject);
    }
    #endregion

    // TCP ������ ���� TcpClient ��ü (�������� ������ ���)
    private TcpClient _client;

    // ��Ʈ��ũ ��Ʈ�� (�������� ������ �ۼ����� ���)
    private NetworkStream _stream;

    // ������ �����ϴ� �޼��� (IP �ּҿ� ��Ʈ ��ȣ�� �Ű������� ����)
    public void ConnectToServer(string ip, int port)
    {
        try
        {
            // TcpClient ��ü�� �����Ͽ� �������� ������ �غ�
            _client = new TcpClient();

            // ������ IP�� ��Ʈ ��ȣ�� ���� ����
            _client.Connect(ip, port);

            // ������ �Ϸ�Ǹ� ��Ʈ��ũ ��Ʈ���� ������
            _stream = _client.GetStream();

            // ���� ���� �޽����� �α׷� ���
            Debug.Log("Connected to Server");
        }
        catch (SocketException e)
        {
            // ������ ������ �� ���� ���� ó�� (��: ������ ���� ���� ��)
            Debug.LogError("Failed to connect to server: " + e.Message);
            // �߰������� ��õ� ������ ������ ���� �ֽ��ϴ�.
        }
        catch (Exception e)
        {
            // �ٸ� ���ܰ� �߻����� ��� ó��
            Debug.LogError("An error occurred: " + e.Message);
        }
    }


    // ������ �޽����� �����ϴ� �޼���
    public void SendNetworkMessage(string type, string data)
    {
        // ��Ʈ��ũ ��Ʈ���� null�� ��� �������� ����
        if (_stream == null) return;

        // RequestMessage ��ü�� �����Ͽ� ������ ������ ����
        RequestMessage requestMessage = new RequestMessage(type, data);

        // ��ü�� JSON ���ڿ��� ����ȭ
        string jsonMessage = JsonUtility.ToJson(requestMessage);

        byte[] dataBytes = Encoding.UTF8.GetBytes(jsonMessage);
        _stream.Write(dataBytes, 0, dataBytes.Length);

        Debug.Log("Message Sent: " + jsonMessage);
    }


    // �������� ������ �����ϴ� �޼���
    public void Disconnect()
    {
        // ��Ʈ���� ���� �ִٸ� ����
        _stream?.Close();

        // TCP Ŭ���̾�Ʈ ������ ���� �ִٸ� ����
        _client?.Close();

        // ���� ���� �޽����� �α׷� ���
        Debug.Log("Disconnected from Server");
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