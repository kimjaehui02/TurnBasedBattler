using UnityEngine;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System;

public class UdpClient : MonoBehaviour
{
    private const string ServerIp = "127.0.0.1";
    private const int ServerPort = 7777;
    private System.Net.Sockets.UdpClient udpClient;
    public GameObject playerPrefab;  // �÷��̾� ������Ʈ�� �������� �����Ϳ��� �巡�� �� ������� ����
    private Dictionary<int, GameObject> playerObjects = new Dictionary<int, GameObject>();
    public int myId = -1;  // �� id, �������� �޾ƿ� ������ ����
    public GameObject myPlayerObject; // �� �÷��̾� ������Ʈ
    private float sendInterval = 0.5f; // ��ǥ ���� ����
    private float timer = 0f;

    [System.Serializable]
    // ���� ������ ���� Ŭ����
    public class ServerResponse
    {
        public string command { get; set; }
        public string data { get; set; }
    }

    [System.Serializable]
    public class ClientRequest
    {
        public int? playerId; // Nullable �ʵ�
        public string action; // �ʵ�� ����
        public float? x;      // Nullable �ʵ�
        public float? y;      // Nullable �ʵ�
    }

    [System.Serializable]
    public class Position
    {
        public float x;
        public float y;
    }

    [System.Serializable]
    public class Player
    {
        public int id;
        public Position position;
    }

    [System.Serializable]
    public class PlayerList
    {
        public List<Player> players;
    }

    void Start()
    {
        // Ŭ���̾�Ʈ�� �����κ��� �ڱ� �ڽ��� ID�� ���� �� �ֵ��� �ʱ�ȭ
        udpClient = new System.Net.Sockets.UdpClient(ServerIp, ServerPort);

        // �������� �ڱ� ID�� �޵��� ��û�ϴ� �ڵ尡 �ʿ��մϴ�.
        RequestMyId();

        ListenForData();
    }

    void Update()
    {
        // �ֱ������� �� �÷��̾� ��ǥ�� ������ ����
        if (myPlayerObject != null)
        {
            timer += Time.deltaTime;
            if (timer >= sendInterval)
            {
                SendMyPosition();
                timer = 0f;
            }
        }
    }

    void RequestMyId()
    {
        var request = new ClientRequest
        {
            action = "GET_ID"
        };

        string json = JsonUtility.ToJson(request);
        Debug.Log($"������ JSON: {json}"); // Ȯ�ο� ���
        byte[] data = Encoding.UTF8.GetBytes(json);
        udpClient.Send(data, data.Length);
    }

    void SendMyPosition()
    {
        // �� �÷��̾��� ��ġ�� ������ ����
        if (myId != -1 && myPlayerObject != null)
        {
            Position myPosition = new Position()
            {
                x = myPlayerObject.transform.position.x,
                y = myPlayerObject.transform.position.y
            };

            Player player = new Player()
            {
                id = myId,
                position = myPosition
            };

            // JSON���� ��ȯ
            string json = JsonUtility.ToJson(player);

            // �α� ��� (�۽� ������)
            Debug.Log("Sending player position: " + json);
            // ������ ����
            byte[] data = Encoding.UTF8.GetBytes(json);
            udpClient.Send(data, data.Length);
        }
    }

    void ListenForData()
    {
        // UDP�� ���� �����͸� ����ؼ� ����
        udpClient.BeginReceive(ReceiveData, null);
    }

    void ReceiveData(IAsyncResult ar)
    {
        // ������ ������ ���� ���� ����
        IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, ServerPort);

        // UDP Ŭ���̾�Ʈ�� ����Ͽ� ������ ����
        byte[] data = udpClient.EndReceive(ar, ref endPoint);

        // ����Ʈ �迭�� UTF-8 ���ڿ��� ��ȯ
        string json = Encoding.UTF8.GetString(data);

        // ������ JSON �����͸� �ֿܼ� ���
        Debug.Log("Received data: " + json);

        // JSON ���ڿ��� ServerResponse ��ü�� ��ȯ
        try
        {
            // JsonUtility�� ����� JSON �����͸� �Ľ�
            ServerResponse response = JsonUtility.FromJson<ServerResponse>(json);

            // 'command'�� ���� ó��
            if (response != null)
            {
                if (response.command == "Assigned ID")
                {
                    // 'Assigned ID'�� ���, data���� ID ����
                    string newPlayerId = response.data;
                    Debug.Log($"New Player ID: {newPlayerId}");

                    // �߰����� ���� �ʿ� �� ó��
                    ProcessAssignedId(newPlayerId);
                }
                else
                {
                    Debug.Log($"Unknown command: {response.command}");
                }
            }
            else
            {
                Debug.Log("Failed to deserialize JSON.");
            }
        }
        catch (Exception ex)
        {
            // ���� ó��: JSON �Ľ� �� ���� �߻� ��
            Debug.LogError($"Error parsing JSON: {ex.Message}");
        }

        // �ٽ� ���� ���
        udpClient.BeginReceive(ReceiveData, null);
    }



    // ���÷�, Assigned ID ó�� �Լ�
    void ProcessAssignedId(string newPlayerId)
    {
        // ���÷� �� �÷��̾� ID�� ó���ϴ� ���� �߰�
        Debug.Log($"Processing new player ID: {newPlayerId}");
    }


    void ProcessPlayerPositions(string json)
    {
        // JsonUtility�� JSON �����͸� �Ľ�
        PlayerList playerList = JsonUtility.FromJson<PlayerList>(json);

        // �ٸ� �÷��̾���� ������Ʈ�� �������� ����
        foreach (var player in playerList.players)
        {
            int playerId = player.id;
            float x = player.position.x;
            float y = player.position.y;

            // �ڱ� �ڽ��� ����
            if (playerId == myId)  // �ڱ� �ڽ��� ID�� ����
                continue;

            if (playerObjects.ContainsKey(playerId))
            {
                // ���� �÷��̾� ��ġ ������Ʈ
                playerObjects[playerId].transform.position = new Vector3(x, y, 0);
            }
            else
            {
                // ���ο� �÷��̾� ������Ʈ ����
                GameObject newPlayerObject = Instantiate(playerPrefab, new Vector3(x, y, 0), Quaternion.identity);
                playerObjects.Add(playerId, newPlayerObject);
            }
        }
    }

    private void OnApplicationQuit()
    {
        // ���ø����̼� ���� �� UDP Ŭ���̾�Ʈ ����
        SafeCloseUdpClient();
    }

    // ����ġ �ʰ� ����� ������ ������ ���� �� �ֵ��� �ϴ� �޼ҵ�
    private void SafeCloseUdpClient()
    {
        try
        {
            // ���� ���� ������ ���� ��ȣ ������
            SendShutdownSignal();

            if (udpClient != null)
            {
                udpClient.Close();
                udpClient.Dispose();  // ���ҽ� ����
                Debug.Log("UDP Client closed successfully.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to close UDP client: {ex.Message}");
        }
    }

    private void SendShutdownSignal()
    {
        Debug.Log("    private void SendShutdownSignal()");
        Debug.Log(myId);
        if (myId != -1)
        {
            // ���� ��ȣ�� ������ ���� Ư���� action�� ����
            var request = new ClientRequest
            {
                playerId = myId,
                action = "SHUTDOWN"  // ������ ���� ��ȣ�� ���� action
            };

            string json = JsonUtility.ToJson(request);
            byte[] data = Encoding.UTF8.GetBytes(json);

            
   
            // ������ ���� ��ȣ ����
            udpClient.Send(data, data.Length);
            Debug.Log("Shutdown signal sent to server.");
        }
    }


    // �������� ID�� ���� �� ȣ���� �Լ�
    public void SetMyId(int id)
    {
        myId = id;
    }

    // �� �÷��̾� ������Ʈ�� ���� (�� �κ��� ����Ƽ���� ȣ��Ǿ�� ��)
    public void SetMyPlayerObject(GameObject playerObject)
    {
        myPlayerObject = playerObject;
    }
}
