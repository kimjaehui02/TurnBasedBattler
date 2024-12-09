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
    public GameObject playerPrefab;  // 플레이어 오브젝트의 프리팹을 에디터에서 드래그 앤 드롭으로 설정
    private Dictionary<int, GameObject> playerObjects = new Dictionary<int, GameObject>();
    public int myId = -1;  // 내 id, 서버에서 받아온 값으로 설정
    public GameObject myPlayerObject; // 내 플레이어 오브젝트
    private float sendInterval = 0.5f; // 좌표 전송 간격
    private float timer = 0f;

    [System.Serializable]
    // 서버 응답을 담을 클래스
    public class ServerResponse
    {
        public string command { get; set; }
        public string data { get; set; }
    }

    [System.Serializable]
    public class ClientRequest
    {
        public int? playerId; // Nullable 필드
        public string action; // 필드로 변경
        public float? x;      // Nullable 필드
        public float? y;      // Nullable 필드
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
        // 클라이언트가 서버로부터 자기 자신의 ID를 받을 수 있도록 초기화
        udpClient = new System.Net.Sockets.UdpClient(ServerIp, ServerPort);

        // 서버에서 자기 ID를 받도록 요청하는 코드가 필요합니다.
        RequestMyId();

        ListenForData();
    }

    void Update()
    {
        // 주기적으로 내 플레이어 좌표를 서버에 전송
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
        Debug.Log($"보내는 JSON: {json}"); // 확인용 출력
        byte[] data = Encoding.UTF8.GetBytes(json);
        udpClient.Send(data, data.Length);
    }

    void SendMyPosition()
    {
        // 내 플레이어의 위치를 서버로 전송
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

            // JSON으로 변환
            string json = JsonUtility.ToJson(player);

            // 로그 출력 (송신 데이터)
            Debug.Log("Sending player position: " + json);
            // 서버로 전송
            byte[] data = Encoding.UTF8.GetBytes(json);
            udpClient.Send(data, data.Length);
        }
    }

    void ListenForData()
    {
        // UDP를 통해 데이터를 계속해서 받음
        udpClient.BeginReceive(ReceiveData, null);
    }

    void ReceiveData(IAsyncResult ar)
    {
        // 데이터 수신을 위한 끝점 정의
        IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, ServerPort);

        // UDP 클라이언트를 사용하여 데이터 수신
        byte[] data = udpClient.EndReceive(ar, ref endPoint);

        // 바이트 배열을 UTF-8 문자열로 변환
        string json = Encoding.UTF8.GetString(data);

        // 수신한 JSON 데이터를 콘솔에 출력
        Debug.Log("Received data: " + json);

        // JSON 문자열을 ServerResponse 객체로 변환
        try
        {
            // JsonUtility를 사용해 JSON 데이터를 파싱
            ServerResponse response = JsonUtility.FromJson<ServerResponse>(json);

            // 'command'에 따라 처리
            if (response != null)
            {
                if (response.command == "Assigned ID")
                {
                    // 'Assigned ID'일 경우, data에서 ID 추출
                    string newPlayerId = response.data;
                    Debug.Log($"New Player ID: {newPlayerId}");

                    // 추가적인 로직 필요 시 처리
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
            // 예외 처리: JSON 파싱 중 오류 발생 시
            Debug.LogError($"Error parsing JSON: {ex.Message}");
        }

        // 다시 수신 대기
        udpClient.BeginReceive(ReceiveData, null);
    }



    // 예시로, Assigned ID 처리 함수
    void ProcessAssignedId(string newPlayerId)
    {
        // 예시로 새 플레이어 ID로 처리하는 로직 추가
        Debug.Log($"Processing new player ID: {newPlayerId}");
    }


    void ProcessPlayerPositions(string json)
    {
        // JsonUtility로 JSON 데이터를 파싱
        PlayerList playerList = JsonUtility.FromJson<PlayerList>(json);

        // 다른 플레이어들의 오브젝트를 동적으로 생성
        foreach (var player in playerList.players)
        {
            int playerId = player.id;
            float x = player.position.x;
            float y = player.position.y;

            // 자기 자신은 무시
            if (playerId == myId)  // 자기 자신의 ID는 무시
                continue;

            if (playerObjects.ContainsKey(playerId))
            {
                // 기존 플레이어 위치 업데이트
                playerObjects[playerId].transform.position = new Vector3(x, y, 0);
            }
            else
            {
                // 새로운 플레이어 오브젝트 생성
                GameObject newPlayerObject = Instantiate(playerPrefab, new Vector3(x, y, 0), Quaternion.identity);
                playerObjects.Add(playerId, newPlayerObject);
            }
        }
    }

    private void OnApplicationQuit()
    {
        // 애플리케이션 종료 시 UDP 클라이언트 종료
        SafeCloseUdpClient();
    }

    // 예기치 않게 종료될 때에도 연결을 닫을 수 있도록 하는 메소드
    private void SafeCloseUdpClient()
    {
        try
        {
            // 종료 전에 서버로 종료 신호 보내기
            SendShutdownSignal();

            if (udpClient != null)
            {
                udpClient.Close();
                udpClient.Dispose();  // 리소스 해제
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
            // 종료 신호를 보내기 위해 특별한 action을 설정
            var request = new ClientRequest
            {
                playerId = myId,
                action = "SHUTDOWN"  // 서버에 종료 신호를 보낼 action
            };

            string json = JsonUtility.ToJson(request);
            byte[] data = Encoding.UTF8.GetBytes(json);

            
   
            // 서버로 종료 신호 전송
            udpClient.Send(data, data.Length);
            Debug.Log("Shutdown signal sent to server.");
        }
    }


    // 서버에서 ID를 받은 후 호출할 함수
    public void SetMyId(int id)
    {
        myId = id;
    }

    // 내 플레이어 오브젝트를 설정 (이 부분은 유니티에서 호출되어야 함)
    public void SetMyPlayerObject(GameObject playerObject)
    {
        myPlayerObject = playerObject;
    }
}
