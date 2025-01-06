using System.Collections;
using System.Net.Sockets;
using System.Net;
using System.Text;
using UnityEngine;
using Newtonsoft.Json;
using System;

public class DelayTest : MonoBehaviour
{
    private UdpClient udpClient;
    private IPEndPoint serverEndPoint;
    private const float sendInterval = 0.02f; // ��ǥ ���� ����
    private float lastSendTime = 0f;

    public int testss;

    public GameObject gameObject1;
    public GameObject gameObject2;
    public int ports;

    void Start()
    {
        udpClient = new UdpClient(ports); // Ŭ���̾�Ʈ�� ������ ��Ʈ
        serverEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 12345); // ������ IP�� ��Ʈ

        // ������ �޽��� ����
        SendMessageToServer("�ȳ��ϼ���, ����!");

        // �ڷ�ƾ�� �����Ͽ� �޽����� ����
        StartCoroutine(ReceiveMessages());
    }

    void SendMessageToServer(object messageData)
    {
        // object�� string���� ��ȯ�� �� ����Ʈ �迭�� ��ȯ
        string messageString = JsonConvert.SerializeObject(messageData); // ����: �޽��� ��ü�� ����ȭ
        byte[] data = Encoding.UTF8.GetBytes(messageString);
        udpClient.Send(data, data.Length, serverEndPoint);
        //udpClient.Send(data, data.Length);
        Debug.Log("������ �޽��� ����: " + messageString);
    }

    // �ڷ�ƾ�� ����Ͽ� �����κ��� �޽����� �񵿱������� ����
    IEnumerator ReceiveMessages()
    {
        while (true)
        {
            if (udpClient.Available > 0)
            {
                byte[] data = udpClient.Receive(ref serverEndPoint);
                string message = Encoding.UTF8.GetString(data);
                Debug.Log("�����κ��� ���� �޽���: " + message);

                // �޽����� JSON ��ü�� ��ȯ
                try
                {
                    dynamic jsonMessage = JsonConvert.DeserializeObject(message);

                    // testss ���� ���� ��ġ ������Ʈ
                    if (testss == 0)
                    {
                        // client0�� �����ͷ� gameObject2 ��ġ ������Ʈ
                        gameObject2.transform.position = new Vector3(
                            (float)jsonMessage.client1.x, // ����� Ÿ�� ��ȯ
                            (float)jsonMessage.client1.y, // ����� Ÿ�� ��ȯ
                            (float)jsonMessage.client1.z  // ����� Ÿ�� ��ȯ
                        );
                    }
                    else if (testss == 1)
                    {
                        // client1�� �����ͷ� gameObject1 ��ġ ������Ʈ
                        gameObject1.transform.position = new Vector3(
                            (float)jsonMessage.client0.x, // ����� Ÿ�� ��ȯ
                            (float)jsonMessage.client0.y, // ����� Ÿ�� ��ȯ
                            (float)jsonMessage.client0.z  // ����� Ÿ�� ��ȯ
                        );
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError("JSON �Ľ� ����: " + ex.Message);
                }
            }
            yield return null; // �� �����Ӹ��� ���
        }
    }

    void OnApplicationQuit()
    {
        udpClient.Close();
    }

    private void Update()
    {
        DelayUpdate();
    }

    public void DelayUpdate()
    {
        if (testss == -1)
        {
            return;
        }
        float currentTime = Time.realtimeSinceStartup;
        if (currentTime - lastSendTime >= sendInterval)
        {
            dynamic messagein = null;
            if (testss == 0)
            {
                messagein = new
                {
                    numb = testss,
                    x = gameObject1.transform.position.x,
                    y = gameObject1.transform.position.y,
                    z = gameObject1.transform.position.z
                };

            }
            else
            {
                messagein = new
                {
                    numb = testss,
                    x = gameObject2.transform.position.x,
                    y = gameObject2.transform.position.y,
                    z = gameObject2.transform.position.z
                };
            }

            // �޽����� ����ȭ�Ͽ� ������ ����
            SendMessageToServer(messagein);

            lastSendTime = currentTime;
        }
    }
}
