using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // 게임 시작 시 호출되는 메서드
    private void Start()
    {
        // 서버 연결
        // NetworkManager 인스턴스를 통해 서버에 연결
        // 여기서는 로컬호스트(127.0.0.1)와 포트 12345로 연결을 시도
        NetworkManager.Instance.ConnectToServer("127.0.0.1", 8080);
        
        
        // 초기 데이터 요청
        // 서버로 "RequestInitialData" 메시지를 보내 초기 데이터를 요청
        NetworkManager.Instance.SendNetworkMessage("PING", "RequestInitialData");
    }

    // 애플리케이션 종료 시 호출되는 메서드
    private void OnApplicationQuit()
    {
        // 서버와의 연결 종료
        // 애플리케이션 종료 시 NetworkManager의 Disconnect 메서드를 호출하여 서버 연결 종료
        NetworkManager.Instance.Disconnect();
    }
}
