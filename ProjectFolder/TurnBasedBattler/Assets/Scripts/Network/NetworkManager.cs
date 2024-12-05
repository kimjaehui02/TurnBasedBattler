using System.Net.Sockets;  // TCP 네트워크 연결을 위해 사용되는 네임스페이스
using System.Text;         // 문자열을 바이트 배열로 변환하기 위한 네임스페이스
using UnityEngine;         // Unity 관련 기능을 사용하기 위한 네임스페이스

public class NetworkManager : MonoBehaviour
{
    #region Singleton Pattern
    // NetworkManager 클래스의 인스턴스를 저장하는 정적 변수 (싱글톤 패턴)
    private static NetworkManager _instance;

    // 외부에서 NetworkManager의 인스턴스에 접근할 수 있게 하는 프로퍼티
    public static NetworkManager Instance => _instance;

    // 이 스크립트가 첫 실행될 때 호출되는 메서드
    private void Awake()
    {
        // Singleton 패턴을 적용하여 NetworkManager가 하나만 존재하도록 보장
        if (_instance == null)
            _instance = this;  // 첫 번째 인스턴스를 설정
        else
            Destroy(gameObject);  // 두 번째 이후 인스턴스는 파괴

        // 씬 전환 시에도 이 객체가 파괴되지 않도록 설정
        DontDestroyOnLoad(gameObject);
    }
    #endregion

    // TCP 연결을 위한 TcpClient 객체 (서버와의 연결을 담당)
    private TcpClient _client;

    // 네트워크 스트림 (서버와의 데이터 송수신을 담당)
    private NetworkStream _stream;

    // 서버에 연결하는 메서드 (IP 주소와 포트 번호를 매개변수로 받음)
    public void ConnectToServer(string ip, int port)
    {
        // TcpClient 객체를 생성하여 서버와의 연결을 준비
        _client = new TcpClient();

        // 서버에 IP와 포트 번호를 통해 연결
        _client.Connect(ip, port);

        // 연결이 완료되면 네트워크 스트림을 가져옴
        _stream = _client.GetStream();

        // 연결 성공 메시지를 로그로 출력
        Debug.Log("Connected to Server");
    }

    // 서버로 메시지를 전송하는 메서드
    public void SendMessage(string message)
    {
        // 네트워크 스트림이 null인 경우 전송하지 않음
        if (_stream == null) return;

        // 문자열 메시지를 UTF-8 바이트 배열로 변환
        byte[] data = Encoding.UTF8.GetBytes(message);

        // 네트워크 스트림을 통해 데이터를 서버로 전송
        _stream.Write(data, 0, data.Length);

        // 전송한 메시지를 로그로 출력
        Debug.Log("Message Sent: " + message);
    }

    // 서버와의 연결을 종료하는 메서드
    public void Disconnect()
    {
        // 스트림이 열려 있다면 닫음
        _stream?.Close();

        // TCP 클라이언트 연결이 열려 있다면 닫음
        _client?.Close();

        // 연결 종료 메시지를 로그로 출력
        Debug.Log("Disconnected from Server");
    }
}
