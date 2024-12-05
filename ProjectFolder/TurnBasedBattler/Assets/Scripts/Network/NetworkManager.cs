using System.Net.Sockets;  // TCP ��Ʈ��ũ ������ ���� ���Ǵ� ���ӽ����̽�
using System.Text;         // ���ڿ��� ����Ʈ �迭�� ��ȯ�ϱ� ���� ���ӽ����̽�
using UnityEngine;         // Unity ���� ����� ����ϱ� ���� ���ӽ����̽�

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
        // TcpClient ��ü�� �����Ͽ� �������� ������ �غ�
        _client = new TcpClient();

        // ������ IP�� ��Ʈ ��ȣ�� ���� ����
        _client.Connect(ip, port);

        // ������ �Ϸ�Ǹ� ��Ʈ��ũ ��Ʈ���� ������
        _stream = _client.GetStream();

        // ���� ���� �޽����� �α׷� ���
        Debug.Log("Connected to Server");
    }

    // ������ �޽����� �����ϴ� �޼���
    public void SendMessage(string message)
    {
        // ��Ʈ��ũ ��Ʈ���� null�� ��� �������� ����
        if (_stream == null) return;

        // ���ڿ� �޽����� UTF-8 ����Ʈ �迭�� ��ȯ
        byte[] data = Encoding.UTF8.GetBytes(message);

        // ��Ʈ��ũ ��Ʈ���� ���� �����͸� ������ ����
        _stream.Write(data, 0, data.Length);

        // ������ �޽����� �α׷� ���
        Debug.Log("Message Sent: " + message);
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
