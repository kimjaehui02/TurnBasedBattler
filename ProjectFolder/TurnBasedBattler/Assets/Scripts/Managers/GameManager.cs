using System.Collections;
using UnityEditor.PackageManager;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // ���� ���� �� ȣ��Ǵ� �޼���
    private void Start()
    {
        // ���� ����
        // NetworkManager �ν��Ͻ��� ���� ������ ����
        // ���⼭�� ����ȣ��Ʈ(127.0.0.1)�� ��Ʈ 8080���� ������ �õ�
        NetworkManager.Instance.ConnectToServer("127.0.0.1", 8080);

        // ������ ����� �Ŀ� �ʱ� �����͸� ��û�ϵ��� �ڵ带 ����
        StartCoroutine(WaitForConnectionAndRequestData());
    }

    // ���� ������ �Ϸ�Ǿ��� �� �ʱ� �����͸� ��û�ϴ� �ڷ�ƾ
    private IEnumerator WaitForConnectionAndRequestData()
    {
        // ���� ������ �Ϸ�� ������ ��ٸ�
        while (!NetworkManager.Instance.IsConnected())
        {
            yield return null; // ����� ������ ��ٸ�
        }

        // ������ �Ϸ�Ǹ� �ʱ� ������ ��û
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
