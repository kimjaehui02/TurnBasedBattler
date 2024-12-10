using System.Collections;
//using UnityEditor.PackageManager;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // 게임 시작 시 호출되는 메서드
    private void Start()
    {
        // 서버 연결
        // TcpClientManager 인스턴스를 통해 서버에 연결
        // 여기서는 로컬호스트(127.0.0.1)와 포트 8080으로 연결을 시도
        //TcpClientManager.Instance.ConnectToServer("127.0.0.1", 8080);

        //// 서버에 연결된 후에 초기 데이터를 요청하도록 코드를 변경
        //StartCoroutine(WaitForConnectionAndRequestData());
    }

    // 서버 연결이 완료되었을 때 초기 데이터를 요청하는 코루틴
    //private IEnumerator WaitForConnectionAndRequestData()
    //{
    //    // 서버 연결이 완료될 때까지 기다림
    //    while (!TcpClientManager.Instance.IsConnected())
    //    {
    //        yield return null; // 연결될 때까지 기다림
    //    }

    //    // 연결이 완료되면 초기 데이터 요청
    //    TcpClientManager.Instance.SendNetworkMessage("PING", "RequestInitialData");
    //}

    // 애플리케이션 종료 시 호출되는 메서드
    //private void OnApplicationQuit()
    //{
    //    // 서버와의 연결 종료
    //    // 애플리케이션 종료 시 TcpClientManager의 Disconnect 메서드를 호출하여 서버 연결 종료
    //    TcpClientManager.Instance.Disconnect();
    //}

}
