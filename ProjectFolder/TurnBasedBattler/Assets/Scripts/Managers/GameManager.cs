using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    #region �̱���
    // GameManager�� �ν��Ͻ��� ���� ���� �ʵ�
    private static GameManager _instance;



    // �̱��� �ν��Ͻ��� �����ϴ� ������Ƽ
    public static GameManager Instance
    {
        get
        {
            // �ν��Ͻ��� ���� �������� �ʾҴٸ�, ���� ����
            if (_instance == null)
            {
                // ���� GameManager ��ü�� ������ ���� �����Ͽ� �Ҵ�
                _instance = new GameObject("GameManager").AddComponent<GameManager>();
            }
            return _instance;
        }
    }

    private void SetupSingleton()
    {
        // �ν��Ͻ��� �̹� ������ �� ��ü�� �ı�
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            // �ν��Ͻ��� ������ �� ��ü�� �ν��Ͻ��� ����
            _instance = this;
            DontDestroyOnLoad(gameObject); // �� ��ȯ �ÿ��� ��ü�� �ı����� �ʵ��� ����
        }
    }

    #endregion

    [SerializeField]
    private TcpClientManager tcpMainClientManager;
    [SerializeField]
    private TcpClientManager tcpSubClientManager;
    [SerializeField]
    private UdpClientManager udpClientManager;
    [SerializeField]
    private PlayerInfoManager playerInfoManager;

    public const string ServerIp = "127.0.0.1";  // ���� IP
    public const int ServerPort = 8080;  // ���� ��Ʈ

    public string SubServerIp = "127.0.0.1";  // ���� IP
    public int SubServerPort = 9090;  // ���� ��Ʈ

    public string UdpServerIp = "127.0.0.1";  // ���� IP
    public int UdpServerPort = 9090;  // ���� ��Ʈ

    public void ConnectMainServer()
    {
        tcpMainClientManager.ConnectServer(ServerIp, ServerPort);
    }

    public void ConnectSubServer(string Ip, int tcpPort, int udpPort)
    {
        tcpSubClientManager.ConnectServer(Ip, tcpPort);
        udpClientManager.ConnectServer(Ip, udpPort);
    }

    // PlayerId�� �����ϴ� �Լ�
    public void SetPlayerId(int playerId)
    {
        playerInfoManager.InitializePlayerInfo(playerId);
    }

    // PlayerId�� �������� �Լ�
    public int GetPlayerId()
    {
        return playerInfoManager.GetPlayerId();
    }

    // ���� ���� �� ȣ��Ǵ� �޼���
    private void Start()
    {
        Application.runInBackground = true;
        ConnectMainServer();
    }


    private void Awake()
    {
        SetupSingleton();
    }
}
