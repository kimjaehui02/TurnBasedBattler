using System.Collections;
//using UnityEditor.PackageManager;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // ���� ���� �� ȣ��Ǵ� �޼���
    private void Start()
    {
        // ���� ����
        // TcpClientManager �ν��Ͻ��� ���� ������ ����
        // ���⼭�� ����ȣ��Ʈ(127.0.0.1)�� ��Ʈ 8080���� ������ �õ�
        //TcpClientManager.Instance.ConnectToServer("127.0.0.1", 8080);

        //// ������ ����� �Ŀ� �ʱ� �����͸� ��û�ϵ��� �ڵ带 ����
        //StartCoroutine(WaitForConnectionAndRequestData());
    }

    // ���� ������ �Ϸ�Ǿ��� �� �ʱ� �����͸� ��û�ϴ� �ڷ�ƾ
    //private IEnumerator WaitForConnectionAndRequestData()
    //{
    //    // ���� ������ �Ϸ�� ������ ��ٸ�
    //    while (!TcpClientManager.Instance.IsConnected())
    //    {
    //        yield return null; // ����� ������ ��ٸ�
    //    }

    //    // ������ �Ϸ�Ǹ� �ʱ� ������ ��û
    //    TcpClientManager.Instance.SendNetworkMessage("PING", "RequestInitialData");
    //}

    // ���ø����̼� ���� �� ȣ��Ǵ� �޼���
    //private void OnApplicationQuit()
    //{
    //    // �������� ���� ����
    //    // ���ø����̼� ���� �� TcpClientManager�� Disconnect �޼��带 ȣ���Ͽ� ���� ���� ����
    //    TcpClientManager.Instance.Disconnect();
    //}

}
