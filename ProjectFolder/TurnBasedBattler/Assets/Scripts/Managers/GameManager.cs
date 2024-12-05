using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private void Start()
    {
        // ���� ����
        NetworkManager.Instance.ConnectToServer("127.0.0.1", 12345);

        // �ʱ� ������ ��û
        NetworkManager.Instance.SendMessage("RequestInitialData");
    }

    private void OnApplicationQuit()
    {
        NetworkManager.Instance.Disconnect();
    }
}
