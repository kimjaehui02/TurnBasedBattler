using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    #region 싱글톤
    // GameManager의 인스턴스를 위한 정적 필드
    private static GameManager _instance;



    // 싱글톤 인스턴스를 제공하는 프로퍼티
    public static GameManager Instance
    {
        get
        {
            // 인스턴스가 아직 생성되지 않았다면, 새로 생성
            if (_instance == null)
            {
                // 씬에 GameManager 객체가 없으면 새로 생성하여 할당
                _instance = new GameObject("GameManager").AddComponent<GameManager>();
            }
            return _instance;
        }
    }

    private void SetupSingleton()
    {
        // 인스턴스가 이미 있으면 이 객체를 파괴
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            // 인스턴스가 없으면 이 객체를 인스턴스로 설정
            _instance = this;
            DontDestroyOnLoad(gameObject); // 씬 전환 시에도 객체가 파괴되지 않도록 설정
        }
    }

    #endregion


    //public delegate void ServerInfoUpdated(List<ServerInfo> serverInfos);
    //public static event ServerInfoUpdated OnServerInfoUpdated;


    [SerializeField]
    private TcpClientManager tcpMainClientManager;
    [SerializeField]
    private TcpClientManager tcpSubClientManager;
    [SerializeField]
    private UdpClientManager udpClientManager;
    [SerializeField]
    private PlayerInfoManager playerInfoManager;
    [SerializeField]
    private ServerUIManager serverUIManager;

    //[SerializeField]
    public Dictionary<int, GameObject> gameObjects = new ();
    public Dictionary<int, Dictionary<int, GameObject>> gameObjects2 = new ();

    public List<GameObject> ServerObjects;

    public GameObject one;
    public GameObject two;

    public const string ServerIp = "127.0.0.1";  // 서버 IP
    public const int ServerPort = 8080;  // 서버 포트

    public string SubServerIp = "127.0.0.1";  // 서버 IP
    public int SubServerPort = 9090;  // 서버 포트

    public string UdpServerIp = "127.0.0.1";  // 서버 IP
    public int UdpServerPort = 9090;  // 서버 포트

    public void ConnectMainServer()
    {
        tcpMainClientManager.ConnectServer(ServerIp, ServerPort);
    }

    public void ConnectSubServer(string Ip, int tcpPort, int udpPort)
    {
        tcpSubClientManager.ConnectServer(Ip, tcpPort);
        udpClientManager.ConnectServer(Ip, udpPort);
    }

    // PlayerId를 설정하는 함수
    public void SetPlayerId(int playerId)
    {
        playerInfoManager.InitializePlayerInfo(playerId);
    }

    // PlayerId를 가져오는 함수
    public int GetPlayerId()
    {
        return playerInfoManager.GetPlayerId();
    }

    public void UpdateObjects()
    {
        // gameObjects2에 gameObjects를 내 ID와 함께 넣기
        int playerId = GetPlayerId(); // 현재 유저 ID를 가져옴
        if (!gameObjects2.ContainsKey(playerId))
        {
            gameObjects2[playerId] = new Dictionary<int, GameObject>();
        }

        foreach (var obj in gameObjects)
        {
            gameObjects2[playerId][obj.Key] = obj.Value;
        }
    }

    public void SetServerUi(List<ServerInfo> input)
    {


        serverUIManager.UpdateModel(input);
    }

    // 게임 시작 시 호출되는 메서드
    private void Start()
    {
        // 창 크기를 800x600으로 설정
        Screen.SetResolution(800, 600, false); // false는 전체화면을 비활성화
        //Screen.SetResolution(1920, 1080, false); // false는 전체화면을 비활성화
        Application.runInBackground = true;
        gameObjects.Add(0, one);
        gameObjects.Add(1, two);



        ConnectMainServer();


    }


    private void Awake()
    {
        SetupSingleton();
    }
}
