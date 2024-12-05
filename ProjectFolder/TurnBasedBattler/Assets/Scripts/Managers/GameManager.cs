using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private void Start()
    {
        // 서버 연결
        NetworkManager.Instance.ConnectToServer("127.0.0.1", 12345);

        // 초기 데이터 요청
        NetworkManager.Instance.SendMessage("RequestInitialData");
    }

    private void OnApplicationQuit()
    {
        NetworkManager.Instance.Disconnect();
    }
}
