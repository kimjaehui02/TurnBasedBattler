using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // ���� ���� �� ȣ��Ǵ� �޼���
    private void Start()
    {
        // ���� ����
        // NetworkManager �ν��Ͻ��� ���� ������ ����
        // ���⼭�� ����ȣ��Ʈ(127.0.0.1)�� ��Ʈ 12345�� ������ �õ�
        NetworkManager.Instance.ConnectToServer("127.0.0.1", 8080);
        
        
        // �ʱ� ������ ��û
        // ������ "RequestInitialData" �޽����� ���� �ʱ� �����͸� ��û
        NetworkManager.Instance.SendNetworkMessage("PING", "RequestInitialData");
    }

    // ���ø����̼� ���� �� ȣ��Ǵ� �޼���
    private void OnApplicationQuit()
    {
        // �������� ���� ����
        // ���ø����̼� ���� �� NetworkManager�� Disconnect �޼��带 ȣ���Ͽ� ���� ���� ����
        NetworkManager.Instance.Disconnect();
    }
}
