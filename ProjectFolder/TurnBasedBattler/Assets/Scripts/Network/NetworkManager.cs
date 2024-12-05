using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    private static NetworkManager _instance;
    public static NetworkManager Instance => _instance;

    private TcpClient _client;
    private NetworkStream _stream;

    private void Awake()
    {
        if (_instance == null) _instance = this;
        else Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    public void ConnectToServer(string ip, int port)
    {
        _client = new TcpClient();
        _client.Connect(ip, port);
        _stream = _client.GetStream();
        Debug.Log("Connected to Server");
    }

    public void SendMessage(string message)
    {
        if (_stream == null) return;

        byte[] data = Encoding.UTF8.GetBytes(message);
        _stream.Write(data, 0, data.Length);
        Debug.Log("Message Sent: " + message);
    }

    public void Disconnect()
    {
        _stream?.Close();
        _client?.Close();
        Debug.Log("Disconnected from Server");
    }
}
